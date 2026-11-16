using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSphere.FraudInvestigation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialInvestigationCasesAndActiveDeduplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fraudinvestigation");

            migrationBuilder.CreateTable(
                name: "Cases",
                schema: "fraudinvestigation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseType = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    AssignedInvestigatorId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AssignedTeamId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DetectionSource = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DetectionTimestampUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RiskScore = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SuspectedFraudAmount_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    SuspectedFraudAmount_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    EstimatedFinancialExposure_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    EstimatedFinancialExposure_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    InvestigationStartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    InvestigationClosedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ResolutionCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolutionSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewDueAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                    table.CheckConstraint("CK_Cases_Audit", "[Version] > 0 AND [UpdatedAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "fraudinvestigation",
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
                name: "FraudCaseAssignments",
                schema: "fraudinvestigation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Investigator = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Team = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AssignedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudCaseAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FraudCaseAssignments_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "fraudinvestigation",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FraudCaseDecisions",
                schema: "fraudinvestigation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Classification = table.Column<int>(type: "int", nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Reviewer = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvidenceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudCaseDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FraudCaseDecisions_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "fraudinvestigation",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FraudCaseEvidence",
                schema: "fraudinvestigation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ContentReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IntegrityHash = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CollectedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudCaseEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FraudCaseEvidence_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "fraudinvestigation",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FraudCaseHumanReviews",
                schema: "fraudinvestigation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reviewer = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudCaseHumanReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FraudCaseHumanReviews_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "fraudinvestigation",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FraudCaseIndicators",
                schema: "fraudinvestigation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    EvidenceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DetectedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudCaseIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FraudCaseIndicators_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "fraudinvestigation",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FraudCaseNotes",
                schema: "fraudinvestigation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Visibility = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudCaseNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FraudCaseNotes_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "fraudinvestigation",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FraudCaseStatusHistory",
                schema: "fraudinvestigation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudCaseStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FraudCaseStatusHistory_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "fraudinvestigation",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FraudCaseTransactionLinks",
                schema: "fraudinvestigation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssociationReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudCaseTransactionLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FraudCaseTransactionLinks_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "fraudinvestigation",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_AccountId_Status",
                schema: "fraudinvestigation",
                table: "Cases",
                columns: new[] { "AccountId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CaseNumber",
                schema: "fraudinvestigation",
                table: "Cases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_TransactionId_Category",
                schema: "fraudinvestigation",
                table: "Cases",
                columns: new[] { "TransactionId", "Category" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_FraudCaseAssignments_ParentId",
                schema: "fraudinvestigation",
                table: "FraudCaseAssignments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FraudCaseDecisions_ParentId",
                schema: "fraudinvestigation",
                table: "FraudCaseDecisions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FraudCaseEvidence_ParentId",
                schema: "fraudinvestigation",
                table: "FraudCaseEvidence",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FraudCaseHumanReviews_ParentId",
                schema: "fraudinvestigation",
                table: "FraudCaseHumanReviews",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FraudCaseIndicators_ParentId",
                schema: "fraudinvestigation",
                table: "FraudCaseIndicators",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FraudCaseNotes_ParentId",
                schema: "fraudinvestigation",
                table: "FraudCaseNotes",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FraudCaseStatusHistory_ParentId",
                schema: "fraudinvestigation",
                table: "FraudCaseStatusHistory",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FraudCaseTransactionLinks_ParentId",
                schema: "fraudinvestigation",
                table: "FraudCaseTransactionLinks",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_OccurredAtUtc",
                schema: "fraudinvestigation",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FraudCaseAssignments",
                schema: "fraudinvestigation");

            migrationBuilder.DropTable(
                name: "FraudCaseDecisions",
                schema: "fraudinvestigation");

            migrationBuilder.DropTable(
                name: "FraudCaseEvidence",
                schema: "fraudinvestigation");

            migrationBuilder.DropTable(
                name: "FraudCaseHumanReviews",
                schema: "fraudinvestigation");

            migrationBuilder.DropTable(
                name: "FraudCaseIndicators",
                schema: "fraudinvestigation");

            migrationBuilder.DropTable(
                name: "FraudCaseNotes",
                schema: "fraudinvestigation");

            migrationBuilder.DropTable(
                name: "FraudCaseStatusHistory",
                schema: "fraudinvestigation");

            migrationBuilder.DropTable(
                name: "FraudCaseTransactionLinks",
                schema: "fraudinvestigation");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "fraudinvestigation");

            migrationBuilder.DropTable(
                name: "Cases",
                schema: "fraudinvestigation");
        }
    }
}
