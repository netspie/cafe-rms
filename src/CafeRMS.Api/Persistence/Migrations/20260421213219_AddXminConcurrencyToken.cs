using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeRMS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddXminConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // xmin is a PostgreSQL system column that already exists on every table.
            // The model snapshot configures it as a concurrency token so EF appends
            // `AND xmin = @current` to UPDATE statements — no DDL required.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
