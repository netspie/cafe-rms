using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.Auth;

public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        // Identity's default unique index on NormalizedName would prevent two companies
        // from each having an "Owner" role. Downgrade it to non-unique and add a composite
        // (CompanyId, NormalizedName) unique index instead — role names are unique per company.
        builder.HasIndex(x => x.NormalizedName).HasDatabaseName("RoleNameIndex").IsUnique(false);
        builder.HasIndex(x => new { x.CompanyId, x.NormalizedName }).IsUnique();

        builder.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}
