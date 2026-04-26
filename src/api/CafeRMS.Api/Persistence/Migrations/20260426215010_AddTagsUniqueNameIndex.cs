using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tags_company_id",
                table: "tags");

            migrationBuilder.CreateIndex(
                name: "ix_tags_company_id_name",
                table: "tags",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tags_company_id_name",
                table: "tags");

            migrationBuilder.CreateIndex(
                name: "ix_tags_company_id",
                table: "tags",
                column: "company_id");
        }
    }
}
