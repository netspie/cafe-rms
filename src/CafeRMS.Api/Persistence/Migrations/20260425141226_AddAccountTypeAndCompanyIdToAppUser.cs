using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountTypeAndCompanyIdToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "account_type",
                table: "asp_net_users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "asp_net_users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_company_id",
                table: "asp_net_users",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_users_companies_company_id",
                table: "asp_net_users",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_users_companies_company_id",
                table: "asp_net_users");

            migrationBuilder.DropIndex(
                name: "ix_asp_net_users_company_id",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "account_type",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "asp_net_users");
        }
    }
}
