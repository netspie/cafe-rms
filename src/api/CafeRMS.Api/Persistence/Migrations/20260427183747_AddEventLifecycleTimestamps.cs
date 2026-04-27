using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventLifecycleTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_event_days_event_id",
                table: "event_days");

            migrationBuilder.AlterColumn<string>(
                name: "cancellation_reason",
                table: "events",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "closed_at",
                table: "events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "published_at",
                table: "events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_days_event_id_date",
                table: "event_days",
                columns: new[] { "event_id", "date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_event_days_event_id_date",
                table: "event_days");

            migrationBuilder.DropColumn(
                name: "closed_at",
                table: "events");

            migrationBuilder.DropColumn(
                name: "published_at",
                table: "events");

            migrationBuilder.AlterColumn<string>(
                name: "cancellation_reason",
                table: "events",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_days_event_id",
                table: "event_days",
                column: "event_id");
        }
    }
}
