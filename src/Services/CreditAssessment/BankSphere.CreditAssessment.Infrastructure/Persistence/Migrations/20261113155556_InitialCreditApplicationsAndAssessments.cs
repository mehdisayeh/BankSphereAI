using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSphere.CreditAssessment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreditApplicationsAndAssessments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "creditassessment");

            migrationBuilder.CreateTable(
                name: "Applications",
                schema: "creditassessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanProductId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LoanPurpose = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RequestedAmount_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    RequestedAmount_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    RequestedTermMonths = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmissionChannel = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AssignedCreditOfficerId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.CheckConstraint("CK_Applications_Audit", "[Version] > 0 AND [UpdatedAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "creditassessment",
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
                name: "CreditApplicationAssessments",
                schema: "creditassessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Assessor = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PolicyVersion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DebtToIncomeRatio = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    DisposableMonthlyIncome = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AssessedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplicationAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplicationAssessments_Applications_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "creditassessment",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplicationCollateral",
                schema: "creditassessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProtectedReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DeclaredValue = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplicationCollateral", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplicationCollateral_Applications_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "creditassessment",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplicationDecisionHistory",
                schema: "creditassessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvidenceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplicationDecisionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplicationDecisionHistory_Applications_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "creditassessment",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplicationDocuments",
                schema: "creditassessment",
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
                    table.PrimaryKey("PK_CreditApplicationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplicationDocuments_Applications_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "creditassessment",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplicationExistingDebts",
                schema: "creditassessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LenderReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    MonthlyPayment = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplicationExistingDebts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplicationExistingDebts_Applications_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "creditassessment",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplicationFinancial",
                schema: "creditassessment",
                columns: table => new
                {
                    CreditApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentType = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DeclaredMonthlyIncome_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    DeclaredMonthlyIncome_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    DeclaredMonthlyExpenses_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    DeclaredMonthlyExpenses_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    ExistingMonthlyDebtPayments_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    ExistingMonthlyDebtPayments_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    HousingExpenses_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    HousingExpenses_Currency = table.Column<string>(type: "char(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplicationFinancial", x => x.CreditApplicationId);
                    table.ForeignKey(
                        name: "FK_CreditApplicationFinancial_Applications_CreditApplicationId",
                        column: x => x.CreditApplicationId,
                        principalSchema: "creditassessment",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplicationIncomeSources",
                schema: "creditassessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvidenceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplicationIncomeSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplicationIncomeSources_Applications_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "creditassessment",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplicationObligations",
                schema: "creditassessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplicationObligations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplicationObligations_Applications_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "creditassessment",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplicationRecommendations",
                schema: "creditassessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Recommendation = table.Column<int>(type: "int", nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvidenceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplicationRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplicationRecommendations_Applications_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "creditassessment",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplicationReviewRequests",
                schema: "creditassessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reviewer = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplicationReviewRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplicationReviewRequests_Applications_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "creditassessment",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditAssessmentRiskFactors",
                schema: "creditassessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvidenceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditAssessmentRiskFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditAssessmentRiskFactors_CreditApplicationAssessments_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "creditassessment",
                        principalTable: "CreditApplicationAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicationNumber",
                schema: "creditassessment",
                table: "Applications",
                column: "ApplicationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CustomerId_Status",
                schema: "creditassessment",
                table: "Applications",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplicationAssessments_ParentId",
                schema: "creditassessment",
                table: "CreditApplicationAssessments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplicationCollateral_ParentId",
                schema: "creditassessment",
                table: "CreditApplicationCollateral",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplicationDecisionHistory_ParentId",
                schema: "creditassessment",
                table: "CreditApplicationDecisionHistory",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplicationDocuments_ParentId",
                schema: "creditassessment",
                table: "CreditApplicationDocuments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplicationExistingDebts_ParentId",
                schema: "creditassessment",
                table: "CreditApplicationExistingDebts",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplicationIncomeSources_ParentId",
                schema: "creditassessment",
                table: "CreditApplicationIncomeSources",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplicationObligations_ParentId",
                schema: "creditassessment",
                table: "CreditApplicationObligations",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplicationRecommendations_ParentId",
                schema: "creditassessment",
                table: "CreditApplicationRecommendations",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplicationReviewRequests_ParentId",
                schema: "creditassessment",
                table: "CreditApplicationReviewRequests",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditAssessmentRiskFactors_ParentId",
                schema: "creditassessment",
                table: "CreditAssessmentRiskFactors",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_OccurredAtUtc",
                schema: "creditassessment",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditApplicationCollateral",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "CreditApplicationDecisionHistory",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "CreditApplicationDocuments",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "CreditApplicationExistingDebts",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "CreditApplicationFinancial",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "CreditApplicationIncomeSources",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "CreditApplicationObligations",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "CreditApplicationRecommendations",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "CreditApplicationReviewRequests",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "CreditAssessmentRiskFactors",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "CreditApplicationAssessments",
                schema: "creditassessment");

            migrationBuilder.DropTable(
                name: "Applications",
                schema: "creditassessment");
        }
    }
}
