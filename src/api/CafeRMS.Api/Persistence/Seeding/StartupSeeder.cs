namespace CafeRMS.Api.Persistence.Seeding;

public class StartupSeeder(MameDemoSeeder mameDemoSeeder)
{
    public async Task SeedAsync()
    {
        await mameDemoSeeder.SeedAsync();
    }
}
