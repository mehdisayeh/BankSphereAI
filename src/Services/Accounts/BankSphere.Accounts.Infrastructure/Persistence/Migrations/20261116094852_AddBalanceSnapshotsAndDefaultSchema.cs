using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSphere.Accounts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceSnapshotsAndDefaultSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BalanceSnapshots",
                schema: "accounts",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Snapshot_BookedBalance_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Snapshot_BookedBalance_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    Snapshot_AvailableBalance_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Snapshot_AvailableBalance_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    Snapshot_PendingAmount_Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Snapshot_PendingAmount_Currency = table.Column<string>(type: "char(3)", nullable: false),
                    Snapshot_AsOfUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Snapshot_LedgerReference = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Snapshot_Source = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceSnapshots", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_BalanceSnapshots_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "accounts",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceSnapshots",
                schema: "accounts");
        }
    }
}
