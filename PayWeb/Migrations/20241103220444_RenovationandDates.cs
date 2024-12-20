using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class RenovationandDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "datetime",
                nullable: false,
                defaultValueSql: "'1900-01-01'");

            migrationBuilder.AddColumn<string>(
                name: "CreationUser",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificationDate",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RenovationCertificate",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationDate",
                schema: "Finansii",
                table: "CertificatesDeposit");

            migrationBuilder.DropColumn(
                name: "CreationUser",
                schema: "Finansii",
                table: "CertificatesDeposit");

            migrationBuilder.DropColumn(
                name: "ModificationDate",
                schema: "Finansii",
                table: "CertificatesDeposit");

            migrationBuilder.DropColumn(
                name: "RenovationCertificate",
                schema: "Finansii",
                table: "CertificatesDeposit");
        }
    }
}
