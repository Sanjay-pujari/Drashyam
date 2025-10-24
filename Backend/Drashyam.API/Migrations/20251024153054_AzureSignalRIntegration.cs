using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drashyam.API.Migrations
{
    /// <inheritdoc />
    public partial class AzureSignalRIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MerchandiseOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "MerchandiseOrders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "MerchandiseOrders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MerchandiseOrders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MerchandiseOrders");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "MerchandiseOrders");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "MerchandiseOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MerchandiseOrders");
        }
    }
}
