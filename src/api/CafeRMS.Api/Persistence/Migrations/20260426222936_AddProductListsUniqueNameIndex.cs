using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductListsUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_product_lists_company_id",
                table: "product_lists");

            migrationBuilder.CreateIndex(
                name: "ix_product_lists_company_id_name",
                table: "product_lists",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_product_lists_company_id_name",
                table: "product_lists");

            migrationBuilder.CreateIndex(
                name: "ix_product_lists_company_id",
                table: "product_lists",
                column: "company_id");
        }
    }
}
