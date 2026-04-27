using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamestore.DAL.Migrations
{
    /// <inheritdoc />
    public partial class TempSchemaCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentPrice",
                table: "GameVendorOffers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TruePrice",
                table: "GameVendorOffers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsNewDiscount",
                table: "GameDiscountSnapshots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DiscountConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscountProbability = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    DiscountPercentageMin = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DiscountPercentageMax = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TimeWindowMinutes = table.Column<int>(type: "int", nullable: false),
                    DiscountRevertProbability = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameDiscounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameVendorOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsCurrentlyActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevertedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameDiscounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameDiscounts_GameVendorOffers_GameVendorOfferId",
                        column: x => x.GameVendorOfferId,
                        principalTable: "GameVendorOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PollingRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedOffersCount = table.Column<int>(type: "int", nullable: false),
                    NewDiscountsCreated = table.Column<int>(type: "int", nullable: false),
                    DiscountsReverted = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollingRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameDiscounts_GameVendorOfferId",
                table: "GameDiscounts",
                column: "GameVendorOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PollingRuns_RunAt",
                table: "PollingRuns",
                column: "RunAt",
                descending: Array.Empty<bool>());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountConfigurations");

            migrationBuilder.DropTable(
                name: "GameDiscounts");

            migrationBuilder.DropTable(
                name: "PollingRuns");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                table: "GameVendorOffers");

            migrationBuilder.DropColumn(
                name: "TruePrice",
                table: "GameVendorOffers");

            migrationBuilder.DropColumn(
                name: "IsNewDiscount",
                table: "GameDiscountSnapshots");
        }
    }
}
