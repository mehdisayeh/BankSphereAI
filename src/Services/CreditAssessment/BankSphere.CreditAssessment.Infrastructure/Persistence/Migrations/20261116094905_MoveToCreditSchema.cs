using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSphere.CreditAssessment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveToCreditSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "credit");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                schema: "creditassessment",
                newName: "OutboxMessages",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditAssessmentRiskFactors",
                schema: "creditassessment",
                newName: "CreditAssessmentRiskFactors",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditApplicationReviewRequests",
                schema: "creditassessment",
                newName: "CreditApplicationReviewRequests",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditApplicationRecommendations",
                schema: "creditassessment",
                newName: "CreditApplicationRecommendations",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditApplicationObligations",
                schema: "creditassessment",
                newName: "CreditApplicationObligations",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditApplicationIncomeSources",
                schema: "creditassessment",
                newName: "CreditApplicationIncomeSources",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditApplicationFinancial",
                schema: "creditassessment",
                newName: "CreditApplicationFinancial",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditApplicationExistingDebts",
                schema: "creditassessment",
                newName: "CreditApplicationExistingDebts",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditApplicationDocuments",
                schema: "creditassessment",
                newName: "CreditApplicationDocuments",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditApplicationDecisionHistory",
                schema: "creditassessment",
                newName: "CreditApplicationDecisionHistory",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditApplicationCollateral",
                schema: "creditassessment",
                newName: "CreditApplicationCollateral",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "CreditApplicationAssessments",
                schema: "creditassessment",
                newName: "CreditApplicationAssessments",
                newSchema: "credit");

            migrationBuilder.RenameTable(
                name: "Applications",
                schema: "creditassessment",
                newName: "Applications",
                newSchema: "credit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "creditassessment");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                schema: "credit",
                newName: "OutboxMessages",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditAssessmentRiskFactors",
                schema: "credit",
                newName: "CreditAssessmentRiskFactors",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditApplicationReviewRequests",
                schema: "credit",
                newName: "CreditApplicationReviewRequests",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditApplicationRecommendations",
                schema: "credit",
                newName: "CreditApplicationRecommendations",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditApplicationObligations",
                schema: "credit",
                newName: "CreditApplicationObligations",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditApplicationIncomeSources",
                schema: "credit",
                newName: "CreditApplicationIncomeSources",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditApplicationFinancial",
                schema: "credit",
                newName: "CreditApplicationFinancial",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditApplicationExistingDebts",
                schema: "credit",
                newName: "CreditApplicationExistingDebts",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditApplicationDocuments",
                schema: "credit",
                newName: "CreditApplicationDocuments",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditApplicationDecisionHistory",
                schema: "credit",
                newName: "CreditApplicationDecisionHistory",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditApplicationCollateral",
                schema: "credit",
                newName: "CreditApplicationCollateral",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "CreditApplicationAssessments",
                schema: "credit",
                newName: "CreditApplicationAssessments",
                newSchema: "creditassessment");

            migrationBuilder.RenameTable(
                name: "Applications",
                schema: "credit",
                newName: "Applications",
                newSchema: "creditassessment");
        }
    }
}
