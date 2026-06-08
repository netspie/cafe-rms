using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutletLegalBillingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "billing_email",
                table: "outlets",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "billing_phone",
                table: "outlets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "invoicing_address",
                table: "outlets",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "legal_name",
                table: "outlets",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tax_id",
                table: "outlets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billing_email",
                table: "outlets");

            migrationBuilder.DropColumn(
                name: "billing_phone",
                table: "outlets");

            migrationBuilder.DropColumn(
                name: "invoicing_address",
                table: "outlets");

            migrationBuilder.DropColumn(
                name: "legal_name",
                table: "outlets");

            migrationBuilder.DropColumn(
                name: "tax_id",
                table: "outlets");
        }
    }
}
