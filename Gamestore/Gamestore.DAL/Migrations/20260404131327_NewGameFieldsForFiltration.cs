using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamestore.DAL.Migrations;

/// <inheritdoc />
public partial class NewGameFieldsForFiltration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "PublishDate",
            table: "Games",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<int>(
            name: "ViewCount",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PublishDate",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "ViewCount",
            table: "Games");
    }
}
