using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdFkSweep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_promotion_codes_code",
                table: "promotion_codes");

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "tax_rates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "tags",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "tables",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "sales_channels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "promotion_codes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "product_lists",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "printout_templates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "price_groups",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "modifiers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "modifier_groups",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "events",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "allergens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_tax_rates_company_id",
                table: "tax_rates",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_company_id",
                table: "tags",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_tables_company_id",
                table: "tables",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_channels_company_id",
                table: "sales_channels",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_codes_company_id_code",
                table: "promotion_codes",
                columns: new[] { "company_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_company_id",
                table: "products",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_lists_company_id",
                table: "product_lists",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_printout_templates_company_id",
                table: "printout_templates",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_price_groups_company_id",
                table: "price_groups",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_modifiers_company_id",
                table: "modifiers",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_modifier_groups_company_id",
                table: "modifier_groups",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_company_id",
                table: "events",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_allergens_company_id",
                table: "allergens",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "fk_allergens_companies_company_id",
                table: "allergens",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_events_companies_company_id",
                table: "events",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_modifier_groups_companies_company_id",
                table: "modifier_groups",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_modifiers_companies_company_id",
                table: "modifiers",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_price_groups_companies_company_id",
                table: "price_groups",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_printout_templates_companies_company_id",
                table: "printout_templates",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_product_lists_companies_company_id",
                table: "product_lists",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_products_companies_company_id",
                table: "products",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_promotion_codes_companies_company_id",
                table: "promotion_codes",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sales_channels_companies_company_id",
                table: "sales_channels",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tables_companies_company_id",
                table: "tables",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tags_companies_company_id",
                table: "tags",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tax_rates_companies_company_id",
                table: "tax_rates",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_allergens_companies_company_id",
                table: "allergens");

            migrationBuilder.DropForeignKey(
                name: "fk_events_companies_company_id",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "fk_modifier_groups_companies_company_id",
                table: "modifier_groups");

            migrationBuilder.DropForeignKey(
                name: "fk_modifiers_companies_company_id",
                table: "modifiers");

            migrationBuilder.DropForeignKey(
                name: "fk_price_groups_companies_company_id",
                table: "price_groups");

            migrationBuilder.DropForeignKey(
                name: "fk_printout_templates_companies_company_id",
                table: "printout_templates");

            migrationBuilder.DropForeignKey(
                name: "fk_product_lists_companies_company_id",
                table: "product_lists");

            migrationBuilder.DropForeignKey(
                name: "fk_products_companies_company_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "fk_promotion_codes_companies_company_id",
                table: "promotion_codes");

            migrationBuilder.DropForeignKey(
                name: "fk_sales_channels_companies_company_id",
                table: "sales_channels");

            migrationBuilder.DropForeignKey(
                name: "fk_tables_companies_company_id",
                table: "tables");

            migrationBuilder.DropForeignKey(
                name: "fk_tags_companies_company_id",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "fk_tax_rates_companies_company_id",
                table: "tax_rates");

            migrationBuilder.DropIndex(
                name: "ix_tax_rates_company_id",
                table: "tax_rates");

            migrationBuilder.DropIndex(
                name: "ix_tags_company_id",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "ix_tables_company_id",
                table: "tables");

            migrationBuilder.DropIndex(
                name: "ix_sales_channels_company_id",
                table: "sales_channels");

            migrationBuilder.DropIndex(
                name: "ix_promotion_codes_company_id_code",
                table: "promotion_codes");

            migrationBuilder.DropIndex(
                name: "ix_products_company_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_product_lists_company_id",
                table: "product_lists");

            migrationBuilder.DropIndex(
                name: "ix_printout_templates_company_id",
                table: "printout_templates");

            migrationBuilder.DropIndex(
                name: "ix_price_groups_company_id",
                table: "price_groups");

            migrationBuilder.DropIndex(
                name: "ix_modifiers_company_id",
                table: "modifiers");

            migrationBuilder.DropIndex(
                name: "ix_modifier_groups_company_id",
                table: "modifier_groups");

            migrationBuilder.DropIndex(
                name: "ix_events_company_id",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_allergens_company_id",
                table: "allergens");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "tax_rates");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "tables");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "sales_channels");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "promotion_codes");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "product_lists");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "printout_templates");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "price_groups");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "modifiers");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "modifier_groups");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "allergens");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_codes_code",
                table: "promotion_codes",
                column: "code",
                unique: true);
        }
    }
}
