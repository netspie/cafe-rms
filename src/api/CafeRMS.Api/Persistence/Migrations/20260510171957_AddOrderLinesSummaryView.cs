using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderLinesSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW vw_order_lines_summary AS
SELECT
    ol.id                                            AS line_id,
    ol.quantity                                      AS quantity,
    ol.net_per_one                                   AS net_per_one,
    ol.vat_per_one                                   AS vat_per_one,
    (ol.net_per_one + ol.vat_per_one) * ol.quantity  AS line_total,
    p.id                                             AS product_id,
    p.name                                           AS product_name,
    o.id                                             AS order_id,
    o.outlet_id                                      AS outlet_id,
    o.created_at                                     AS order_created_at,
    o.closed_at                                      AS order_closed_at,
    o.cancelled_at                                   AS order_cancelled_at,
    CASE
        WHEN o.cancelled_at IS NOT NULL THEN 'Cancelled'
        WHEN o.closed_at    IS NOT NULL THEN 'Closed'
        ELSE 'Placed'
    END                                              AS order_status,
    o.user_id                                        AS user_id,
    NULLIF(TRIM(COALESCE(u.first_name, '') || ' ' || COALESCE(u.last_name, '')), '') AS customer_name,
    u.email                                          AS customer_email
FROM order_lines ol
JOIN orders o   ON o.id = ol.order_id
JOIN products p ON p.id = ol.product_id
LEFT JOIN asp_net_users u ON u.id = o.user_id;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_order_lines_summary;");
        }
    }
}
