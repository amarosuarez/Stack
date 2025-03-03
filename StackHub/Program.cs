using Microsoft.AspNetCore.ResponseCompression;
using StackHub;

var builder = WebApplication.CreateBuilder(args);

// Agregado SignalR
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

var app = builder.Build();

// Agregado SignalR
app.UseResponseCompression();

app.MapGet("/", () => "Hello World!");

// Agregado SignalR
app.MapHub<myHub>("/stackHub");

app.Run();
