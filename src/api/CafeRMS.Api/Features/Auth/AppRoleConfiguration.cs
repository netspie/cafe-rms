using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.Auth;

public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        // Identity ships a unique RoleNameIndex on NormalizedName by default — leave it,
        // role names are globally unique now that there's no per-tenant scope.
    }
}
