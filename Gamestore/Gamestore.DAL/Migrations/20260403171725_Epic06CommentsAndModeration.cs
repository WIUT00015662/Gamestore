#nullable disable

namespace Gamestore.DAL.Migrations;

/// <inheritdoc />
public partial class Epic06CommentsAndModeration : Microsoft.EntityFrameworkCore.Migrations.Migration
{
    /// <inheritdoc />
    protected override void Up(Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CommentBans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                BannedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsPermanent = table.Column<bool>(type: "bit", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CommentBans", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Comments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                QuotedCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Comments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Comments_Comments_ParentCommentId",
                    column: x => x.ParentCommentId,
                    principalTable: "Comments",
                    principalColumn: "Id",
                    onDelete: Microsoft.EntityFrameworkCore.Migrations.ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Comments_Comments_QuotedCommentId",
                    column: x => x.QuotedCommentId,
                    principalTable: "Comments",
                    principalColumn: "Id",
                    onDelete: Microsoft.EntityFrameworkCore.Migrations.ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Comments_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: Microsoft.EntityFrameworkCore.Migrations.ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CommentBans_Name",
            table: "CommentBans",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Comments_GameId",
            table: "Comments",
            column: "GameId");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_ParentCommentId",
            table: "Comments",
            column: "ParentCommentId");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_QuotedCommentId",
            table: "Comments",
            column: "QuotedCommentId");
    }

    /// <inheritdoc />
    protected override void Down(Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CommentBans");

        migrationBuilder.DropTable(
            name: "Comments");
    }
}
