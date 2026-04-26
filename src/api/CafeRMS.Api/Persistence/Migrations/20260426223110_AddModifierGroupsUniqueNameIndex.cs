using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModifierGroupsUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_modifier_groups_company_id",
                table: "modifier_groups");

            migrationBuilder.CreateIndex(
                name: "ix_modifier_groups_company_id_name",
                table: "modifier_groups",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_modifier_groups_company_id_name",
                table: "modifier_groups");

            migrationBuilder.CreateIndex(
                name: "ix_modifier_groups_company_id",
                table: "modifier_groups",
                column: "company_id");
        }
    }
}
