// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using Dotnetydd.OtlpDevDashboard.Otlp.Model;
using Dotnetydd.OtlpDevDashboard.Otlp.Storage;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace Dotnetydd.OtlpDevDashboard.Otlp.Grpc;

public class OtlpTraceService : TraceService.TraceServiceBase
{
    private readonly ILogger<OtlpTraceService> _logger;
    private readonly TelemetryRepository _telemetryRepository;

    public OtlpTraceService(ILogger<OtlpTraceService> logger, TelemetryRepository telemetryRepository)
    {
        _logger = logger;
        _telemetryRepository = telemetryRepository;
    }

    public override Task<ExportTraceServiceResponse> Export(ExportTraceServiceRequest request, ServerCallContext context)
    {
        var addContext = new AddContext();
        _telemetryRepository.AddTraces(addContext, request.ResourceSpans);
        _logger.LogInformation("Received {SpanCount} spans, {FailureCount} failed.", request.ResourceSpans.Count, addContext.FailureCount);
        return Task.FromResult(new ExportTraceServiceResponse
        {
            PartialSuccess = new ExportTracePartialSuccess
            {
                RejectedSpans = addContext.FailureCount
            }
        });
    }
}
