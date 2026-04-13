# To-Do: API Generation

Generate the full CafeRMS API. Existing solution (`CafeRMS.Api.sln`) stays — clean it up, don't recreate.

---

## Target folder structure

```
src/CafeRMS.Api/
├── Program.cs
├── CafeRMS.Api.csproj
├── appsettings.json
├── appsettings.Development.json
│
├── Persistence/
│   ├── AppDbContext.cs                          ← extends IdentityDbContext<AppUser, AppRole, Guid>
│   └── Migrations/
│
├── Infrastructure/
│   └── GlobalExceptionHandler.cs
│
├── Shared/
│   ├── ClaimsPrincipalExtensions.cs
│   ├── ValidationFilter.cs
│   └── Errors/
│       ├── DomainException.cs
│       └── NotFoundException.cs
│
├── Features/
│   ├── Auth/
│   │   ├── AppUser.cs                           ← extends IdentityUser<Guid>
│   │   ├── AppRole.cs                           ← extends IdentityRole<Guid>
│   │   ├── Permissions.cs                       ← static class with permission claim constants
│   │   └── UseCases/
│   │       ├── Register.cs                      ← POST /api/auth/register (AllowAnonymous)
│   │       └── Login.cs                         ← POST /api/auth/login (AllowAnonymous)
│   │
│   ├── Companies/
│   │   ├── Company.cs                           ← Name, Address, TaxId, Currency, TimeZone
│   │   ├── CompanyConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddCompany.cs
│   │       ├── GetCompanyById.cs
│   │       ├── GetCompanies.cs
│   │       ├── UpdateCompany.cs
│   │       └── DeleteCompany.cs
│   │
│   ├── Outlets/
│   │   ├── Outlet.cs                            ← Name, Address, CompanyId (FK)
│   │   ├── OutletConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddOutlet.cs
│   │       ├── GetOutletById.cs
│   │       ├── GetOutlets.cs
│   │       ├── UpdateOutlet.cs
│   │       └── DeleteOutlet.cs
│   │
│   ├── Tables/
│   │   ├── Table.cs                             ← Name, OutletId (FK)
│   │   ├── TableConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddTable.cs
│   │       ├── GetTableById.cs
│   │       ├── GetTables.cs
│   │       ├── UpdateTable.cs
│   │       └── DeleteTable.cs
│   │
│   ├── TaxRates/
│   │   ├── TaxRate.cs                           ← Name, Description, Rate
│   │   ├── TaxRateConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddTaxRate.cs
│   │       ├── GetTaxRateById.cs
│   │       ├── GetTaxRates.cs
│   │       ├── UpdateTaxRate.cs
│   │       └── DeleteTaxRate.cs
│   │
│   ├── PriceGroups/
│   │   ├── PriceGroup.cs                        ← Name
│   │   ├── PriceGroupConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddPriceGroup.cs
│   │       ├── GetPriceGroupById.cs
│   │       ├── GetPriceGroups.cs
│   │       ├── UpdatePriceGroup.cs
│   │       └── DeletePriceGroup.cs
│   │
│   ├── SalesChannels/
│   │   ├── SalesChannel.cs                      ← Name, IsTakeout
│   │   ├── SalesChannelConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddSalesChannel.cs
│   │       ├── GetSalesChannelById.cs
│   │       ├── GetSalesChannels.cs
│   │       ├── UpdateSalesChannel.cs
│   │       └── DeleteSalesChannel.cs
│   │
│   ├── Tags/
│   │   ├── Tag.cs                               ← Name, ImageUrl?
│   │   ├── TagConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddTag.cs
│   │       ├── GetTagById.cs
│   │       ├── GetTags.cs
│   │       ├── UpdateTag.cs
│   │       └── DeleteTag.cs
│   │
│   ├── Allergens/
│   │   ├── Allergen.cs                          ← Name
│   │   ├── AllergenConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddAllergen.cs
│   │       ├── GetAllergenById.cs
│   │       ├── GetAllergens.cs
│   │       ├── UpdateAllergen.cs
│   │       └── DeleteAllergen.cs
│   │
│   ├── Products/
│   │   ├── Product.cs                           ← Name, Description?, Barcode?, TaxRateId (FK)
│   │   ├── ProductImage.cs                      ← ProductId (FK), Url
│   │   ├── ProductTag.cs                        ← ProductId + TagId (composite PK)
│   │   ├── ProductAllergen.cs                   ← ProductId + AllergenId (composite PK)
│   │   ├── ProductPrice.cs                      ← ProductId (FK), PriceGroupId (FK), Net
│   │   ├── ProductModifierGroup.cs              ← ProductId + ModifierGroupId (composite PK)
│   │   ├── ProductConfiguration.cs              ← configures ALL product-related entities
│   │   └── UseCases/
│   │       ├── AddProduct.cs
│   │       ├── GetProductById.cs
│   │       ├── GetProducts.cs
│   │       ├── UpdateProduct.cs
│   │       ├── DeleteProduct.cs
│   │       ├── AddTagToProduct.cs
│   │       ├── RemoveTagFromProduct.cs
│   │       ├── AddAllergenToProduct.cs
│   │       ├── RemoveAllergenFromProduct.cs
│   │       ├── AddProductPrice.cs
│   │       ├── RemoveProductPrice.cs
│   │       ├── AddModifierGroupToProduct.cs
│   │       └── RemoveModifierGroupFromProduct.cs
│   │
│   ├── ProductLists/
│   │   ├── ProductList.cs                       ← Name
│   │   ├── ProductListItem.cs                   ← ProductListId + ProductId (composite PK)
│   │   ├── ProductListConfiguration.cs          ← configures ProductList + ProductListItem
│   │   └── UseCases/
│   │       ├── AddProductList.cs
│   │       ├── GetProductListById.cs
│   │       ├── GetProductLists.cs
│   │       ├── UpdateProductList.cs
│   │       ├── DeleteProductList.cs
│   │       ├── AddProductToList.cs
│   │       └── RemoveProductFromList.cs
│   │
│   ├── ModifierGroups/
│   │   ├── ModifierGroup.cs                     ← Name
│   │   ├── ModifierGroupConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddModifierGroup.cs
│   │       ├── GetModifierGroupById.cs
│   │       ├── GetModifierGroups.cs
│   │       ├── UpdateModifierGroup.cs
│   │       └── DeleteModifierGroup.cs
│   │
│   ├── Modifiers/
│   │   ├── Modifier.cs                          ← Name, ModifierGroupId (FK)
│   │   ├── ModifierConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddModifier.cs
│   │       ├── GetModifierById.cs
│   │       ├── GetModifiers.cs
│   │       ├── UpdateModifier.cs
│   │       └── DeleteModifier.cs
│   │
│   ├── PromotionCodes/
│   │   ├── PromotionCode.cs                     ← Code (unique), DiscountPercentage
│   │   ├── PromotionCodeConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddPromotionCode.cs
│   │       ├── GetPromotionCodeById.cs
│   │       ├── GetPromotionCodes.cs
│   │       ├── UpdatePromotionCode.cs
│   │       └── DeletePromotionCode.cs
│   │
│   ├── Orders/
│   │   ├── Order.cs                             ← OutletId (FK), TableId? (FK), SalesChannelId? (FK),
│   │   │                                           UserId? (FK), Status (enum: Open/Paid/Cancelled),
│   │   │                                           Discount, LoyaltyPointsUsed, EventId? (FK)
│   │   ├── OrderLine.cs                         ← OrderId (FK), ProductId (FK), Quantity,
│   │   │                                           QuantityRealized, NetPerOne, VatPerOne
│   │   ├── OrderConfiguration.cs                ← configures Order + OrderLine
│   │   └── UseCases/
│   │       ├── CreateOrder.cs
│   │       ├── GetOrderById.cs
│   │       ├── GetOrders.cs
│   │       ├── AddOrderLine.cs
│   │       └── UpdateOrderStatus.cs
│   │
│   ├── Loyalty/
│   │   ├── LoyaltyPointLog.cs                   ← UserId, Points, Reason?
│   │   ├── LoyaltyPointLogConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddLoyaltyPoints.cs
│   │       └── GetLoyaltyPointLogs.cs
│   │
│   ├── Favorites/
│   │   ├── Favorite.cs                          ← UserId + ProductId (composite PK)
│   │   ├── FavoriteConfiguration.cs
│   │   └── UseCases/
│   │       ├── AddFavorite.cs
│   │       ├── RemoveFavorite.cs
│   │       └── GetFavorites.cs
│   │
│   ├── Events/
│   │   ├── Event.cs                             ← Name, Description?, ImageUrl?, ProductListId? (FK),
│   │   │                                           PriceGroupId? (FK)
│   │   ├── EventDay.cs                          ← EventId (FK), Date
│   │   ├── EventConfiguration.cs                ← configures Event + EventDay
│   │   └── UseCases/
│   │       ├── AddEvent.cs
│   │       ├── GetEventById.cs
│   │       ├── GetEvents.cs
│   │       ├── UpdateEvent.cs
│   │       ├── DeleteEvent.cs
│   │       ├── AddEventDay.cs
│   │       └── RemoveEventDay.cs
│   │
│   └── UserSettings/
│       ├── UserSettings.cs                      ← UserId (FK, unique), Theme, UiSettingsJson
│       ├── UserSettingsConfiguration.cs
│       └── UseCases/
│           ├── GetMySettings.cs
│           └── UpdateMySettings.cs              ← upsert
│
src/CafeRMS.Api.Tests/
    ├── ApiFactory.cs
    └── Features/
        ├── Auth/AuthTests.cs
        ├── Companies/CompanyTests.cs
        ├── Products/ProductTests.cs
        ├── Orders/OrderTests.cs
        └── ...
```

