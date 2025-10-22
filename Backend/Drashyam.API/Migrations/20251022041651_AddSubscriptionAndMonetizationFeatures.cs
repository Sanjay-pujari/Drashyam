using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Drashyam.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionAndMonetizationFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChannelBrandings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BannerUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    SecondaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    AccentColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    CustomDomain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomCss = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AboutText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SocialMediaLinks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelBrandings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelBrandings_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelBrandings_ChannelId",
                table: "ChannelBrandings",
                column: "ChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelBrandings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Subscriptions");
        }
    }
}
