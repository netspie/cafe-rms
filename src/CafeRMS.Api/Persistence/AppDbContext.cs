using CafeRMS.Api.Features.Allergens;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Companies;
using CafeRMS.Api.Features.Events;
using CafeRMS.Api.Features.Favorites;
using CafeRMS.Api.Features.Loyalty;
using CafeRMS.Api.Features.ModifierGroups;
using CafeRMS.Api.Features.Modifiers;
using CafeRMS.Api.Features.Orders;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Features.PrintoutTemplates;
using CafeRMS.Api.Features.ProductLists;
using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Features.PromotionCodes;
using CafeRMS.Api.Features.SalesChannels;
using CafeRMS.Api.Features.Tables;
using CafeRMS.Api.Features.Tags;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Features.UserSettings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid>(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Outlet> Outlets => Set<Outlet>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<PriceGroup> PriceGroups => Set<PriceGroup>();
    public DbSet<SalesChannel> SalesChannels => Set<SalesChannel>();
    public DbSet<SalesChannelPriceGroup> SalesChannelPriceGroups => Set<SalesChannelPriceGroup>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Allergen> Allergens => Set<Allergen>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<ProductAllergen> ProductAllergens => Set<ProductAllergen>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<ProductModifierGroup> ProductModifierGroups => Set<ProductModifierGroup>();
    public DbSet<ProductList> ProductLists => Set<ProductList>();
    public DbSet<ProductListItem> ProductListItems => Set<ProductListItem>();
    public DbSet<ModifierGroup> ModifierGroups => Set<ModifierGroup>();
    public DbSet<Modifier> Modifiers => Set<Modifier>();
    public DbSet<PromotionCode> PromotionCodes => Set<PromotionCode>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<LoyaltyPointLog> LoyaltyPointLogs => Set<LoyaltyPointLog>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventDay> EventDays => Set<EventDay>();
    public DbSet<PrintoutTemplate> PrintoutTemplates => Set<PrintoutTemplate>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Exclude unused Identity tables
        builder.Ignore<IdentityUserClaim<Guid>>();
        builder.Ignore<IdentityUserLogin<Guid>>();
        builder.Ignore<IdentityUserToken<Guid>>();

        // Force snake_case on Identity tables (NamingConventions runs after OnModelCreating,
        // but Identity sets explicit table names which override the convention)
        builder.Entity<AppUser>().ToTable("asp_net_users");
        builder.Entity<AppRole>().ToTable("asp_net_roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("asp_net_user_roles");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("asp_net_role_claims");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
