using Microsoft.EntityFrameworkCore;
using ShortenerService.Config;

namespace ShortenerService.Extensions {

    public static class MigrateExtention {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using ShortnerDbContext context = scope.ServiceProvider.GetRequiredService<ShortnerDbContext>();

            context.Database.Migrate();
        }
    }
}