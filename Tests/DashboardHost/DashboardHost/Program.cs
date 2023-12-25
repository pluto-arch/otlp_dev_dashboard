using Dotnetydd.OtlpDevDashboard;
using Dotnetydd.OtlpDevDashboard.Model;

var builder = WebApplication.CreateBuilder(args);


var ctx = new CancellationTokenSource();
AppDomain.CurrentDomain.ProcessExit += (s, e) =>
{
    if (!ctx.IsCancellationRequested)
    {
        ctx.Cancel();
    }
};

var dashboardLogger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<DashboardWebApplication>>();

var _dashboard = new DashboardWebApplication(dashboardLogger, serviceCollection =>
{
    serviceCollection.AddScoped<IResourceService, ApplicationResourceService>();
});

if (_dashboard!=null)
{
   await  _dashboard.StartAsync(ctx.Token).ConfigureAwait(false);
}

var app = builder.Build();
app.Lifetime.ApplicationStopping.Register(() =>
{
    if (!ctx.IsCancellationRequested)
    {
        ctx.Cancel();
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();
