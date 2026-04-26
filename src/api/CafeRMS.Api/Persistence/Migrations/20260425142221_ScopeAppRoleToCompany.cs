using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScopeAppRoleToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "asp_net_roles");

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "asp_net_roles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_roles_company_id_normalized_name",
                table: "asp_net_roles",
                columns: new[] { "company_id", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "asp_net_roles",
                column: "normalized_name");

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_roles_companies_company_id",
                table: "asp_net_roles",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_roles_companies_company_id",
                table: "asp_net_roles");

            migrationBuilder.DropIndex(
                name: "ix_asp_net_roles_company_id_normalized_name",
                table: "asp_net_roles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "asp_net_roles");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "asp_net_roles");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "asp_net_roles",
                column: "normalized_name",
                unique: true);
        }
    }
}
