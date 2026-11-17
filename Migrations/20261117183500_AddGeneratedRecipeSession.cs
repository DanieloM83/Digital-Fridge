using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital_Fridge.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedRecipeSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneratedRecipeSessions",
                columns: table => new {
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_GeneratedRecipeSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_GeneratedRecipeSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedRecipes",
                columns: table => new {
                    GeneratedRecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_GeneratedRecipes", x => x.GeneratedRecipeId);
                    table.ForeignKey(
                        name: "FK_GeneratedRecipes_GeneratedRecipeSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "GeneratedRecipeSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneratedRecipes_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedRecipes_RecipeId",
                table: "GeneratedRecipes",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedRecipes_SessionId",
                table: "GeneratedRecipes",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedRecipeSessions_UserId",
                table: "GeneratedRecipeSessions",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratedRecipes");

            migrationBuilder.DropTable(
                name: "GeneratedRecipeSessions");
        }
    }
}
