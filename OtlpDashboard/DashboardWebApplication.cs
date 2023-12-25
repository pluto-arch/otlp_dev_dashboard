// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Dotnetydd.OtlpDevDashboard.Components;
using Dotnetydd.OtlpDevDashboard.Model;
using Dotnetydd.OtlpDevDashboard.Otlp.Grpc;
using Dotnetydd.OtlpDevDashboard.Otlp.Model;
using Dotnetydd.OtlpDevDashboard.Otlp.Storage;
using Google.Protobuf.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Metrics.V1;
using OpenTelemetry.Proto.Resource.V1;

namespace Dotnetydd.OtlpDevDashboard;

public class DashboardWebApplication : IHostedService
{
    private const string DashboardOtlpUrlVariableName = "DOTNET_DASHBOARD_OTLP_ENDPOINT_URL";
    private const string DashboardOtlpUrlDefaultValue = "http://localhost:4317";
    private const string DashboardUrlVariableName = "ASPNETCORE_URLSddddd";
    private const string DashboardUrlDefaultValue = "http://localhost:18888";

    private readonly bool _isAllHttps;
    private readonly WebApplication _app;
    private readonly ILogger<DashboardWebApplication> _logger;

    public DashboardWebApplication(ILogger<DashboardWebApplication> logger, Action<IServiceCollection> configureServices)
    {
        _logger = logger;
        var builder = WebApplication.CreateBuilder();
        builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Error);

        var dashboardUris = GetAddressUris(DashboardUrlVariableName, DashboardUrlDefaultValue);

        if (dashboardUris.FirstOrDefault() is { } reportedDashboardUri)
        {
            // dotnet watch needs the trailing slash removed. See https://github.com/dotnet/sdk/issues/36709
            _logger.LogInformation("Now listening on: {dashboardUri}", reportedDashboardUri.AbsoluteUri.TrimEnd('/'));
        }

        var dashboardHttpsPort = dashboardUris.FirstOrDefault(IsHttps)?.Port;
        var otlpUris = GetAddressUris(DashboardOtlpUrlVariableName, DashboardOtlpUrlDefaultValue);

        if (otlpUris.Length > 1)
        {
            throw new InvalidOperationException("Only one URL for Aspire dashboard OTLP endpoint is supported.");
        }

        if (otlpUris.FirstOrDefault() is { } reportedOtlpUri)
        {
            // dotnet watch needs the trailing slash removed. See https://github.com/dotnet/sdk/issues/36709. Conform to dashboard URL format above
            _logger.LogInformation("OTLP server running at: {dashboardUri}", reportedOtlpUri.AbsoluteUri.TrimEnd('/'));
        }

        _isAllHttps = dashboardHttpsPort is not null && IsHttps(otlpUris[0]);

        builder.WebHost.ConfigureKestrel(kestrelOptions =>
        {
            ConfigureListenAddresses(kestrelOptions, dashboardUris);
            ConfigureListenAddresses(kestrelOptions, otlpUris, HttpProtocols.Http2);
        });

        if (!builder.Environment.IsDevelopment())
        {
            // This is set up automatically by the DefaultBuilder when IsDevelopment is true
            // But since this gets packaged up and used in another app, we need it to look for
            // static assets on disk as if it were at development time even when it is not
            builder.WebHost.UseStaticWebAssets();
        }

        if (_isAllHttps)
        {
            // Explicitly configure the HTTPS redirect port as we're possibly listening on multiple HTTPS addresses
            // if the dashboard OTLP URL is configured to use HTTPS too
            builder.Services.Configure<HttpsRedirectionOptions>(options => options.HttpsPort = dashboardHttpsPort);
        }

        // Add services to the container.
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();

