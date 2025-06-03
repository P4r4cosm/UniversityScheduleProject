using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Schedule_Generator.Migrations
{
    /// <inheritdoc />
    public partial class Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "visitTime",
                table: "Visits",
                newName: "VisitTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VisitTime",
                table: "Visits",
                newName: "visitTime");
        }
    }
}
