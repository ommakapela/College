using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ibhayiPharmacy.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Make DoctorId nullable (allow customers to upload without doctor)
            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "Prescriptions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // 2. Update Status column (remove default value, make it required)
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Prescriptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending Processing",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "Uploaded");

            // 3. Add new workflow tracking fields to Prescriptions
            migrationBuilder.AddColumn<DateTime>(
                name: "UploadDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "PatientIDNumber",
                table: "Prescriptions",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploadedById",
                table: "Prescriptions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessedById",
                table: "Prescriptions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DispensedById",
                table: "Prescriptions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DispensedDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDispensed",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReadyForCollection",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomerNotified",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCollected",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CollectionDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Prescriptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // 4. Add UnitPrice to PrescriptionItems (THIS FIXES YOUR ERROR!)
            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "PrescriptionItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // 5. Update existing RepeatsRemaining if needed
            migrationBuilder.Sql(
                "UPDATE PrescriptionItems SET RepeatsRemaining = Repeats WHERE RepeatsRemaining = 0");

            // 6. Create indexes for better performance
            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_Status",
                table: "Prescriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_UploadDate",
                table: "Prescriptions",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ReadyForCollection",
                table: "Prescriptions",
                column: "ReadyForCollection");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_IsDispensed",
                table: "Prescriptions",
                column: "IsDispensed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove indexes
            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_Status",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_UploadDate",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_ReadyForCollection",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_IsDispensed",
                table: "Prescriptions");

            // Remove columns from PrescriptionItems
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "PrescriptionItems");

            // Remove columns from Prescriptions
            migrationBuilder.DropColumn(
                name: "UploadDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "PatientIDNumber",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "UploadedById",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "ProcessedById",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "ProcessedDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "DispensedById",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "DispensedDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "IsDispensed",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "ReadyForCollection",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "CustomerNotified",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "IsCollected",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "CollectionDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Prescriptions");

            // Restore Status to old configuration
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Prescriptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "Uploaded",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Pending Processing");

            // Restore DoctorId as required
            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}