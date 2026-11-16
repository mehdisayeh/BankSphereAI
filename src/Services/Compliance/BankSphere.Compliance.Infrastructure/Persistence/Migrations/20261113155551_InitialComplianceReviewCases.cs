using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSphere.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialComplianceReviewCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "compliance");

            migrationBuilder.CreateTable(
                name: "Cases",
                schema: "compliance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RiskClassification = table.Column<int>(type: "int", nullable: false),
                    RegulatoryJurisdiction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ReviewReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ScreeningSource = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ComplianceOfficerId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OpenedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Decision = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DecisionRationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                schema: "compliance",
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
                name: "ComplianceCaseDocuments",
                schema: "compliance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProtectedReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AddedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceCaseDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceCaseDocuments_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "compliance",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceCaseDueDiligence",
                schema: "compliance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enhanced = table.Column<bool>(type: "bit", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SourceOfFundsAssessment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Officer = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvidenceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceCaseDueDiligence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceCaseDueDiligence_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "compliance",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceCaseKycReviews",
                schema: "compliance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NextReviewAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Officer = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Findings = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvidenceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceCaseKycReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceCaseKycReviews_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "compliance",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceCaseReviewHistory",
                schema: "compliance",
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
                    table.PrimaryKey("PK_ComplianceCaseReviewHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceCaseReviewHistory_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "compliance",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceCaseRiskFactors",
                schema: "compliance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Classification = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Assessor = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceCaseRiskFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceCaseRiskFactors_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "compliance",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceCaseScreenings",
                schema: "compliance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ScreeningSource = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProtectedReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PotentialMatch = table.Column<bool>(type: "bit", nullable: false),
                    Disposition = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Officer = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceCaseScreenings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceCaseScreenings_Cases_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "compliance",
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CaseNumber",
                schema: "compliance",
                table: "Cases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CustomerId_Status",
                schema: "compliance",
                table: "Cases",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceCaseDocuments_ParentId",
                schema: "compliance",
                table: "ComplianceCaseDocuments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceCaseDueDiligence_ParentId",
                schema: "compliance",
                table: "ComplianceCaseDueDiligence",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceCaseKycReviews_ParentId",
                schema: "compliance",
                table: "ComplianceCaseKycReviews",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceCaseReviewHistory_ParentId",
                schema: "compliance",
                table: "ComplianceCaseReviewHistory",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceCaseRiskFactors_ParentId",
                schema: "compliance",
                table: "ComplianceCaseRiskFactors",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceCaseScreenings_ParentId",
                schema: "compliance",
                table: "ComplianceCaseScreenings",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_OccurredAtUtc",
                schema: "compliance",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplianceCaseDocuments",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "ComplianceCaseDueDiligence",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "ComplianceCaseKycReviews",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "ComplianceCaseReviewHistory",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "ComplianceCaseRiskFactors",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "ComplianceCaseScreenings",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "Cases",
                schema: "compliance");
        }
    }
}
