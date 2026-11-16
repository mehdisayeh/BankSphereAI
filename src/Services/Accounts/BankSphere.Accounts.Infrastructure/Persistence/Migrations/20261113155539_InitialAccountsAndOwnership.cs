using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSphere.Accounts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAccountsAndOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "accounts");

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Iban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    Bic = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Currency = table.Column<string>(type: "char(3)", nullable: false),
                    PrimaryOwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OpeningDateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActivationDateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ClosingDateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OverdraftFacilityReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StatementFrequency = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StatementDeliveryMethod = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    InterestRatePlanReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FeePlanReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DailyTransactionLimit_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    DailyTransactionLimit_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    MonthlyTransactionLimit_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    MonthlyTransactionLimit_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.CheckConstraint("CK_Accounts_Audit", "[Version] > 0 AND [UpdatedAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankAccountMandates",
                schema: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorizedPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Permissions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ExpirationUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountMandates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccountMandates_Accounts_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "accounts",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankAccountOwnerships",
                schema: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    StartDateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountOwnerships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccountOwnerships_Accounts_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "accounts",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankAccountRestrictions",
                schema: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EffectiveAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RemovedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountRestrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccountRestrictions_Accounts_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "accounts",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankAccountStatusHistory",
                schema: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccountStatusHistory_Accounts_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "accounts",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountNumber",
                schema: "accounts",
                table: "Accounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Iban",
                schema: "accounts",
                table: "Accounts",
                column: "Iban",
                unique: true,
                filter: "[Iban] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PrimaryOwnerId",
                schema: "accounts",
                table: "Accounts",
                column: "PrimaryOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountMandates_ParentId",
                schema: "accounts",
                table: "BankAccountMandates",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountOwnerships_ParentId",
                schema: "accounts",
                table: "BankAccountOwnerships",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountRestrictions_ParentId",
                schema: "accounts",
                table: "BankAccountRestrictions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountStatusHistory_ParentId",
                schema: "accounts",
                table: "BankAccountStatusHistory",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_OccurredAtUtc",
                schema: "accounts",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccountMandates",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "BankAccountOwnerships",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "BankAccountRestrictions",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "BankAccountStatusHistory",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "accounts");
        }
    }
}
