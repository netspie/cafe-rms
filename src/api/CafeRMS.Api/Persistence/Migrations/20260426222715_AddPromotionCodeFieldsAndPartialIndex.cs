using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionCodeFieldsAndPartialIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_promotion_codes_company_id_code",
                table: "promotion_codes");

            migrationBuilder.AddColumn<int>(
                name: "max_uses",
                table: "promotion_codes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "uses_count",
                table: "promotion_codes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "valid_from",
                table: "promotion_codes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "valid_until",
                table: "promotion_codes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_promotion_codes_company_id_code",
                table: "promotion_codes",
                columns: new[] { "company_id", "code" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_promotion_codes_company_id_code",
                table: "promotion_codes");

            migrationBuilder.DropColumn(
                name: "max_uses",
                table: "promotion_codes");

            migrationBuilder.DropColumn(
                name: "uses_count",
                table: "promotion_codes");

            migrationBuilder.DropColumn(
                name: "valid_from",
                table: "promotion_codes");

            migrationBuilder.DropColumn(
                name: "valid_until",
                table: "promotion_codes");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_codes_company_id_code",
                table: "promotion_codes",
                columns: new[] { "company_id", "code" },
                unique: true);
        }
    }
}
