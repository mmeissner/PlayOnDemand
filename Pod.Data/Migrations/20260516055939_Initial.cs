using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pod.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "SubscriptionOrder_OrderNumber_seq",
                startValue: 1000L);

            migrationBuilder.CreateSequence(
                name: "User_CustomerNumber_seq",
                startValue: 100L);

            migrationBuilder.CreateTable(
                name: "DeviceIdentities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceIdentities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EMailAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    SenderName = table.Column<string>(type: "text", nullable: true),
                    EmailAddress = table.Column<string>(type: "text", nullable: true),
                    SmtpServer = table.Column<string>(type: "text", nullable: true),
                    SmtpPort = table.Column<int>(type: "integer", nullable: false),
                    UseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AuthMethod = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMailAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailContentTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Identifier = table.Column<int>(type: "integer", nullable: false),
                    SubjectText = table.Column<string>(type: "text", nullable: true),
                    VariableControlChar = table.Column<char>(type: "character(1)", nullable: false),
                    ContentText = table.Column<string>(type: "text", nullable: true),
                    ContentHtml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailContentTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailSendOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSendAttemptUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TemplateIdentifier = table.Column<int>(type: "integer", nullable: false),
                    SendState = table.Column<int>(type: "integer", nullable: false),
                    SendAttempts = table.Column<long>(type: "bigint", nullable: false),
                    ErrorMsg = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSendOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicHostAddress = table.Column<string>(type: "text", nullable: true),
                    PublicPort = table.Column<long>(type: "bigint", nullable: false),
                    PublicInterfaceVersion = table.Column<long>(type: "bigint", nullable: false),
                    HeartbeatInterval = table.Column<TimeSpan>(type: "interval", nullable: false),
                    HeartbeatTimeout = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ConnectTimeout = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessionRule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDuration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    StartApplication = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionRule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentReference = table.Column<string>(type: "text", nullable: true),
                    PaymentGatewayReference = table.Column<string>(type: "text", nullable: true),
                    PaymentAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    PaymentReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UniqueApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Origin = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<string>(type: "text", nullable: true),
                    Platform = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniqueApps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNumber = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"User_CustomerNumber_seq\"')"),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailAccTemplateLinks",
                columns: table => new
                {
                    EMailAccountDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    EMailContentTemplateId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAccTemplateLinks", x => new { x.EMailAccountDataId, x.EMailContentTemplateId });
                    table.ForeignKey(
                        name: "FK_EmailAccTemplateLinks_EMailAccounts_EMailAccountDataId",
                        column: x => x.EMailAccountDataId,
                        principalTable: "EMailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailAccTemplateLinks_EmailContentTemplates_EMailContentTem~",
                        column: x => x.EMailContentTemplateId,
                        principalTable: "EmailContentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateVariable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailContentTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariableKey = table.Column<int>(type: "integer", nullable: false),
                    VariableKeyString = table.Column<string>(type: "text", nullable: true),
                    StartChar = table.Column<int>(type: "integer", nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateVariable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateVariable_EmailContentTemplates_EmailContentTemplate~",
                        column: x => x.EmailContentTemplateId,
                        principalTable: "EmailContentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMailReceiver",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailSendOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMailReceiver", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EMailReceiver_EmailSendOrders_EmailSendOrderId",
                        column: x => x.EmailSendOrderId,
                        principalTable: "EmailSendOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMailVariableValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailSendOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMailVariableValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EMailVariableValue_EmailSendOrders_EmailSendOrderId",
                        column: x => x.EmailSendOrderId,
                        principalTable: "EmailSendOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stations_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRoots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LastSyncTimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceIdentityId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRoots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationRoots_DeviceIdentities_DeviceIdentityId",
                        column: x => x.DeviceIdentityId,
                        principalTable: "DeviceIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationRoots_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClosedConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedServerOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConnectedToServerOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisconnectedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedBy = table.Column<int>(type: "integer", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceIdentityId = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosedConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClosedConnections_DeviceIdentities_DeviceIdentityId",
                        column: x => x.DeviceIdentityId,
                        principalTable: "DeviceIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClosedConnections_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClosedConnections_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectionStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NetworkState = table.Column<int>(type: "integer", nullable: false),
                    ServerRequestOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConnectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConnectedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastHeartbeatOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeviceIdentityId = table.Column<string>(type: "text", nullable: true),
                    ShellServerId = table.Column<Guid>(type: "uuid", nullable: true),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConnectionStates_DeviceIdentities_DeviceIdentityId",
                        column: x => x.DeviceIdentityId,
                        principalTable: "DeviceIdentities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConnectionStates_Servers_ShellServerId",
                        column: x => x.ShellServerId,
                        principalTable: "Servers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConnectionStates_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    RequestedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestedBy = table.Column<int>(type: "integer", nullable: false),
                    RequestFromIpAddress = table.Column<string>(type: "text", nullable: true),
                    RequestReference = table.Column<string>(type: "text", nullable: true),
                    SendOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SendToConnectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    StoppedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StopReason = table.Column<int>(type: "integer", nullable: false),
                    SessionRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_SessionRule_SessionRuleId",
                        column: x => x.SessionRuleId,
                        principalTable: "SessionRule",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sessions_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StationApiKeys",
                columns: table => new
                {
                    PublicKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SecretKey = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationApiKeys", x => x.PublicKey);
                    table.ForeignKey(
                        name: "FK_StationApiKeys_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    QRCode = table.Column<string>(type: "text", nullable: true),
                    ControlMode = table.Column<int>(type: "integer", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StationSettings_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionStates_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstanceVersion = table.Column<long>(type: "bigint", nullable: false),
                    LocalDisplayName = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsInstalled = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicationRootId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniqueAppId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalApps_ApplicationRoots_ApplicationRootId",
                        column: x => x.ApplicationRootId,
                        principalTable: "ApplicationRoots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalApps_UniqueApps_UniqueAppId",
                        column: x => x.UniqueAppId,
                        principalTable: "UniqueApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestFrom = table.Column<int>(type: "integer", nullable: false),
                    SourceIpAddress = table.Column<string>(type: "text", nullable: true),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    TimeChange = table.Column<TimeSpan>(type: "interval", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeRequest_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeoutLoginRequestDelivery = table.Column<TimeSpan>(type: "interval", nullable: false),
                    TimeoutLoginRequestResponse = table.Column<TimeSpan>(type: "interval", nullable: false),
                    UserTimeForLoginRequestResponse = table.Column<TimeSpan>(type: "interval", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionDetails_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessionDetails_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExtendsFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExtendsToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionChanges_SubscriptionPayments_SubscriptionPaymen~",
                        column: x => x.SubscriptionPaymentId,
                        principalTable: "SubscriptionPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionChanges_SubscriptionStates_SubscriptionStateId",
                        column: x => x.SubscriptionStateId,
                        principalTable: "SubscriptionStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"SubscriptionOrder_OrderNumber_seq\"')"),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedFromIpAddress = table.Column<string>(type: "text", nullable: true),
                    TimeOrdered = table.Column<TimeSpan>(type: "interval", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    CustomerOrderReference = table.Column<string>(type: "text", nullable: true),
                    SubscriptionStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionOrders_SubscriptionPayments_SubscriptionPayment~",
                        column: x => x.SubscriptionPaymentId,
                        principalTable: "SubscriptionPayments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubscriptionOrders_SubscriptionStates_SubscriptionStateId",
                        column: x => x.SubscriptionStateId,
                        principalTable: "SubscriptionStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionRuleLocalApp",
                columns: table => new
                {
                    SessionRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalAppId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionRuleLocalApp", x => new { x.SessionRuleId, x.LocalAppId });
                    table.ForeignKey(
                        name: "FK_SessionRuleLocalApp_LocalApps_LocalAppId",
                        column: x => x.LocalAppId,
                        principalTable: "LocalApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionRuleLocalApp_SessionRule_SessionRuleId",
                        column: x => x.SessionRuleId,
                        principalTable: "SessionRule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoots_DeviceIdentityId",
                table: "ApplicationRoots",
                column: "DeviceIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoots_StationId_DeviceIdentityId",
                table: "ApplicationRoots",
                columns: new[] { "StationId", "DeviceIdentityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequest_SessionId",
                table: "ChangeRequest",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedConnections_ConnectionId",
                table: "ClosedConnections",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedConnections_DeviceIdentityId",
                table: "ClosedConnections",
                column: "DeviceIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedConnections_ServerId",
                table: "ClosedConnections",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedConnections_StationId",
                table: "ClosedConnections",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionStates_DeviceIdentityId",
                table: "ConnectionStates",
                column: "DeviceIdentityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionStates_ShellServerId",
                table: "ConnectionStates",
                column: "ShellServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionStates_StationId",
                table: "ConnectionStates",
                column: "StationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailAccTemplateLinks_EMailContentTemplateId",
                table: "EmailAccTemplateLinks",
                column: "EMailContentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EMailReceiver_EmailSendOrderId",
                table: "EMailReceiver",
                column: "EmailSendOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSendOrders_SendState",
                table: "EmailSendOrders",
                column: "SendState");

            migrationBuilder.CreateIndex(
                name: "IX_EMailVariableValue_EmailSendOrderId",
                table: "EMailVariableValue",
                column: "EmailSendOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalApps_ApplicationRootId_UniqueAppId",
                table: "LocalApps",
                columns: new[] { "ApplicationRootId", "UniqueAppId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalApps_InstanceVersion",
                table: "LocalApps",
                column: "InstanceVersion");

            migrationBuilder.CreateIndex(
                name: "IX_LocalApps_UniqueAppId",
                table: "LocalApps",
                column: "UniqueAppId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servers_PublicHostAddress_PublicPort",
                table: "Servers",
                columns: new[] { "PublicHostAddress", "PublicPort" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionDetails_SessionId",
                table: "SessionDetails",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionDetails_StationId",
                table: "SessionDetails",
                column: "StationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionRuleLocalApp_LocalAppId",
                table: "SessionRuleLocalApp",
                column: "LocalAppId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionRuleId",
                table: "Sessions",
                column: "SessionRuleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_StationId",
                table: "Sessions",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_StationApiKeys_StationId",
                table: "StationApiKeys",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_ApplicationUserId",
                table: "Stations",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StationSettings_StationId",
                table: "StationSettings",
                column: "StationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionChanges_SubscriptionPaymentId",
                table: "SubscriptionChanges",
                column: "SubscriptionPaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionChanges_SubscriptionStateId",
                table: "SubscriptionChanges",
                column: "SubscriptionStateId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionOrders_ApplicationUserId",
                table: "SubscriptionOrders",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionOrders_OrderNumber",
                table: "SubscriptionOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionOrders_SubscriptionPaymentId",
                table: "SubscriptionOrders",
                column: "SubscriptionPaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionOrders_SubscriptionStateId",
                table: "SubscriptionOrders",
                column: "SubscriptionStateId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStates_StationId",
                table: "SubscriptionStates",
                column: "StationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateVariable_EmailContentTemplateId",
                table: "TemplateVariable",
                column: "EmailContentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CustomerNumber",
                table: "Users",
                column: "CustomerNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeRequest");

            migrationBuilder.DropTable(
                name: "ClosedConnections");

            migrationBuilder.DropTable(
                name: "ConnectionStates");

            migrationBuilder.DropTable(
                name: "EmailAccTemplateLinks");

            migrationBuilder.DropTable(
                name: "EMailReceiver");

            migrationBuilder.DropTable(
                name: "EMailVariableValue");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "SessionDetails");

            migrationBuilder.DropTable(
                name: "SessionRuleLocalApp");

            migrationBuilder.DropTable(
                name: "StationApiKeys");

            migrationBuilder.DropTable(
                name: "StationSettings");

            migrationBuilder.DropTable(
                name: "SubscriptionChanges");

            migrationBuilder.DropTable(
                name: "SubscriptionOrders");

            migrationBuilder.DropTable(
                name: "TemplateVariable");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "EMailAccounts");

            migrationBuilder.DropTable(
                name: "EmailSendOrders");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "LocalApps");

            migrationBuilder.DropTable(
                name: "SubscriptionPayments");

            migrationBuilder.DropTable(
                name: "SubscriptionStates");

            migrationBuilder.DropTable(
                name: "EmailContentTemplates");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "SessionRule");

            migrationBuilder.DropTable(
                name: "ApplicationRoots");

            migrationBuilder.DropTable(
                name: "UniqueApps");

            migrationBuilder.DropTable(
                name: "DeviceIdentities");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropSequence(
                name: "SubscriptionOrder_OrderNumber_seq");

            migrationBuilder.DropSequence(
                name: "User_CustomerNumber_seq");
        }
    }
}
