using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropOldSqlArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_close_stale_orders(integer, integer);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_sales_per_period(date, date, text);");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_order_lines_summary;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
