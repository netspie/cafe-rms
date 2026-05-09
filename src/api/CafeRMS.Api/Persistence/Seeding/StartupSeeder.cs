namespace CafeRMS.Api.Persistence.Seeding;

public class StartupSeeder(YumeyaDemoSeeder yumeyaDemoSeeder)
{
    public async Task SeedAsync()
    {
        await yumeyaDemoSeeder.SeedAsync();
    }
}
