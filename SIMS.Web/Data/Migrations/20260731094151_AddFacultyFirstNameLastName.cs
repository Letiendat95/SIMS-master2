using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMS.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyFirstNameLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Faculty_FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Faculty_LastName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Faculty_FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Faculty_LastName",
                table: "Users");
        }
    }
}
