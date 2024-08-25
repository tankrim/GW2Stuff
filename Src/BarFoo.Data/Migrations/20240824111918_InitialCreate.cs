using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarFoo.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    HasBeenSyncedOnce = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    LastSyncTime = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Objectives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Track = table.Column<string>(type: "TEXT", nullable: false),
                    Acclaim = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgressCurrent = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgressComplete = table.Column<int>(type: "INTEGER", nullable: false),
                    Claimed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApiEndpoint = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objectives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeyObjectives",
                columns: table => new
                {
                    ApiKeyName = table.Column<string>(type: "TEXT", nullable: false),
                    ObjectiveId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeyObjectives", x => new { x.ApiKeyName, x.ObjectiveId });
                    table.ForeignKey(
                        name: "FK_ApiKeyObjectives_ApiKeys_ApiKeyName",
                        column: x => x.ApiKeyName,
                        principalTable: "ApiKeys",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiKeyObjectives_Objectives_ObjectiveId",
                        column: x => x.ObjectiveId,
                        principalTable: "Objectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeyObjectives_ObjectiveId",
                table: "ApiKeyObjectives",
                column: "ObjectiveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeyObjectives");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "Objectives");
        }
    }
}
