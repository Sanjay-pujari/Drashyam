using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drashyam.API.Migrations
{
    /// <inheritdoc />
    public partial class FixReferralIdDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing foreign key and index
            migrationBuilder.DropForeignKey(
                name: "FK_ReferralRewards_Referrals_ReferralId1",
                table: "ReferralRewards");

            migrationBuilder.DropIndex(
                name: "IX_ReferralRewards_ReferralId1",
                table: "ReferralRewards");

            // Drop the old ReferralId column and ReferralId1 column
            migrationBuilder.DropColumn(
                name: "ReferralId",
                table: "ReferralRewards");

            migrationBuilder.DropColumn(
                name: "ReferralId1",
                table: "ReferralRewards");

            // Add the new ReferralId column as integer
            migrationBuilder.AddColumn<int>(
                name: "ReferralId",
                table: "ReferralRewards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Create the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_ReferralRewards_Referrals_ReferralId",
                table: "ReferralRewards",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_ReferralRewards_Referrals_ReferralId",
                table: "ReferralRewards");

            // Drop the integer ReferralId column
            migrationBuilder.DropColumn(
                name: "ReferralId",
                table: "ReferralRewards");

            // Add back the string ReferralId column
            migrationBuilder.AddColumn<string>(
                name: "ReferralId",
                table: "ReferralRewards",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            // Add back the ReferralId1 column
            migrationBuilder.AddColumn<int>(
                name: "ReferralId1",
                table: "ReferralRewards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Create the index
            migrationBuilder.CreateIndex(
                name: "IX_ReferralRewards_ReferralId1",
                table: "ReferralRewards",
                column: "ReferralId1");

            // Add the foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_ReferralRewards_Referrals_ReferralId1",
                table: "ReferralRewards",
                column: "ReferralId1",
                principalTable: "Referrals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
