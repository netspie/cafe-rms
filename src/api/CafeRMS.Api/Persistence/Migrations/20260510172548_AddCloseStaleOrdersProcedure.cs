using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCloseStaleOrdersProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE PROCEDURE sp_close_stale_orders(
    p_stale_after_hours integer,
    INOUT p_closed_count integer DEFAULT 0
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_stale_after_hours IS NULL OR p_stale_after_hours <= 0 THEN
        RAISE EXCEPTION 'p_stale_after_hours must be a positive integer — got %', p_stale_after_hours;
    END IF;

    UPDATE orders
       SET closed_at = now()
     WHERE closed_at IS NULL
       AND cancelled_at IS NULL
       AND created_at < now() - (p_stale_after_hours || ' hours')::interval;

    GET DIAGNOSTICS p_closed_count = ROW_COUNT;
END;
$$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_close_stale_orders(integer, integer);");
        }
    }
}
