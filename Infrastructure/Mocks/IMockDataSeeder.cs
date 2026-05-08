namespace Medreserve.Infrastructure.Mocks;

public interface IMockDataSeeder
{
    Task SeedAsync(bool reset, CancellationToken cancellationToken = default);
}
