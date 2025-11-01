using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamestore.DAL.Migrations;

/// <inheritdoc />
public partial class Epic04Enhancements : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Discount",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<double>(
            name: "Price",
            table: "Games",
            type: "float",
            nullable: false,
            defaultValue: 0.0);

        migrationBuilder.AddColumn<Guid>(
            name: "PublisherId",
            table: "Games",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.AddColumn<int>(
            name: "UnitInStock",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "Publishers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CompanyName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                HomePage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Publishers", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Games_PublisherId",
            table: "Games",
            column: "PublisherId");

        migrationBuilder.CreateIndex(
            name: "IX_Publishers_CompanyName",
            table: "Publishers",
            column: "CompanyName",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Games_Publishers_PublisherId",
            table: "Games",
            column: "PublisherId",
            principalTable: "Publishers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Games_Publishers_PublisherId",
            table: "Games");

        migrationBuilder.DropTable(
            name: "Publishers");

        migrationBuilder.DropIndex(
            name: "IX_Games_PublisherId",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "Discount",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "Price",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "PublisherId",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "UnitInStock",
            table: "Games");
    }
}
