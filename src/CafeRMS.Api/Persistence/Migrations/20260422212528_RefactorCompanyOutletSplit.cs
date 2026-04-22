using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCompanyOutletSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outlets_company_id",
                table: "outlets");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "time_zone",
                table: "companies");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "outlets",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "outlets",
                newName: "street_address");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "companies",
                newName: "legal_name");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "companies",
                newName: "invoicing_address");

            migrationBuilder.AddColumn<string>(
                name: "logo_url",
                table: "outlets",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "outlets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "time_zone",
                table: "outlets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "billing_email",
                table: "companies",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "billing_phone",
                table: "companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_outlets_company_id",
                table: "outlets",
                column: "company_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outlets_company_id",
                table: "outlets");

            migrationBuilder.DropColumn(
                name: "logo_url",
                table: "outlets");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "outlets");

            migrationBuilder.DropColumn(
                name: "time_zone",
                table: "outlets");

            migrationBuilder.DropColumn(
                name: "billing_email",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "billing_phone",
                table: "companies");

            migrationBuilder.RenameColumn(
                name: "street_address",
                table: "outlets",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "outlets",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "legal_name",
                table: "companies",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "invoicing_address",
                table: "companies",
                newName: "address");

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "companies",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "time_zone",
                table: "companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_outlets_company_id",
                table: "outlets",
                column: "company_id");
        }
    }
}
