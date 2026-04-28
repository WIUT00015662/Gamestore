using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamestore.DAL.Migrations;

public partial class RemoveGameLegacyPricingAndSoftDelete : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Discount",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "Price",
            table: "Games");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Discount",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Games",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<double>(
            name: "Price",
            table: "Games",
            type: "float",
            nullable: false,
            defaultValue: 0.0);
    }
}
