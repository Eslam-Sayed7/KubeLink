using ShortenerService.Base;
using ShortenerService.Serivces;

namespace ShortenerService.Extensions;

public static class ServiceRegisterExtensions
{
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUrlSerivce, UrlService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}