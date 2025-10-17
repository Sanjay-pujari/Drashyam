using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Drashyam.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteReferralAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InviteAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InvitesSent = table.Column<int>(type: "integer", nullable: false),
                    InvitesAccepted = table.Column<int>(type: "integer", nullable: false),
                    InvitesExpired = table.Column<int>(type: "integer", nullable: false),
                    InvitesCancelled = table.Column<int>(type: "integer", nullable: false),
                    ConversionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    EmailInvites = table.Column<int>(type: "integer", nullable: false),
                    SocialInvites = table.Column<int>(type: "integer", nullable: false),
                    DirectLinkInvites = table.Column<int>(type: "integer", nullable: false),
                    BulkInvites = table.Column<int>(type: "integer", nullable: false),
                    AverageTimeToAccept = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Resends = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InviteAnalytics_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InviteEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    InviteId = table.Column<int>(type: "integer", nullable: true),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InviteEvents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InviteEvents_UserInvites_InviteId",
                        column: x => x.InviteId,
                        principalTable: "UserInvites",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReferralAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReferralsCreated = table.Column<int>(type: "integer", nullable: false),
                    ReferralsCompleted = table.Column<int>(type: "integer", nullable: false),
                    ReferralsRewarded = table.Column<int>(type: "integer", nullable: false),
                    TotalRewardsEarned = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalRewardsClaimed = table.Column<decimal>(type: "numeric", nullable: false),
                    ConversionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    ReferralCodesGenerated = table.Column<int>(type: "integer", nullable: false),
                    ReferralCodesUsed = table.Column<int>(type: "integer", nullable: false),
                    AverageRewardAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageTimeToComplete = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralAnalytics_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReferralEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ReferralId = table.Column<int>(type: "integer", nullable: true),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralEvents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReferralEvents_Referrals_ReferralId",
                        column: x => x.ReferralId,
                        principalTable: "Referrals",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InviteAnalytics_Date",
                table: "InviteAnalytics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_InviteAnalytics_UserId",
                table: "InviteAnalytics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InviteAnalytics_UserId_Date",
                table: "InviteAnalytics",
                columns: new[] { "UserId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InviteEvents_EventType",
                table: "InviteEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_InviteEvents_InviteId",
                table: "InviteEvents",
                column: "InviteId");

            migrationBuilder.CreateIndex(
                name: "IX_InviteEvents_Timestamp",
                table: "InviteEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_InviteEvents_UserId",
                table: "InviteEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralAnalytics_Date",
                table: "ReferralAnalytics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralAnalytics_UserId",
                table: "ReferralAnalytics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralAnalytics_UserId_Date",
                table: "ReferralAnalytics",
                columns: new[] { "UserId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralEvents_EventType",
                table: "ReferralEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralEvents_ReferralId",
                table: "ReferralEvents",
                column: "ReferralId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralEvents_Timestamp",
                table: "ReferralEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralEvents_UserId",
                table: "ReferralEvents",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InviteAnalytics");

            migrationBuilder.DropTable(
                name: "InviteEvents");

            migrationBuilder.DropTable(
                name: "ReferralAnalytics");

            migrationBuilder.DropTable(
                name: "ReferralEvents");
        }
    }
}
