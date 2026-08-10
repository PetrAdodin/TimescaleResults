using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TimescaleResults.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    DateRangeSeconds = table.Column<decimal>(type: "numeric", nullable: false),
                    MinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AverageExecutionTime = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageValue = table.Column<decimal>(type: "numeric", nullable: false),
                    MedianValue = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxValue = table.Column<decimal>(type: "numeric", nullable: false),
                    MinValue = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Values",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutionTime = table.Column<decimal>(type: "numeric", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    ResultId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Values_Results_ResultId",
                        column: x => x.ResultId,
                        principalTable: "Results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Results_FileName",
                table: "Results",
                column: "FileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Values_ResultId_Date",
                table: "Values",
                columns: new[] { "ResultId", "Date" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Values");

            migrationBuilder.DropTable(
                name: "Results");
        }
    }
}
