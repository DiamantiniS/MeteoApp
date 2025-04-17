using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeteoApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameToWeatherData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "WeatherData",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "WeatherData");
        }
    }
}
