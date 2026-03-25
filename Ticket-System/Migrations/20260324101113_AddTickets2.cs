using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket_System.Migrations
{
    /// <inheritdoc />
    public partial class AddTickets2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ErstellAm",
                table: "Tickets",
                newName: "ErstelltAm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ErstelltAm",
                table: "Tickets",
                newName: "ErstellAm");
        }
    }
}
