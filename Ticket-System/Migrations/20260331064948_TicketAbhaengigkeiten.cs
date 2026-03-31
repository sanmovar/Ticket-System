using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket_System.Migrations
{
    /// <inheritdoc />
    public partial class TicketAbhaengigkeiten : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketAbhaengigkeit",
                columns: table => new
                {
                    BlockiertesTicketId = table.Column<int>(type: "int", nullable: false),
                    BlockierendesTicketId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAbhaengigkeit", x => new { x.BlockiertesTicketId, x.BlockierendesTicketId });
                    table.ForeignKey(
                        name: "FK_TicketAbhaengigkeit_Tickets_BlockierendesTicketId",
                        column: x => x.BlockierendesTicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketAbhaengigkeit_Tickets_BlockiertesTicketId",
                        column: x => x.BlockiertesTicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketAbhaengigkeit_BlockierendesTicketId",
                table: "TicketAbhaengigkeit",
                column: "BlockierendesTicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketAbhaengigkeit");
        }
    }
}
