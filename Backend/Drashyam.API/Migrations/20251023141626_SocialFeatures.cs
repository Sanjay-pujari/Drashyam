using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Drashyam.API.Migrations
{
    /// <inheritdoc />
    public partial class SocialFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatorId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Hashtag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VotingEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxParticipants = table.Column<int>(type: "integer", nullable: true),
                    MinVideoLength = table.Column<int>(type: "integer", nullable: true),
                    MaxVideoLength = table.Column<int>(type: "integer", nullable: true),
                    Rules = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Prizes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PrizeAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    PrizeCurrency = table.Column<string>(type: "text", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    BannerUrl = table.Column<string>(type: "text", nullable: true),
                    ParticipantCount = table.Column<int>(type: "integer", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    ShareCount = table.Column<int>(type: "integer", nullable: false),
                    AllowVoting = table.Column<bool>(type: "boolean", nullable: false),
                    AllowComments = table.Column<bool>(type: "boolean", nullable: false),
                    RequireApproval = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityChallenges_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentPromotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Budget = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    SpentAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TargetViews = table.Column<int>(type: "integer", nullable: false),
                    TargetClicks = table.Column<int>(type: "integer", nullable: false),
                    TargetEngagement = table.Column<int>(type: "integer", nullable: false),
                    ActualViews = table.Column<int>(type: "integer", nullable: false),
                    ActualClicks = table.Column<int>(type: "integer", nullable: false),
                    ActualEngagement = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPromotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentPromotions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentPromotions_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreatorCollaborations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InitiatorId = table.Column<string>(type: "text", nullable: false),
                    CollaboratorId = table.Column<string>(type: "text", nullable: false),
                    VideoId = table.Column<int>(type: "integer", nullable: true),
                    ChannelId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InitiatorRole = table.Column<int>(type: "integer", nullable: false),
                    CollaboratorRole = table.Column<int>(type: "integer", nullable: false),
                    RevenueSharePercentage = table.Column<decimal>(type: "numeric", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreatorCollaborations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreatorCollaborations_AspNetUsers_CollaboratorId",
                        column: x => x.CollaboratorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreatorCollaborations_AspNetUsers_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreatorCollaborations_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreatorCollaborations_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EnhancedComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoId = table.Column<int>(type: "integer", nullable: false),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    DislikeCount = table.Column<int>(type: "integer", nullable: false),
                    ReplyCount = table.Column<int>(type: "integer", nullable: false),
                    ShareCount = table.Column<int>(type: "integer", nullable: false),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    IsHighlighted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsEdited = table.Column<bool>(type: "boolean", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancedComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnhancedComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancedComments_EnhancedComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "EnhancedComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EnhancedComments_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentorshipPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MentorId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Requirements = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Benefits = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    MaxMentees = table.Column<int>(type: "integer", nullable: false),
                    CurrentMentees = table.Column<int>(type: "integer", nullable: false),
                    DurationWeeks = table.Column<int>(type: "integer", nullable: false),
                    Fee = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicationDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    ApplicationCount = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipPrograms_AspNetUsers_MentorId",
                        column: x => x.MentorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    PlatformUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PlatformUsername = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ConnectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialConnections_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VideoId = table.Column<int>(type: "integer", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    CustomMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Hashtags = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ShareUrl = table.Column<string>(type: "text", nullable: false),
                    ExternalPostId = table.Column<string>(type: "text", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    ClickCount = table.Column<int>(type: "integer", nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    CommentCount = table.Column<int>(type: "integer", nullable: false),
                    ShareCount = table.Column<int>(type: "integer", nullable: false),
                    IsScheduled = table.Column<bool>(type: "boolean", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialShares_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SocialShares_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViralContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    ViewVelocity = table.Column<int>(type: "integer", nullable: false),
                    ShareVelocity = table.Column<int>(type: "integer", nullable: false),
                    EngagementVelocity = table.Column<int>(type: "integer", nullable: false),
                    ViralScore = table.Column<decimal>(type: "numeric", nullable: false),
                    PeakViews = table.Column<int>(type: "integer", nullable: false),
                    PeakShares = table.Column<int>(type: "integer", nullable: false),
                    PeakEngagement = table.Column<int>(type: "integer", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeakAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationHours = table.Column<int>(type: "integer", nullable: false),
                    TotalViews = table.Column<int>(type: "integer", nullable: false),
                    TotalShares = table.Column<int>(type: "integer", nullable: false),
                    TotalEngagement = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViralContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViralContents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ViralContents_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChallengeId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmissionCount = table.Column<int>(type: "integer", nullable: false),
                    VoteCount = table.Column<int>(type: "integer", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    IsWinner = table.Column<bool>(type: "boolean", nullable: false),
                    WinnerPosition = table.Column<int>(type: "integer", nullable: true),
                    PrizeAmount = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeParticipants_CommunityChallenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "CommunityChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChallengeId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VideoId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VideoUrl = table.Column<string>(type: "text", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    VideoQuality = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    CommentCount = table.Column<int>(type: "integer", nullable: false),
                    ShareCount = table.Column<int>(type: "integer", nullable: false),
                    VoteCount = table.Column<int>(type: "integer", nullable: false),
                    IsWinner = table.Column<bool>(type: "boolean", nullable: false),
                    WinnerPosition = table.Column<int>(type: "integer", nullable: true),
                    PrizeAmount = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeSubmissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeSubmissions_CommunityChallenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "CommunityChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeSubmissions_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CollaborationAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollaborationId = table.Column<int>(type: "integer", nullable: false),
                    UploadedById = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollaborationAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollaborationAssets_AspNetUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollaborationAssets_CreatorCollaborations_CollaborationId",
                        column: x => x.CollaborationId,
                        principalTable: "CreatorCollaborations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollaborationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollaborationId = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    AttachmentType = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollaborationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollaborationMessages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollaborationMessages_CreatorCollaborations_CollaborationId",
                        column: x => x.CollaborationId,
                        principalTable: "CreatorCollaborations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentMentions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommentId = table.Column<int>(type: "integer", nullable: false),
                    MentionedUserId = table.Column<string>(type: "text", nullable: false),
                    MentionedUserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MentionedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentMentions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentMentions_AspNetUsers_MentionedUserId",
                        column: x => x.MentionedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentMentions_EnhancedComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "EnhancedComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentModerations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommentId = table.Column<int>(type: "integer", nullable: false),
                    ModeratorId = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModeratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentModerations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentModerations_AspNetUsers_ModeratorId",
                        column: x => x.ModeratorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentModerations_EnhancedComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "EnhancedComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentReactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommentId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ReactedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentReactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentReactions_EnhancedComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "EnhancedComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentorshipApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramId = table.Column<int>(type: "integer", nullable: false),
                    MenteeId = table.Column<string>(type: "text", nullable: false),
                    Motivation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Experience = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Goals = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PortfolioUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResumeUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MentorNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipApplications_AspNetUsers_MenteeId",
                        column: x => x.MenteeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorshipApplications_MentorshipPrograms_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "MentorshipPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentorshipReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramId = table.Column<int>(type: "integer", nullable: false),
                    MenteeId = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Review = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MentorRating = table.Column<int>(type: "integer", nullable: false),
                    ProgramRating = table.Column<int>(type: "integer", nullable: false),
                    ValueRating = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipReviews_AspNetUsers_MenteeId",
                        column: x => x.MenteeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorshipReviews_MentorshipPrograms_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "MentorshipPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentorshipSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramId = table.Column<int>(type: "integer", nullable: false),
                    MenteeId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    MeetingUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipSessions_AspNetUsers_MenteeId",
                        column: x => x.MenteeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorshipSessions_MentorshipPrograms_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "MentorshipPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChallengeId = table.Column<int>(type: "integer", nullable: false),
                    SubmissionId = table.Column<int>(type: "integer", nullable: true),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    ReplyCount = table.Column<int>(type: "integer", nullable: false),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeComments_ChallengeComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "ChallengeComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChallengeComments_ChallengeSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "ChallengeSubmissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChallengeComments_CommunityChallenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "CommunityChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChallengeId = table.Column<int>(type: "integer", nullable: false),
                    SubmissionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeVotes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeVotes_ChallengeSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "ChallengeSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeVotes_CommunityChallenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "CommunityChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeCommentLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommentId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeCommentLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeCommentLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeCommentLikes_ChallengeComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "ChallengeComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeCommentLikes_CommentId",
                table: "ChallengeCommentLikes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeCommentLikes_UserId",
                table: "ChallengeCommentLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeComments_ChallengeId",
                table: "ChallengeComments",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeComments_ParentCommentId",
                table: "ChallengeComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeComments_SubmissionId",
                table: "ChallengeComments",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeComments_UserId",
                table: "ChallengeComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_ChallengeId",
                table: "ChallengeParticipants",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_UserId",
                table: "ChallengeParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeSubmissions_ChallengeId",
                table: "ChallengeSubmissions",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeSubmissions_UserId",
                table: "ChallengeSubmissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeSubmissions_VideoId",
                table: "ChallengeSubmissions",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeVotes_ChallengeId",
                table: "ChallengeVotes",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeVotes_SubmissionId",
                table: "ChallengeVotes",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeVotes_UserId",
                table: "ChallengeVotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaborationAssets_CollaborationId",
                table: "CollaborationAssets",
                column: "CollaborationId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaborationAssets_UploadedById",
                table: "CollaborationAssets",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollaborationMessages_CollaborationId",
                table: "CollaborationMessages",
                column: "CollaborationId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaborationMessages_SenderId",
                table: "CollaborationMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentMentions_CommentId",
                table: "CommentMentions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentMentions_MentionedUserId",
                table: "CommentMentions",
                column: "MentionedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentModerations_CommentId",
                table: "CommentModerations",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentModerations_ModeratorId",
                table: "CommentModerations",
                column: "ModeratorId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_CommentId",
                table: "CommentReactions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_UserId",
                table: "CommentReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityChallenges_CreatorId",
                table: "CommunityChallenges",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPromotions_UserId",
                table: "ContentPromotions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPromotions_VideoId",
                table: "ContentPromotions",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_CreatorCollaborations_ChannelId",
                table: "CreatorCollaborations",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_CreatorCollaborations_CollaboratorId",
                table: "CreatorCollaborations",
                column: "CollaboratorId");

            migrationBuilder.CreateIndex(
                name: "IX_CreatorCollaborations_InitiatorId",
                table: "CreatorCollaborations",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CreatorCollaborations_VideoId",
                table: "CreatorCollaborations",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancedComments_ParentCommentId",
                table: "EnhancedComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancedComments_UserId",
                table: "EnhancedComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancedComments_VideoId",
                table: "EnhancedComments",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipApplications_MenteeId",
                table: "MentorshipApplications",
                column: "MenteeId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipApplications_ProgramId",
                table: "MentorshipApplications",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipPrograms_MentorId",
                table: "MentorshipPrograms",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipReviews_MenteeId",
                table: "MentorshipReviews",
                column: "MenteeId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipReviews_ProgramId",
                table: "MentorshipReviews",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipSessions_MenteeId",
                table: "MentorshipSessions",
                column: "MenteeId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipSessions_ProgramId",
                table: "MentorshipSessions",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialConnections_UserId",
                table: "SocialConnections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialShares_UserId",
                table: "SocialShares",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialShares_VideoId",
                table: "SocialShares",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_ViralContents_UserId",
                table: "ViralContents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ViralContents_VideoId",
                table: "ViralContents",
                column: "VideoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallengeCommentLikes");

            migrationBuilder.DropTable(
                name: "ChallengeParticipants");

            migrationBuilder.DropTable(
                name: "ChallengeVotes");

            migrationBuilder.DropTable(
                name: "CollaborationAssets");

            migrationBuilder.DropTable(
                name: "CollaborationMessages");

            migrationBuilder.DropTable(
                name: "CommentMentions");

            migrationBuilder.DropTable(
                name: "CommentModerations");

            migrationBuilder.DropTable(
                name: "CommentReactions");

            migrationBuilder.DropTable(
                name: "ContentPromotions");

            migrationBuilder.DropTable(
                name: "MentorshipApplications");

            migrationBuilder.DropTable(
                name: "MentorshipReviews");

            migrationBuilder.DropTable(
                name: "MentorshipSessions");

            migrationBuilder.DropTable(
                name: "SocialConnections");

            migrationBuilder.DropTable(
                name: "SocialShares");

            migrationBuilder.DropTable(
                name: "ViralContents");

            migrationBuilder.DropTable(
                name: "ChallengeComments");

            migrationBuilder.DropTable(
                name: "CreatorCollaborations");

            migrationBuilder.DropTable(
                name: "EnhancedComments");

            migrationBuilder.DropTable(
                name: "MentorshipPrograms");

            migrationBuilder.DropTable(
                name: "ChallengeSubmissions");

            migrationBuilder.DropTable(
                name: "CommunityChallenges");
        }
    }
}
