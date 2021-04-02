using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Infrastructure.DataAccess.EfCoreDataAccess.Migrations
{
    public partial class Added_Cascade_Delete_For_Transactions_And_Rowversioning_For_Wallet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_WalletJmbg",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "Ballance",
                table: "Wallets",
                newName: "Balance");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Wallets",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Destination",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_WalletJmbg",
                table: "Transactions",
                column: "WalletJmbg",
                principalTable: "Wallets",
                principalColumn: "Jmbg",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_WalletJmbg",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Wallets");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "Wallets",
                newName: "Ballance");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "Transactions",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Destination",
                table: "Transactions",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_WalletJmbg",
                table: "Transactions",
                column: "WalletJmbg",
                principalTable: "Wallets",
                principalColumn: "Jmbg",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
