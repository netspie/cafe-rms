using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceGroupsUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_price_groups_company_id",
                table: "price_groups");

            migrationBuilder.CreateIndex(
                name: "ix_price_groups_company_id_name",
                table: "price_groups",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_price_groups_company_id_name",
                table: "price_groups");

            migrationBuilder.CreateIndex(
                name: "ix_price_groups_company_id",
                table: "price_groups",
                column: "company_id");
        }
    }
}