---

## Conventions

- **Audit fields**: `CreatedAt` + `CreatedBy`, `UpdatedAt` + `UpdatedBy`, `DeletedAt` + `DeletedBy` on all real entities
- **Soft delete**: `DeletedAt != null` with EF global query filter, not on join tables
- **Permissions**: claim-based via `asp_net_role_claims`, manageable through admin UI
- **Aggregate configs**: one `*Configuration.cs` per aggregate root, configures child entities too
- **Source of truth for entities**: `Classes.cs` (no OutletArea — Table has OutletId directly)

---

## Table count check

| Area | Tables | Count |
|---|---|---|
| Identity | asp_net_users, asp_net_roles, asp_net_user_roles, asp_net_role_claims | 4 |
| Organization | companies, outlets, tables | 3 |
| Sales config | tax_rates, price_groups, sales_channels | 3 |
| Products | products, product_images, product_tags, product_allergens, product_prices, product_modifier_groups | 6 |
| Product lists | product_lists, product_list_items | 2 |
| Modifiers | modifier_groups, modifiers | 2 |
| Promo | promotion_codes | 1 |
| Orders | orders, order_lines | 2 |
| Loyalty | loyalty_point_logs | 1 |
| Favorites | favorites | 1 |
| Events | events, event_days | 2 |
| Settings | user_settings | 1 |
| **Total** | | **28** |

