using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailVision.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Obstacles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElementId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obstacles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Railways",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElementId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Railways", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElementId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coordinates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    ObstacleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RailwayId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coordinates", x => x.Id);
                    table.CheckConstraint("CK_Coordinates_Latitude", "[Latitude] >= -90 AND [Latitude] <= 90");
                    table.CheckConstraint("CK_Coordinates_Longitude", "[Longitude] >= -180 AND [Longitude] <= 180");
                    table.ForeignKey(
                        name: "FK_Coordinates_Obstacles_ObstacleId",
                        column: x => x.ObstacleId,
                        principalTable: "Obstacles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coordinates_Railways_RailwayId",
                        column: x => x.RailwayId,
                        principalTable: "Railways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coordinates_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coordinates_ObstacleId",
                table: "Coordinates",
                column: "ObstacleId");

            migrationBuilder.CreateIndex(
                name: "IX_Coordinates_RailwayId",
                table: "Coordinates",
                column: "RailwayId");

            migrationBuilder.CreateIndex(
                name: "IX_Coordinates_StationId",
                table: "Coordinates",
                column: "StationId",
                unique: true,
                filter: "[StationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coordinates");

            migrationBuilder.DropTable(
                name: "Obstacles");

            migrationBuilder.DropTable(
                name: "Railways");

            migrationBuilder.DropTable(
                name: "Stations");
        }
    }
}
