using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CircloApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingIsActiveColumnToEventMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EventMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EventMembers");
        }
    }
}
