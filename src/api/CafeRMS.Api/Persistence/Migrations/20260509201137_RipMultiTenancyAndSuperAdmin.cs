using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RipMultiTenancyAndSuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_allergens_companies_company_id",
                table: "allergens");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_roles_companies_company_id",
                table: "asp_net_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_users_companies_company_id",
                table: "asp_net_users");

            migrationBuilder.DropForeignKey(
                name: "fk_events_companies_company_id",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "fk_loyalty_point_logs_companies_company_id",
                table: "loyalty_point_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_modifier_groups_companies_company_id",
                table: "modifier_groups");

            migrationBuilder.DropForeignKey(
                name: "fk_modifiers_companies_company_id",
                table: "modifiers");

            migrationBuilder.DropForeignKey(
                name: "fk_outlets_companies_company_id",
                table: "outlets");

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

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropIndex(
                name: "ix_tax_rates_company_id_name",
                table: "tax_rates");

            migrationBuilder.DropIndex(
                name: "ix_tags_company_id_name",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "ix_tables_company_id_name",
                table: "tables");

            migrationBuilder.DropIndex(
                name: "ix_sales_channels_company_id_name",
                table: "sales_channels");

            migrationBuilder.DropIndex(
                name: "ix_promotion_codes_company_id_code",
                table: "promotion_codes");

            migrationBuilder.DropIndex(
                name: "ix_products_company_id_barcode",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_company_id_name",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_product_lists_company_id_name",
                table: "product_lists");

            migrationBuilder.DropIndex(
                name: "ix_printout_templates_company_id_name",
                table: "printout_templates");

            migrationBuilder.DropIndex(
                name: "ix_price_groups_company_id_name",
                table: "price_groups");

            migrationBuilder.DropIndex(
                name: "ix_outlets_company_id",
                table: "outlets");

            migrationBuilder.DropIndex(
                name: "ix_modifiers_company_id_group_id_name",
                table: "modifiers");

            migrationBuilder.DropIndex(
                name: "ix_modifiers_modifier_group_id",
                table: "modifiers");

            migrationBuilder.DropIndex(
                name: "ix_modifier_groups_company_id_name",
                table: "modifier_groups");

            migrationBuilder.DropIndex(
                name: "ix_loyalty_point_logs_company_id",
                table: "loyalty_point_logs");

            migrationBuilder.DropIndex(
                name: "ix_events_company_id",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_asp_net_users_company_id",
                table: "asp_net_users");

            migrationBuilder.DropIndex(
                name: "ix_asp_net_roles_company_id_normalized_name",
                table: "asp_net_roles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "asp_net_roles");

            migrationBuilder.DropIndex(
                name: "ix_allergens_company_id_name",
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
                table: "outlets");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "modifiers");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "modifier_groups");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "loyalty_point_logs");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "asp_net_roles");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "allergens");

            migrationBuilder.CreateIndex(
                name: "ix_tax_rates_name",
                table: "tax_rates",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                table: "tags",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tables_name",
                table: "tables",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_sales_channels_name",
                table: "sales_channels",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_codes_code",
                table: "promotion_codes",
                column: "code",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_products_barcode",
                table: "products",
                column: "barcode",
                unique: true,
                filter: "deleted_at IS NULL AND barcode IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_products_name",
                table: "products",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_product_lists_name",
                table: "product_lists",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_printout_templates_name",
                table: "printout_templates",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_price_groups_name",
                table: "price_groups",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_modifiers_group_id_name",
                table: "modifiers",
                columns: new[] { "modifier_group_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_modifier_groups_name",
                table: "modifier_groups",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "asp_net_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_allergens_name",
                table: "allergens",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tax_rates_name",
                table: "tax_rates");

            migrationBuilder.DropIndex(
                name: "ix_tags_name",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "ix_tables_name",
                table: "tables");

            migrationBuilder.DropIndex(
                name: "ix_sales_channels_name",
                table: "sales_channels");

            migrationBuilder.DropIndex(
                name: "ix_promotion_codes_code",
                table: "promotion_codes");

            migrationBuilder.DropIndex(
                name: "ix_products_barcode",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_name",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_product_lists_name",
                table: "product_lists");

            migrationBuilder.DropIndex(
                name: "ix_printout_templates_name",
                table: "printout_templates");

            migrationBuilder.DropIndex(
                name: "ix_price_groups_name",
                table: "price_groups");

            migrationBuilder.DropIndex(
                name: "ix_modifiers_group_id_name",
                table: "modifiers");

            migrationBuilder.DropIndex(
                name: "ix_modifier_groups_name",
                table: "modifier_groups");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "asp_net_roles");

            migrationBuilder.DropIndex(
                name: "ix_allergens_name",
                table: "allergens");

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
                table: "outlets",
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
                table: "loyalty_point_logs",
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
                table: "asp_net_users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "asp_net_roles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "allergens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    billing_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    billing_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    invoicing_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tax_rates_company_id_name",
                table: "tax_rates",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tags_company_id_name",
                table: "tags",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tables_company_id_name",
                table: "tables",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_sales_channels_company_id_name",
                table: "sales_channels",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_codes_company_id_code",
                table: "promotion_codes",
                columns: new[] { "company_id", "code" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_products_company_id_barcode",
                table: "products",
                columns: new[] { "company_id", "barcode" },
                unique: true,
                filter: "deleted_at IS NULL AND barcode IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_products_company_id_name",
                table: "products",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_product_lists_company_id_name",
                table: "product_lists",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_printout_templates_company_id_name",
                table: "printout_templates",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_price_groups_company_id_name",
                table: "price_groups",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_outlets_company_id",
                table: "outlets",
                column: "company_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_modifiers_company_id_group_id_name",
                table: "modifiers",
                columns: new[] { "company_id", "modifier_group_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_modifiers_modifier_group_id",
                table: "modifiers",
                column: "modifier_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_modifier_groups_company_id_name",
                table: "modifier_groups",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_point_logs_company_id",
                table: "loyalty_point_logs",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_company_id",
                table: "events",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_company_id",
                table: "asp_net_users",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_roles_company_id_normalized_name",
                table: "asp_net_roles",
                columns: new[] { "company_id", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "asp_net_roles",
                column: "normalized_name");

            migrationBuilder.CreateIndex(
                name: "ix_allergens_company_id_name",
                table: "allergens",
                columns: new[] { "company_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_companies_tax_id",
                table: "companies",
                column: "tax_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_allergens_companies_company_id",
                table: "allergens",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_roles_companies_company_id",
                table: "asp_net_roles",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_users_companies_company_id",
                table: "asp_net_users",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_events_companies_company_id",
                table: "events",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_loyalty_point_logs_companies_company_id",
                table: "loyalty_point_logs",
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
                name: "fk_outlets_companies_company_id",
                table: "outlets",
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
    }
}
