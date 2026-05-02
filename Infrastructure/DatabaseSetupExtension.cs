using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Infrastructure;

public static class DatabaseSetupExtension
{
    public static async Task ApplyDatabaseSetupAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var dbContext = services.GetRequiredService<DatabaseContext>();
            await dbContext.Database.MigrateAsync();
            
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            string[] requiredRoles = { "Patient", "Doctor", "Admin" };

            foreach (var role in requiredRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Wystąpił błąd podczas migracji lub seedowania bazy danych.");
        }
    }
}