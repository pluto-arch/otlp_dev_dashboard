using DemoApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapGet("/", async () =>
{
    using (var httpclient = new HttpClient())
    {
        await httpclient.GetAsync("http://localhost:5049/todo");
    }
    return Results.Ok("aaa");
});

app.Run();