        // OTLP services.
        builder.Services.AddGrpc();
        builder.Services.AddSingleton<TelemetryRepository>();
        builder.Services.AddTransient<StructuredLogsViewModel>();
        builder.Services.AddTransient<TracesViewModel>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IOutgoingPeerResolver, ResourceOutgoingPeerResolver>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IOutgoingPeerResolver, BrowserLinkOutgoingPeerResolver>());

        builder.Services.AddFluentUIComponents();

        builder.Services.AddSingleton<ThemeManager>();

        configureServices(builder.Services);
        builder.Services.AddLocalization();

        _app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!_app.Environment.IsDevelopment())
        {
            _app.UseExceptionHandler("/Error");
        }

        if (_isAllHttps)
        {
            _app.UseHttpsRedirection();
        }

        _app.UseStaticFiles(new StaticFileOptions()
        {
            OnPrepareResponse = (context) =>
            {
                // If Cache-Control isn't already set to something, set it to 'no-cache' so that the
                // ETag and Last-Modified headers will be respected by the browser.
                // This may be able to be removed if https://github.com/dotnet/aspnetcore/issues/44153
                // is fixed to make this the default
                if (context.Context.Response.Headers.CacheControl.Count == 0)
                {
                    context.Context.Response.Headers.CacheControl = "no-cache";
                }
            }
        });

        _app.UseAuthorization();

        _app.UseAntiforgery();

        _app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        // OTLP gRPC services.
        _app.MapGrpcService<OtlpMetricsService>();
        _app.MapGrpcService<OtlpTraceService>();
        _app.MapGrpcService<OtlpLogsService>();
    }

    private static Uri[] GetAddressUris(string variableName, string defaultValue)
    {
        var urls = Environment.GetEnvironmentVariable(variableName) ?? defaultValue;
        try
        {
            return urls.Split(';').Select(url => new Uri(url)).ToArray();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error parsing URIs from environment variable '{variableName}'.", ex);
        }
    }

    private static void ConfigureListenAddresses(KestrelServerOptions kestrelOptions, Uri[] uris, HttpProtocols? httpProtocols = null)
    {
        foreach (var uri in uris)
        {
            if (uri.IsLoopback)
            {
                kestrelOptions.ListenLocalhost(uri.Port, options =>
                {
                    ConfigureListenOptions(options, uri, httpProtocols);
                });
            }
            else
            {
                kestrelOptions.Listen(IPAddress.Parse(uri.Host), uri.Port, options =>
                {
                    ConfigureListenOptions(options, uri, httpProtocols);
                });
            }
        }

        static void ConfigureListenOptions(ListenOptions options, Uri uri, HttpProtocols? httpProtocols)
        {
            if (IsHttps(uri))
            {
                options.UseHttps();
            }
            if (httpProtocols is not null)
            {
                options.Protocols = httpProtocols.Value;
            }
        }
    }

    private static bool IsHttps(Uri uri) => string.Equals(uri.Scheme, "https", StringComparison.Ordinal);
    private static readonly DateTime s_testTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _app.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public static Metric CreateSumMetric(string metricName, DateTime startTime, KeyValuePair<string, string>[]? attributes = null)
    {
        return new Metric
        {
            Name = metricName,
            Description = "Test metric description",
            Unit = "widget",
            Sum = new Sum
            {
                AggregationTemporality = AggregationTemporality.Cumulative,
                IsMonotonic = true,
                DataPoints =
                {
                    CreateNumberPoint(startTime, attributes)
                }
            }
        };
    }
    private static NumberDataPoint CreateNumberPoint(DateTime startTime, KeyValuePair<string, string>[]? attributes = null)
    {
        var point = new NumberDataPoint
        {
            AsInt = 1,
            StartTimeUnixNano = DateTimeToUnixNanoseconds(startTime),
            TimeUnixNano = DateTimeToUnixNanoseconds(startTime)
        };
        if (attributes != null)
        {
            foreach (var attribute in attributes)
            {
                point.Attributes.Add(new KeyValue { Key = attribute.Key, Value = new AnyValue { StringValue = attribute.Value } });
            }
        }

        return point;
    }
    public static ulong DateTimeToUnixNanoseconds(DateTime dateTime)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var timeSinceEpoch = dateTime.ToUniversalTime() - unixEpoch;

        return (ulong)timeSinceEpoch.Ticks * 100;
    }
    public static InstrumentationScope CreateScope(string? name = null)
    {
        return new InstrumentationScope() { Name = name ?? "TestScope" };
    }

    public static Resource CreateResource(string? name = null, string? instanceId = null)
    {
        return new Resource()
        {
            Attributes =
            {
                new KeyValue { Key = "service.name", Value = new AnyValue { StringValue = name ?? "TestService" } },
                new KeyValue { Key = "service.instance.id", Value = new AnyValue { StringValue = instanceId ?? "TestId" } }
            }
        };
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _app.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
