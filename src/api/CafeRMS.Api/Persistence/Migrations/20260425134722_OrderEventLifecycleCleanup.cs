using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrderEventLifecycleCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "events");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "events",
                newName: "cancelled_at");

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "events",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "events");

            migrationBuilder.RenameColumn(
                name: "cancelled_at",
                table: "events",
                newName: "deleted_at");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "events",
                type: "uuid",
                nullable: true);
        }
    }
}
