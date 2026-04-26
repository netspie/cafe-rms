using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModifiersPriceDeltaAndUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_modifiers_company_id",
                table: "modifiers");

            migrationBuilder.AddColumn<decimal>(
                name: "price_delta",
                table: "modifiers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_modifiers_company_id_group_id_name",
                table: "modifiers",
                columns: new[] { "company_id", "modifier_group_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_modifiers_company_id_group_id_name",
                table: "modifiers");

            migrationBuilder.DropColumn(
                name: "price_delta",
                table: "modifiers");

            migrationBuilder.CreateIndex(
                name: "ix_modifiers_company_id",
                table: "modifiers",
                column: "company_id");
        }
    }
}
