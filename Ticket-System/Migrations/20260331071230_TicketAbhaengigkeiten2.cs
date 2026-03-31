using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket_System.Migrations
{
    /// <inheritdoc />
    public partial class TicketAbhaengigkeiten2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAbhaengigkeit_Tickets_BlockierendesTicketId",
                table: "TicketAbhaengigkeit");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketAbhaengigkeit_Tickets_BlockiertesTicketId",
                table: "TicketAbhaengigkeit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketAbhaengigkeit",
                table: "TicketAbhaengigkeit");

            migrationBuilder.RenameTable(
                name: "TicketAbhaengigkeit",
                newName: "TicketAbhaengigkeiten");

            migrationBuilder.RenameIndex(
                name: "IX_TicketAbhaengigkeit_BlockierendesTicketId",
                table: "TicketAbhaengigkeiten",
                newName: "IX_TicketAbhaengigkeiten_BlockierendesTicketId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketAbhaengigkeiten",
                table: "TicketAbhaengigkeiten",
                columns: new[] { "BlockiertesTicketId", "BlockierendesTicketId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAbhaengigkeiten_Tickets_BlockierendesTicketId",
                table: "TicketAbhaengigkeiten",
                column: "BlockierendesTicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAbhaengigkeiten_Tickets_BlockiertesTicketId",
                table: "TicketAbhaengigkeiten",
                column: "BlockiertesTicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAbhaengigkeiten_Tickets_BlockierendesTicketId",
                table: "TicketAbhaengigkeiten");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketAbhaengigkeiten_Tickets_BlockiertesTicketId",
                table: "TicketAbhaengigkeiten");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketAbhaengigkeiten",
                table: "TicketAbhaengigkeiten");

            migrationBuilder.RenameTable(
                name: "TicketAbhaengigkeiten",
                newName: "TicketAbhaengigkeit");

            migrationBuilder.RenameIndex(
                name: "IX_TicketAbhaengigkeiten_BlockierendesTicketId",
                table: "TicketAbhaengigkeit",
                newName: "IX_TicketAbhaengigkeit_BlockierendesTicketId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketAbhaengigkeit",
                table: "TicketAbhaengigkeit",
                columns: new[] { "BlockiertesTicketId", "BlockierendesTicketId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAbhaengigkeit_Tickets_BlockierendesTicketId",
                table: "TicketAbhaengigkeit",
                column: "BlockierendesTicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAbhaengigkeit_Tickets_BlockiertesTicketId",
                table: "TicketAbhaengigkeit",
                column: "BlockiertesTicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
