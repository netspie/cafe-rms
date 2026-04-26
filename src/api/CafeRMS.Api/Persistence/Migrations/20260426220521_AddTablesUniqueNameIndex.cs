using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tables_company_id",
                table: "tables");

            migrationBuilder.CreateIndex(
                name: "ix_tables_company_id_name",
                table: "tables",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tables_company_id_name",
                table: "tables");

            migrationBuilder.CreateIndex(
                name: "ix_tables_company_id",
                table: "tables",
                column: "company_id");
        }
    }
}
