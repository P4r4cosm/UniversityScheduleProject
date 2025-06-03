using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Schedule_Generator.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name", "PasswordHash" },
                values: new object[] { 1, "admin", "$2a$11$fIrKbG4cvjEkeGjRKAF/X.heLrKZLAmWfnVbdfKNMBMlW0HIS5UXS" });
        }
    }
}
