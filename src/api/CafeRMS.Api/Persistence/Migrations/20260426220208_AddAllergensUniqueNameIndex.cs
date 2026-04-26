using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAllergensUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_allergens_company_id",
                table: "allergens");

            migrationBuilder.CreateIndex(
                name: "ix_allergens_company_id_name",
                table: "allergens",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_allergens_company_id_name",
                table: "allergens");

            migrationBuilder.CreateIndex(
                name: "ix_allergens_company_id",
                table: "allergens",
                column: "company_id");
        }
    }
}
