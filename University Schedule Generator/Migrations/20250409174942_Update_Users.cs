using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Schedule_Generator.Migrations
{
    /// <inheritdoc />
    public partial class Update_Users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$fIrKbG4cvjEkeGjRKAF/X.heLrKZLAmWfnVbdfKNMBMlW0HIS5UXS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$oncGYym9xRSGi0rYiIz9luHiKp76WPISoviXXCAHJ.8RiQ0U/egni");
        }
    }
}
