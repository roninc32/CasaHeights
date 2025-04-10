using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasaHeights.Migrations
{
    /// <inheritdoc />
    public partial class FixingReservationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationDate",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Reservations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectionDate",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CancellationDate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RejectionDate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Reservations");
        }
    }
}
