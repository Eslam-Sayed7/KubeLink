using Microsoft.EntityFrameworkCore;
using ShortenerService.Config;

namespace ShortenerService.Extensions;

public static class StorageServiceExtension
{
    public static void RegisterStorageService(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ShortnerDbContext>(x => x.UseNpgsql( 
            builder.Configuration.GetConnectionString("DefaultConnection")
        ));
    }

}