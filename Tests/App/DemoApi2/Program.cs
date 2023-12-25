using DemoApi2;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/todo", () => "this is todo api");


app.Run();
