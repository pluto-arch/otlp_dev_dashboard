using DemoApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapGet("/", async () =>
{
    using (var httpclient = new HttpClient())
    {
        await httpclient.GetAsync("http://localhost:7891/todo");
    }
    return Results.Ok("aaa");
});

app.Run();
