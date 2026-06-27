using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentTracker.Migrations
{
    /// <inheritdoc />
    public partial class FixIsActiveDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The previous migration added IsActive with defaultValue: false,
            // which incorrectly set all existing users to inactive.
            // This migration corrects that by activating every user that hasn't
            // been intentionally deactivated (i.e. all of them, since the feature is new).
            migrationBuilder.Sql(@"UPDATE ""Users"" SET ""IsActive"" = true WHERE ""IsActive"" = false;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""Users"" SET ""IsActive"" = false WHERE ""IsActive"" = true;");
        }
    }
}
