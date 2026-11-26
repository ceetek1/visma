using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visma.Data.Migrations;

/// <inheritdoc />
public partial class _20261126165807_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Hosts",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Hosts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Visitors",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Company = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Visitors", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Visits",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                VisitorId = table.Column<int>(type: "INTEGER", nullable: false),
                HostId = table.Column<int>(type: "INTEGER", nullable: false),
                Purpose = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                Notes = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                CheckInTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                CheckOutTime = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Visits", x => x.Id);
                table.ForeignKey(
                    name: "FK_Visits_Hosts_HostId",
                    column: x => x.HostId,
                    principalTable: "Hosts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Visits_Visitors_VisitorId",
                    column: x => x.VisitorId,
                    principalTable: "Visitors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Visitors_Email",
            table: "Visitors",
            column: "Email");

        migrationBuilder.CreateIndex(
            name: "IX_Visitors_Phone",
            table: "Visitors",
            column: "Phone",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Visits_CheckInTime",
            table: "Visits",
            column: "CheckInTime");

        migrationBuilder.CreateIndex(
            name: "IX_Visits_HostId",
            table: "Visits",
            column: "HostId");

        migrationBuilder.CreateIndex(
            name: "IX_Visits_VisitorId_CheckOutTime",
            table: "Visits",
            columns: new[] { "VisitorId", "CheckOutTime" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Visits");

        migrationBuilder.DropTable(
            name: "Hosts");

        migrationBuilder.DropTable(
            name: "Visitors");
    }
}
