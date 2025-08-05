using ShortenerService.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.RegisterServices();
builder.RegisterStorageService();
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();
