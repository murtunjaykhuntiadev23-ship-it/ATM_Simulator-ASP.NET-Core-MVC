using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATM_Simulator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPinLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedPinAttempts",
                table: "BankAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntil",
                table: "BankAccounts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedPinAttempts",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "LockedUntil",
                table: "BankAccounts");
        }
    }
}
