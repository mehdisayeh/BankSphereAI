using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSphere.Transactions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSyntheticTransactionsAndOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "transactions");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "transactions",
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
                name: "Transactions",
                schema: "transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExternalTransactionReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Amount_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    OriginalAmount_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    OriginalAmount_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    ExchangeRateReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TransactionDateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    BookingDateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ValueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentChannel = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PaymentScheme = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RemittanceInformation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TerminalId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DeviceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IpAddressReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GeoLocationReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InitiationChannel = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SimulatorBatchId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SimulationScenarioId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsSynthetic = table.Column<bool>(type: "bit", nullable: false),
                    PayloadFingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.CheckConstraint("CK_Transactions_Audit", "[Version] > 0 AND [UpdatedAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                    table.CheckConstraint("CK_Transactions_Synthetic", "[IsSynthetic] = 1");
                });

            migrationBuilder.CreateTable(
                name: "BankingTransactionCounterparty",
                schema: "transactions",
                columns: table => new
                {
                    BankingTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProtectedAccountReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    BankIdentifier = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingTransactionCounterparty", x => x.BankingTransactionId);
                    table.ForeignKey(
                        name: "FK_BankingTransactionCounterparty_Transactions_BankingTransactionId",
                        column: x => x.BankingTransactionId,
                        principalSchema: "transactions",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankingTransactionEnrichments",
                schema: "transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProtectedReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingTransactionEnrichments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankingTransactionEnrichments_Transactions_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "transactions",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankingTransactionInstrument",
                schema: "transactions",
                columns: table => new
                {
                    BankingTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProtectedReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingTransactionInstrument", x => x.BankingTransactionId);
                    table.ForeignKey(
                        name: "FK_BankingTransactionInstrument_Transactions_BankingTransactionId",
                        column: x => x.BankingTransactionId,
                        principalSchema: "transactions",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankingTransactionMerchant",
                schema: "transactions",
                columns: table => new
                {
                    BankingTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CategoryCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    City = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingTransactionMerchant", x => x.BankingTransactionId);
                    table.ForeignKey(
                        name: "FK_BankingTransactionMerchant_Transactions_BankingTransactionId",
                        column: x => x.BankingTransactionId,
                        principalSchema: "transactions",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankingTransactionMetadata",
                schema: "transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingTransactionMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankingTransactionMetadata_Transactions_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "transactions",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankingTransactionRiskSignals",
                schema: "transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvidenceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DetectedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingTransactionRiskSignals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankingTransactionRiskSignals_Transactions_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "transactions",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankingTransactionStatusHistory",
                schema: "transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingTransactionStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankingTransactionStatusHistory_Transactions_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "transactions",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankingTransactionEnrichments_ParentId",
                schema: "transactions",
                table: "BankingTransactionEnrichments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BankingTransactionMetadata_ParentId",
                schema: "transactions",
                table: "BankingTransactionMetadata",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BankingTransactionRiskSignals_ParentId",
                schema: "transactions",
                table: "BankingTransactionRiskSignals",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BankingTransactionStatusHistory_ParentId",
                schema: "transactions",
                table: "BankingTransactionStatusHistory",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_OccurredAtUtc",
                schema: "transactions",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId_TransactionDateUtc_Id",
                schema: "transactions",
                table: "Transactions",
                columns: new[] { "AccountId", "TransactionDateUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Source_ExternalTransactionReference",
                schema: "transactions",
                table: "Transactions",
                columns: new[] { "Source", "ExternalTransactionReference" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankingTransactionCounterparty",
                schema: "transactions");

            migrationBuilder.DropTable(
                name: "BankingTransactionEnrichments",
                schema: "transactions");

            migrationBuilder.DropTable(
                name: "BankingTransactionInstrument",
                schema: "transactions");

            migrationBuilder.DropTable(
                name: "BankingTransactionMerchant",
                schema: "transactions");

            migrationBuilder.DropTable(
                name: "BankingTransactionMetadata",
                schema: "transactions");

            migrationBuilder.DropTable(
                name: "BankingTransactionRiskSignals",
                schema: "transactions");

            migrationBuilder.DropTable(
                name: "BankingTransactionStatusHistory",
                schema: "transactions");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "transactions");

            migrationBuilder.DropTable(
                name: "Transactions",
                schema: "transactions");
        }
    }
}
