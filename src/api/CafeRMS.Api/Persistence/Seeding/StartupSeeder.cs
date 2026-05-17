namespace CafeRMS.Api.Persistence.Seeding;

public class StartupSeeder(ShibaDemoSeeder shibaDemoSeeder)
{
    public async Task SeedAsync()
    {
        await shibaDemoSeeder.SeedAsync();
    }
}
