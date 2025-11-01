using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1861

namespace Gamestore.DAL.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Games",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Games", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Genres",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ParentGenreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Genres", x => x.Id);
                table.ForeignKey(
                    name: "FK_Genres_Genres_ParentGenreId",
                    column: x => x.ParentGenreId,
                    principalTable: "Genres",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Platforms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Platforms", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "GameGenres",
            columns: table => new
            {
                GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                GenreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameGenres", x => new { x.GameId, x.GenreId });
                table.ForeignKey(
                    name: "FK_GameGenres_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_GameGenres_Genres_GenreId",
                    column: x => x.GenreId,
                    principalTable: "Genres",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "GamePlatforms",
            columns: table => new
            {
                GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PlatformId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GamePlatforms", x => new { x.GameId, x.PlatformId });
                table.ForeignKey(
                    name: "FK_GamePlatforms_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_GamePlatforms_Platforms_PlatformId",
                    column: x => x.PlatformId,
                    principalTable: "Platforms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            table: "Genres",
            columns: ["Id", "Name", "ParentGenreId"],
            values: new object[,]
            {
                { new Guid("10c8abd7-0901-4d25-96cd-460ed43b002b"), "Sports", null },
                { new Guid("3fdfda42-5f9f-4f50-90a3-b0f9f4c83297"), "Strategy", null },
                { new Guid("58413a2f-534a-488e-8066-8f1e48611b35"), "Action", null },
                { new Guid("6cc93ce8-8b02-489c-a2c0-5c45e12380cf"), "Races", null },
                { new Guid("e219b857-af30-4f06-97a7-23fca0b061f5"), "RPG", null },
                { new Guid("f426ce2d-3f57-430e-9614-66e0313ba5a1"), "Puzzle & Skill", null },
                { new Guid("fba94f42-23ef-4e20-9871-73c51852517a"), "Adventure", null },
            });

        migrationBuilder.InsertData(
            table: "Platforms",
            columns: ["Id", "Type"],
            values: new object[,]
            {
                { new Guid("40429db0-1439-41aa-a420-73dccbdfdc3d"), "Browser" },
                { new Guid("60dccf95-d657-4ec3-b9d1-01529b4c40d3"), "Mobile" },
                { new Guid("a76d2e28-ae0c-43c1-9e6e-5c75523ddc67"), "Console" },
                { new Guid("fd1e5957-93bc-4a81-a76b-b219b1b76d3f"), "Desktop" },
            });

        migrationBuilder.InsertData(
            table: "Genres",
            columns: ["Id", "Name", "ParentGenreId"],
            values: new object[,]
            {
                { new Guid("08044742-f1b2-4e1e-be70-2c03077e3570"), "Formula", new Guid("6cc93ce8-8b02-489c-a2c0-5c45e12380cf") },
                { new Guid("2e98834a-1196-41e3-93b9-5dcf79c3504f"), "TBS", new Guid("3fdfda42-5f9f-4f50-90a3-b0f9f4c83297") },
                { new Guid("3aea7cbc-edba-40d0-9709-5655f8476ef6"), "TPS", new Guid("58413a2f-534a-488e-8066-8f1e48611b35") },
                { new Guid("52514583-2213-4dee-aabe-febfd2f95846"), "FPS", new Guid("58413a2f-534a-488e-8066-8f1e48611b35") },
                { new Guid("534f0bbc-8e8a-465a-9185-4eead9bb8881"), "Rally", new Guid("6cc93ce8-8b02-489c-a2c0-5c45e12380cf") },
                { new Guid("826e499c-abe9-4f43-a7cc-6e6361b8eaf3"), "Arcade", new Guid("6cc93ce8-8b02-489c-a2c0-5c45e12380cf") },
                { new Guid("da7216d9-61e6-4b5f-989d-faee4a0df753"), "Off-road", new Guid("6cc93ce8-8b02-489c-a2c0-5c45e12380cf") },
                { new Guid("fcdbd9b9-32e0-4190-964f-be636cc1ceb8"), "RTS", new Guid("3fdfda42-5f9f-4f50-90a3-b0f9f4c83297") },
            });

        migrationBuilder.CreateIndex(
            name: "IX_GameGenres_GenreId",
            table: "GameGenres",
            column: "GenreId");

        migrationBuilder.CreateIndex(
            name: "IX_GamePlatforms_PlatformId",
            table: "GamePlatforms",
            column: "PlatformId");

        migrationBuilder.CreateIndex(
            name: "IX_Games_Key",
            table: "Games",
            column: "Key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Genres_Name",
            table: "Genres",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Genres_ParentGenreId",
            table: "Genres",
            column: "ParentGenreId");

        migrationBuilder.CreateIndex(
            name: "IX_Platforms_Type",
            table: "Platforms",
            column: "Type",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "GameGenres");

        migrationBuilder.DropTable(
            name: "GamePlatforms");

        migrationBuilder.DropTable(
            name: "Genres");

        migrationBuilder.DropTable(
            name: "Games");

        migrationBuilder.DropTable(
            name: "Platforms");
    }
}
