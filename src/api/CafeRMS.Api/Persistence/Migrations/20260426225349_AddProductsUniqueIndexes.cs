using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductsUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_company_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_product_prices_product_id",
                table: "product_prices");

            migrationBuilder.DropIndex(
                name: "ix_printout_templates_company_id",
                table: "printout_templates");

            migrationBuilder.CreateIndex(
                name: "ix_products_company_id_barcode",
                table: "products",
                columns: new[] { "company_id", "barcode" },
                unique: true,
                filter: "deleted_at IS NULL AND barcode IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_products_company_id_name",
                table: "products",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_product_prices_product_id_price_group_id",
                table: "product_prices",
                columns: new[] { "product_id", "price_group_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_printout_templates_company_id_name",
                table: "printout_templates",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_company_id_barcode",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_company_id_name",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_product_prices_product_id_price_group_id",
                table: "product_prices");

            migrationBuilder.DropIndex(
                name: "ix_printout_templates_company_id_name",
                table: "printout_templates");

            migrationBuilder.CreateIndex(
                name: "ix_products_company_id",
                table: "products",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_prices_product_id",
                table: "product_prices",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_printout_templates_company_id",
                table: "printout_templates",
                column: "company_id");
        }
    }
}
