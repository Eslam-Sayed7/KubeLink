using AuthService.Extensions;
using DotNetEnv;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Env.Load(".env");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.RegisterSecurityServices();
// builder.Host.ConfigureLogging();
builder.RegisterStorageService();
builder.RegisterServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddIdentityServices();
builder.Services.AddControllers();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseDefaultFiles();
app.UseAuthentication();
app.ApplyMigrations();
app.UseAuthorization();
app.MapControllers();
app.Run();