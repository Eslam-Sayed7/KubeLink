var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// FastAPI-style minimal controllers
app.MapGet("/auth-api/{**catchAll}", async context =>
{
    // Custom logic or just forward
    await context.Response.WriteAsync("Auth route hit");
});

app.MapGet("/redirector-api/{**catchAll}", async context =>
{
    await context.Response.WriteAsync("Redirector route hit");
});

app.MapGet("/shortener-api/{**catchAll}", async context =>
{
    await context.Response.WriteAsync("Shortener route hit");
});

app.MapGet("/analytics-api/{**catchAll}", async context =>
{
    await context.Response.WriteAsync("Analytics route hit");
});

app.MapReverseProxy();

app.UseHttpsRedirection();
app.Run();