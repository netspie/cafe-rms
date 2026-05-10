using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesPerPeriodFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION fn_sales_per_period(
    p_from        date,
    p_to          date,
    p_granularity text
)
RETURNS TABLE (
    period       date,
    revenue      numeric(18, 2),
    orders_count integer,
    items_sold   integer
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_granularity NOT IN ('day', 'month', 'quarter') THEN
        RAISE EXCEPTION 'granularity must be day, month or quarter — got %', p_granularity;
    END IF;

    RETURN QUERY
    SELECT
        date_trunc(p_granularity, o.closed_at)::date                                 AS period,
        COALESCE(SUM((ol.net_per_one + ol.vat_per_one) * ol.quantity), 0)::numeric(18, 2) AS revenue,
        COUNT(DISTINCT o.id)::integer                                                AS orders_count,
        COALESCE(SUM(ol.quantity), 0)::integer                                       AS items_sold
    FROM orders o
    JOIN order_lines ol ON ol.order_id = o.id
    WHERE o.closed_at IS NOT NULL
      AND o.cancelled_at IS NULL
      AND o.closed_at::date >= p_from
      AND o.closed_at::date <  p_to + INTERVAL '1 day'
    GROUP BY date_trunc(p_granularity, o.closed_at)
    ORDER BY period;
END;
$$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_sales_per_period(date, date, text);");
        }
    }
}
