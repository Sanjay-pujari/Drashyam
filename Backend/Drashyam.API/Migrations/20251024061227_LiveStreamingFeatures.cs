using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drashyam.API.Migrations
{
    /// <inheritdoc />
    public partial class LiveStreamingFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastViewerCountUpdate",
                table: "LiveStreams",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastViewerCountUpdate",
                table: "LiveStreams");
        }
    }
}
