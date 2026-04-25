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

            // xmin shadow concurrency tokens are now tracked on every IAuditable entity
            // (broadened from ISoftDeletable). Postgres provides xmin as a system column on
            // every table — Npgsql maps to it directly, so no AddColumn DDL is needed for the
            // 6 entities that gained tracking here (event_days, loyalty_point_logs, order_lines,
            // product_images, product_prices, user_settings).
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
