using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRatesUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tax_rates_company_id",
                table: "tax_rates");

            migrationBuilder.CreateIndex(
                name: "ix_tax_rates_company_id_name",
                table: "tax_rates",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tax_rates_company_id_name",
                table: "tax_rates");

            migrationBuilder.CreateIndex(
                name: "ix_tax_rates_company_id",
                table: "tax_rates",
                column: "company_id");
        }
    }
}
