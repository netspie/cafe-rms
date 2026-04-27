using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderLifecycleTimestampsAndPromoFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "accepted_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "in_progress_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "promotion_code_id",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ready_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_promotion_code_id",
                table: "orders",
                column: "promotion_code_id");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_promotion_codes_promotion_code_id",
                table: "orders",
                column: "promotion_code_id",
                principalTable: "promotion_codes",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_promotion_codes_promotion_code_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_promotion_code_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "accepted_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "in_progress_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "promotion_code_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ready_at",
                table: "orders");
        }
    }
}