Pending supervisor answer on whether Identity tables count. Need 2 more if not.

---

## Checklist

### 1. Project setup

- [ ] Add NuGet packages to `CafeRMS.Api.csproj`:
  - EF Core + Npgsql
  - EFCore.NamingConventions (snake_case)
  - FluentValidation
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore
  - Microsoft.AspNetCore.Authentication.JwtBearer
  - Scalar.AspNetCore
- [ ] Remove template boilerplate (WeatherForecast, `Classes.cs`, `exp.txt`, old Feature files)
- [ ] `appsettings.json` — connection string, JWT config (Issuer, Audience, Key)
- [ ] `appsettings.Development.json` — dev connection string

### 2. Infrastructure & shared

- [ ] `Shared/Errors/DomainException.cs`
- [ ] `Shared/Errors/NotFoundException.cs`
- [ ] `Infrastructure/GlobalExceptionHandler.cs`
- [ ] `Shared/ValidationFilter.cs`
- [ ] `Shared/ClaimsPrincipalExtensions.cs`

### 3. Auth setup (Identity + JWT)

- [ ] `Features/Auth/AppUser.cs`
- [ ] `Features/Auth/AppRole.cs`
- [ ] `Features/Auth/Permissions.cs` — claim constants (CanManageProducts, CanViewOrders, etc.)
- [ ] `Persistence/AppDbContext.cs` — IdentityDbContext, exclude unused Identity tables (UserClaims, UserLogins, UserTokens)
- [ ] Identity + JWT config in `Program.cs`

