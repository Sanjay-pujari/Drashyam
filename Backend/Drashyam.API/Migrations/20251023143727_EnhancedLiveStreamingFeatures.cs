using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Drashyam.API.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedLiveStreamingFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LiveStreamEventId",
                table: "LiveStreams",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LiveStreamChallengeId",
                table: "ChallengeParticipants",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LiveStreamAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    ViewerCount = table.Column<long>(type: "bigint", nullable: false),
                    PeakViewerCount = table.Column<long>(type: "bigint", nullable: false),
                    TotalViewTime = table.Column<long>(type: "bigint", nullable: false),
                    ChatMessageCount = table.Column<long>(type: "bigint", nullable: false),
                    ReactionCount = table.Column<long>(type: "bigint", nullable: false),
                    ShareCount = table.Column<long>(type: "bigint", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamAnalytics_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxParticipants = table.Column<int>(type: "integer", nullable: false),
                    CurrentParticipants = table.Column<int>(type: "integer", nullable: false),
                    PrizeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PrizeDescription = table.Column<string>(type: "text", nullable: true),
                    IsMonetized = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamChallenges_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: true),
                    IsModerator = table.Column<bool>(type: "boolean", nullable: false),
                    IsHighlighted = table.Column<bool>(type: "boolean", nullable: false),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamChats_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveStreamChats_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamCollaborations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    CollaboratorId = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevenueShare = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamCollaborations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamCollaborations_AspNetUsers_CollaboratorId",
                        column: x => x.CollaboratorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveStreamCollaborations_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamDonations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    DonorId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    IsHighlighted = table.Column<bool>(type: "boolean", nullable: false),
                    DonatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamDonations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamDonations_AspNetUsers_DonorId",
                        column: x => x.DonorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveStreamDonations_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatorId = table.Column<string>(type: "text", nullable: false),
                    ChannelId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ScheduledStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsMonetized = table.Column<bool>(type: "boolean", nullable: false),
                    TicketPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxAttendees = table.Column<int>(type: "integer", nullable: false),
                    CurrentAttendees = table.Column<int>(type: "integer", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Tags = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamEvents_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveStreamEvents_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamPolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    Question = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AllowMultipleChoices = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamPolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamPolls_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamQualities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    Quality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Bitrate = table.Column<int>(type: "integer", nullable: false),
                    FrameRate = table.Column<int>(type: "integer", nullable: false),
                    Resolution = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamQualities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamQualities_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamReactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamReactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveStreamReactions_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamRevenues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric", nullable: false),
                    DonationRevenue = table.Column<decimal>(type: "numeric", nullable: false),
                    SuperChatRevenue = table.Column<decimal>(type: "numeric", nullable: false),
                    SubscriptionRevenue = table.Column<decimal>(type: "numeric", nullable: false),
                    AdRevenue = table.Column<decimal>(type: "numeric", nullable: false),
                    PlatformFee = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatorEarnings = table.Column<decimal>(type: "numeric", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamRevenues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamRevenues_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    SubscriberId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    SubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamSubscriptions_AspNetUsers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveStreamSubscriptions_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamSuperChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiveStreamId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    IsHighlighted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamSuperChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamSuperChats_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveStreamSuperChats_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventAttendees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckedOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAttendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventAttendees_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventAttendees_LiveStreamEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "LiveStreamEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamPollOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PollId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VoteCount = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamPollOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamPollOptions_LiveStreamPolls_PollId",
                        column: x => x.PollId,
                        principalTable: "LiveStreamPolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamPollVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PollId = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamPollVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamPollVotes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveStreamPollVotes_LiveStreamPollOptions_OptionId",
                        column: x => x.OptionId,
                        principalTable: "LiveStreamPollOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveStreamPollVotes_LiveStreamPolls_PollId",
                        column: x => x.PollId,
                        principalTable: "LiveStreamPolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreams_LiveStreamEventId",
                table: "LiveStreams",
                column: "LiveStreamEventId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_LiveStreamChallengeId",
                table: "ChallengeParticipants",
                column: "LiveStreamChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendees_EventId",
                table: "EventAttendees",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendees_UserId",
                table: "EventAttendees",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamAnalytics_LiveStreamId",
                table: "LiveStreamAnalytics",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamChallenges_LiveStreamId",
                table: "LiveStreamChallenges",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamChats_LiveStreamId",
                table: "LiveStreamChats",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamChats_UserId",
                table: "LiveStreamChats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamCollaborations_CollaboratorId",
                table: "LiveStreamCollaborations",
                column: "CollaboratorId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamCollaborations_LiveStreamId",
                table: "LiveStreamCollaborations",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamDonations_DonorId",
                table: "LiveStreamDonations",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamDonations_LiveStreamId",
                table: "LiveStreamDonations",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamEvents_ChannelId",
                table: "LiveStreamEvents",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamEvents_CreatorId",
                table: "LiveStreamEvents",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamPollOptions_PollId",
                table: "LiveStreamPollOptions",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamPolls_LiveStreamId",
                table: "LiveStreamPolls",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamPollVotes_OptionId",
                table: "LiveStreamPollVotes",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamPollVotes_PollId",
                table: "LiveStreamPollVotes",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamPollVotes_UserId",
                table: "LiveStreamPollVotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamQualities_LiveStreamId",
                table: "LiveStreamQualities",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamReactions_LiveStreamId",
                table: "LiveStreamReactions",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamReactions_UserId",
                table: "LiveStreamReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamRevenues_LiveStreamId",
                table: "LiveStreamRevenues",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamSubscriptions_LiveStreamId",
                table: "LiveStreamSubscriptions",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamSubscriptions_SubscriberId",
                table: "LiveStreamSubscriptions",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamSuperChats_LiveStreamId",
                table: "LiveStreamSuperChats",
                column: "LiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamSuperChats_UserId",
                table: "LiveStreamSuperChats",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengeParticipants_LiveStreamChallenges_LiveStreamChalle~",
                table: "ChallengeParticipants",
                column: "LiveStreamChallengeId",
                principalTable: "LiveStreamChallenges",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LiveStreams_LiveStreamEvents_LiveStreamEventId",
                table: "LiveStreams",
                column: "LiveStreamEventId",
                principalTable: "LiveStreamEvents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengeParticipants_LiveStreamChallenges_LiveStreamChalle~",
                table: "ChallengeParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_LiveStreams_LiveStreamEvents_LiveStreamEventId",
                table: "LiveStreams");

            migrationBuilder.DropTable(
                name: "EventAttendees");

            migrationBuilder.DropTable(
                name: "LiveStreamAnalytics");

            migrationBuilder.DropTable(
                name: "LiveStreamChallenges");

            migrationBuilder.DropTable(
                name: "LiveStreamChats");

            migrationBuilder.DropTable(
                name: "LiveStreamCollaborations");

            migrationBuilder.DropTable(
                name: "LiveStreamDonations");

            migrationBuilder.DropTable(
                name: "LiveStreamPollVotes");

            migrationBuilder.DropTable(
                name: "LiveStreamQualities");

            migrationBuilder.DropTable(
                name: "LiveStreamReactions");

            migrationBuilder.DropTable(
                name: "LiveStreamRevenues");

            migrationBuilder.DropTable(
                name: "LiveStreamSubscriptions");

            migrationBuilder.DropTable(
                name: "LiveStreamSuperChats");

            migrationBuilder.DropTable(
                name: "LiveStreamEvents");

            migrationBuilder.DropTable(
                name: "LiveStreamPollOptions");

            migrationBuilder.DropTable(
                name: "LiveStreamPolls");

            migrationBuilder.DropIndex(
                name: "IX_LiveStreams_LiveStreamEventId",
                table: "LiveStreams");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeParticipants_LiveStreamChallengeId",
                table: "ChallengeParticipants");

            migrationBuilder.DropColumn(
                name: "LiveStreamEventId",
                table: "LiveStreams");

            migrationBuilder.DropColumn(
                name: "LiveStreamChallengeId",
                table: "ChallengeParticipants");
        }
    }
}
