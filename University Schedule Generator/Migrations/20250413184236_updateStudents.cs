using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Schedule_Generator.Migrations
{
    /// <inheritdoc />
    public partial class updateStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdGroup",
                table: "Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdGroup",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
