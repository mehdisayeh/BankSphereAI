using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSphere.Customers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCustomerProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customers");

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Segment = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CustomerSince = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OnboardingChannel = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PreferredCommunicationChannel = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RelationshipManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    KycStatus = table.Column<int>(type: "int", nullable: false),
                    KycLastReviewedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    KycNextReviewAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.CheckConstraint("CK_Customers_Audit", "[Version] > 0 AND [UpdatedAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "customers",
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
                name: "CustomerAddresses",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    City = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EffectiveFromUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EffectiveToUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerAddresses_Customers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerBeneficialOwners",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerReference = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnershipPercentage = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    ControlType = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerBeneficialOwners", x => x.Id);
                    table.CheckConstraint("CK_BeneficialOwnership_Percentage", "[OwnershipPercentage] >= 0 AND [OwnershipPercentage] <= 100");
                    table.ForeignKey(
                        name: "FK_CustomerBeneficialOwners_Customers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerConsents",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PolicyVersion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    GrantedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    WithdrawnAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerConsents_Customers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerContacts",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Verified = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerContacts_Customers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerCorporate",
                schema: "customers",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegisteredBusinessName = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IncorporationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LegalForm = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RegistrationCountry = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCorporate", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_CustomerCorporate_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerCountries",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCountries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerCountries_Customers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerFinancial",
                schema: "customers",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Occupation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EmploymentStatus = table.Column<int>(type: "int", nullable: false),
                    EmployerName = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AnnualIncomeRange = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SourceOfFunds = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SourceOfWealth = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ExpectedAccountActivity = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFinancial", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_CustomerFinancial_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerIdentityDocuments",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IssuingCountry = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ProtectedReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Verified = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerIdentityDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerIdentityDocuments_Customers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerIndividual",
                schema: "customers",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalFirstName = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LegalLastName = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PreferredName = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CountryOfBirth = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerIndividual", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_CustomerIndividual_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerRelationships",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelatedCustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EffectiveFromUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EffectiveToUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerRelationships_Customers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerRiskProfiles",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Methodology = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AssessedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReassessAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRiskProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerRiskProfiles_Customers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerRiskFactors",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRiskFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerRiskFactors_CustomerRiskProfiles_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "customers",
                        principalTable: "CustomerRiskProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddresses_ParentId",
                schema: "customers",
                table: "CustomerAddresses",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBeneficialOwners_ParentId",
                schema: "customers",
                table: "CustomerBeneficialOwners",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerConsents_ParentId",
                schema: "customers",
                table: "CustomerConsents",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_ParentId",
                schema: "customers",
                table: "CustomerContacts",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCountries_ParentId",
                schema: "customers",
                table: "CustomerCountries",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerIdentityDocuments_ParentId",
                schema: "customers",
                table: "CustomerIdentityDocuments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRelationships_ParentId",
                schema: "customers",
                table: "CustomerRelationships",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRiskFactors_ParentId",
                schema: "customers",
                table: "CustomerRiskFactors",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRiskProfiles_ParentId",
                schema: "customers",
                table: "CustomerRiskProfiles",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerNumber",
                schema: "customers",
                table: "Customers",
                column: "CustomerNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_OccurredAtUtc",
                schema: "customers",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerAddresses",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerBeneficialOwners",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerConsents",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerContacts",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerCorporate",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerCountries",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerFinancial",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerIdentityDocuments",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerIndividual",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerRelationships",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerRiskFactors",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerRiskProfiles",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "customers");
        }
    }
}
