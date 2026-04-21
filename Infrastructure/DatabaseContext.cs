using Medreserve.Features.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Infrastructure;

public class DatabaseContext(IConfiguration configuration) : IdentityDbContext<User>
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            var connectionString = configuration.GetConnectionString("Default");
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
