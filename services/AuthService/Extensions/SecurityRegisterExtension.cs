using System.Text;
using AuthService.Entities;
using AuthService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Extensions;

public static class SecurityRegisterExtension
{
    public static void RegisterSecurityServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JWT>(options =>
        {
            options.Key = Environment.GetEnvironmentVariable("JWT__KEY");
            options.Issuer = Environment.GetEnvironmentVariable("JWT__ISSUER");
            options.Audience = Environment.GetEnvironmentVariable("JWT__AUDIENCE");
            options.DurationInMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT__DURATION") ?? "1");
        });

        builder.Services.AddIdentity<AppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<AppDbContext>()
            .AddRoles<IdentityRole>()
            .AddApiEndpoints();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("JWT__ISSUER"),
                    ValidAudience = Environment.GetEnvironmentVariable("JWT__AUDIENCE"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__KEY"))),
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("User", policy => policy.RequireRole("User"));
        });
    }
}