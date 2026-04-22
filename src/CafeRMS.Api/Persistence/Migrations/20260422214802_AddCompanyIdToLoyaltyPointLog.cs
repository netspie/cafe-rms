using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToLoyaltyPointLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "loyalty_point_logs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_point_logs_company_id",
                table: "loyalty_point_logs",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "fk_loyalty_point_logs_companies_company_id",
                table: "loyalty_point_logs",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_loyalty_point_logs_companies_company_id",
                table: "loyalty_point_logs");

            migrationBuilder.DropIndex(
                name: "ix_loyalty_point_logs_company_id",
                table: "loyalty_point_logs");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "loyalty_point_logs");
        }
    }
}
