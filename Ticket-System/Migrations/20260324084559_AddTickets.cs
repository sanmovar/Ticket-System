using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket_System.Migrations
{
    /// <inheritdoc />
    public partial class AddTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Beschreibung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ProjektId = table.Column<int>(type: "int", nullable: false),
                    ErstellerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ErstellAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ZugewiesenerBenutzerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ZugewiesenAm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeschlossenVonId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GeschlossenAm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_AspNetUsers_ErstellerId",
                        column: x => x.ErstellerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_AspNetUsers_GeschlossenVonId",
                        column: x => x.GeschlossenVonId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_AspNetUsers_ZugewiesenerBenutzerId",
                        column: x => x.ZugewiesenerBenutzerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Projects_ProjektId",
                        column: x => x.ProjektId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ErstellerId",
                table: "Tickets",
                column: "ErstellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_GeschlossenVonId",
                table: "Tickets",
                column: "GeschlossenVonId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ProjektId",
                table: "Tickets",
                column: "ProjektId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ZugewiesenerBenutzerId",
                table: "Tickets",
                column: "ZugewiesenerBenutzerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
