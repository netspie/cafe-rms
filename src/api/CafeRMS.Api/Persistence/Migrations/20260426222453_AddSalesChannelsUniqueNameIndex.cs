using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesChannelsUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sales_channels_company_id",
                table: "sales_channels");

            migrationBuilder.CreateIndex(
                name: "ix_sales_channels_company_id_name",
                table: "sales_channels",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sales_channels_company_id_name",
                table: "sales_channels");

            migrationBuilder.CreateIndex(
                name: "ix_sales_channels_company_id",
                table: "sales_channels",
                column: "company_id");
        }
    }
}
