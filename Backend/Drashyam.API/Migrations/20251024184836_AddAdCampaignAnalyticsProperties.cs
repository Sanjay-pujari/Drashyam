using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drashyam.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdCampaignAnalyticsProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Spent",
                table: "AdCampaigns",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalClicks",
                table: "AdCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalImpressions",
                table: "AdCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRevenue",
                table: "AdCampaigns",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Spent",
                table: "AdCampaigns");

            migrationBuilder.DropColumn(
                name: "TotalClicks",
                table: "AdCampaigns");

            migrationBuilder.DropColumn(
                name: "TotalImpressions",
                table: "AdCampaigns");

            migrationBuilder.DropColumn(
                name: "TotalRevenue",
                table: "AdCampaigns");
        }
    }
}
