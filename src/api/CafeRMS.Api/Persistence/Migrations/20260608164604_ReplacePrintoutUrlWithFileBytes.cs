using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePrintoutUrlWithFileBytes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "template_file_url",
                table: "printout_templates");

            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "printout_templates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "file_content",
                table: "printout_templates",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "file_name",
                table: "printout_templates",
                type: "character varying(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_type",
                table: "printout_templates");

            migrationBuilder.DropColumn(
                name: "file_content",
                table: "printout_templates");

            migrationBuilder.DropColumn(
                name: "file_name",
                table: "printout_templates");

            migrationBuilder.AddColumn<string>(
                name: "template_file_url",
                table: "printout_templates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
