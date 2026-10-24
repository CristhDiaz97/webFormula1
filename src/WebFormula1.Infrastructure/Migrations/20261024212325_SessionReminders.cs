using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFormula1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SessionReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RaceId = table.Column<int>(type: "int", nullable: false),
                    SessionName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    StartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotifyAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sent = table.Column<bool>(type: "bit", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionReminders_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionReminders_RaceId_SessionName",
                table: "SessionReminders",
                columns: new[] { "RaceId", "SessionName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionReminders");
        }
    }
}