### 4. All entities + EF configurations

All entities get full audit fields:
- `CreatedAt` (DateTimeOffset) + `CreatedBy` (Guid?)
- `UpdatedAt` (DateTimeOffset?) + `UpdatedBy` (Guid?)
- `DeletedAt` (DateTimeOffset?) + `DeletedBy` (Guid?) — soft delete via EF global query filter (`DeletedAt == null`)

Soft delete on all real entities — NOT on join tables (product_tags, product_allergens, product_modifier_groups, product_list_items, favorites).

- [ ] Company + CompanyConfiguration
- [ ] Outlet + OutletConfiguration
- [ ] Table + TableConfiguration
- [ ] TaxRate + TaxRateConfiguration
- [ ] PriceGroup + PriceGroupConfiguration
- [ ] SalesChannel + SalesChannelConfiguration
- [ ] Tag + TagConfiguration
- [ ] Allergen + AllergenConfiguration
- [ ] Product, ProductImage, ProductTag, ProductAllergen, ProductPrice, ProductModifierGroup + ProductConfiguration
- [ ] ProductList, ProductListItem + ProductListConfiguration
- [ ] ModifierGroup + ModifierGroupConfiguration
- [ ] Modifier + ModifierConfiguration
- [ ] PromotionCode + PromotionCodeConfiguration
- [ ] Order, OrderLine + OrderConfiguration
- [ ] LoyaltyPointLog + LoyaltyPointLogConfiguration
- [ ] Favorite + FavoriteConfiguration
- [ ] Event, EventDay + EventConfiguration
- [ ] UserSettings + UserSettingsConfiguration
- [ ] DbSets registered in AppDbContext

### 5. EF migration

- [ ] Initial migration covering all entities + Identity tables
- [ ] Verify migration looks correct
- [ ] `dotnet build` — zero errors

### 6. UML & business scenario diagrams

- [ ] Full class diagram (all entities, one diagram)
- [ ] Database ERD (export from DBeaver after migration)
- [ ] 3 business scenario diagrams (activity/sequence) for closed processes:
  1. **Online ordering** — customer browses menu → selects products + modifiers → applies promo code → uses loyalty points → places order → staff sees order → status updates (open → paid/cancelled) → loyalty points earned
  2. **Product & menu management** — staff creates product → sets tax rate → assigns prices per price group → adds tags/allergens/images → assigns modifiers → adds to product list (menu) → menu visible to customers
  3. **Event management** — staff creates event → adds description/image → links product list & price group → adds event days → customers browse events → order from event menu with event pricing

### 7. Use cases

- [ ] Auth: Register, Login
- [ ] CRUD + filtering + sorting for all entity endpoints
- [ ] Join table management (AddTagToProduct, etc.)
- [ ] Order lifecycle (create → add lines → update status)
- [ ] Loyalty points
- [ ] Favorites
- [ ] User settings (upsert)
- [ ] `Program.cs` — register all routes

### 8. Tests

- [ ] Set up `CafeRMS.Api.Tests` project (NUnit)
- [ ] `ApiFactory.cs`
- [ ] Tests per feature area
- [ ] `dotnet test` — all green

### 9. Reports & printouts

- [ ] DB views / stored procedures / functions for report data
- [ ] Report endpoints with PDF/Excel export (2–3 reports)
- [ ] Word template-based printout generation
- [ ] Admin endpoint for template management
