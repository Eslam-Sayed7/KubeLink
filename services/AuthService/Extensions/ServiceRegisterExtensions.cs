using AuthService.Base;
using AuthService.IServices;
using AuthService.Services;

namespace AuthService.Extensions;

public static class ServiceRegisterExtensions
{
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAuthService, Services.AuthService>();
        builder.Services.AddScoped<IAuthService, Services.AuthService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IUserService, UserService>();
    }
}