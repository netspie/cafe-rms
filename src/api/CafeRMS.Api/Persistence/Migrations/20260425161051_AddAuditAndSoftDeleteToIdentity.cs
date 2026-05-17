using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDeleteToIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "asp_net_users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "asp_net_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "asp_net_users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "asp_net_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "asp_net_users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "asp_net_roles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "asp_net_roles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "asp_net_roles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "asp_net_roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "asp_net_roles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "asp_net_roles",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "asp_net_roles");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "asp_net_roles");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "asp_net_roles");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "asp_net_roles");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "asp_net_roles");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "asp_net_roles");
        }
    }
}
