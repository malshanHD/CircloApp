using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CircloApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalExpenseTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PersonalExpenseSettingsUserId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MonthlyExpenseBudgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    LimitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyExpenseBudgets", x => x.Id);
                    table.CheckConstraint("CK_MonthlyExpenseBudget_LimitAmount", "[LimitAmount] >= 0");
                    table.CheckConstraint("CK_MonthlyExpenseBudget_Month", "[Month] >= 1 AND [Month] <= 12");
                    table.ForeignKey(
                        name: "FK_MonthlyExpenseBudgets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalExpenseCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalExpenseCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonalExpenseSettings",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultMonthlyLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    WarningPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalExpenseSettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_PersonalExpenseSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalExpenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpenseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalExpenses_PersonalExpenseCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PersonalExpenseCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonalExpenses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PersonalExpenseCategories",
                columns: new[] { "Id", "Color", "CreatedAt", "Icon", "IsActive", "IsDeleted", "IsSystem", "Name", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "#F59E0B", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "utensils", true, false, true, "Food", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "#3B82F6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "car", true, false, true, "Transport", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "#EC4899", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "shopping-bag", true, false, true, "Shopping", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "#8B5CF6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "receipt", true, false, true, "Bills", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "#EF4444", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "heart", true, false, true, "Health", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "#14B8A6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "film", true, false, true, "Entertainment", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "#6366F1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "book", true, false, true, "Education", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "#06B6D4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "plane", true, false, true, "Travel", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "#64748B", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "circle", true, false, true, "Other", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "#94A3B8", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "help-circle", true, false, true, "Uncategorized", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PersonalExpenseSettingsUserId",
                table: "Users",
                column: "PersonalExpenseSettingsUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyExpenseBudgets_UserId_Year_Month",
                table: "MonthlyExpenseBudgets",
                columns: new[] { "UserId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalExpenseCategories_UserId_Name",
                table: "PersonalExpenseCategories",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalExpenses_CategoryId",
                table: "PersonalExpenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalExpenses_UserId_CategoryId_ExpenseDate",
                table: "PersonalExpenses",
                columns: new[] { "UserId", "CategoryId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalExpenses_UserId_ExpenseDate",
                table: "PersonalExpenses",
                columns: new[] { "UserId", "ExpenseDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PersonalExpenseSettings_PersonalExpenseSettingsUserId",
                table: "Users",
                column: "PersonalExpenseSettingsUserId",
                principalTable: "PersonalExpenseSettings",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PersonalExpenseSettings_PersonalExpenseSettingsUserId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "MonthlyExpenseBudgets");

            migrationBuilder.DropTable(
                name: "PersonalExpenses");

            migrationBuilder.DropTable(
                name: "PersonalExpenseSettings");

            migrationBuilder.DropTable(
                name: "PersonalExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_Users_PersonalExpenseSettingsUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PersonalExpenseSettingsUserId",
                table: "Users");
        }
    }
}
