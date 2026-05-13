using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ibhayiPharmacy.Migrations
{
    /// <inheritdoc />
    public partial class FixPrescriptionRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "Prescriptions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Prescriptions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CollectionDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomerNotified",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AddColumn<int>(
                name: "DoctorId1",
                table: "Prescriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCollected",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDispensed",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PatientIDNumber",
                table: "Prescriptions",
                type: "nvarchar(13)",
                maxLength: 13,
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

            migrationBuilder.AddColumn<bool>(
                name: "ReadyForCollection",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Prescriptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UploadedById",
                table: "Prescriptions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedicationId1",
                table: "PrescriptionItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "PrescriptionItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DosageFormId1",
                table: "Medications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId1",
                table: "Medications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ApplicationUserId",
                table: "Prescriptions",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DispensedById",
                table: "Prescriptions",
                column: "DispensedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DoctorId1",
                table: "Prescriptions",
                column: "DoctorId1");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_IsDispensed",
                table: "Prescriptions",
                column: "IsDispensed");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ProcessedById",
                table: "Prescriptions",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ReadyForCollection",
                table: "Prescriptions",
                column: "ReadyForCollection");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_Status",
                table: "Prescriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_UploadDate",
                table: "Prescriptions",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_UploadedById",
                table: "Prescriptions",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_MedicationId1",
                table: "PrescriptionItems",
                column: "MedicationId1");

            migrationBuilder.CreateIndex(
                name: "IX_Medications_DosageFormId1",
                table: "Medications",
                column: "DosageFormId1");

            migrationBuilder.CreateIndex(
                name: "IX_Medications_SupplierId1",
                table: "Medications",
                column: "SupplierId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Medications_DosageForms_DosageFormId1",
                table: "Medications",
                column: "DosageFormId1",
                principalTable: "DosageForms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medications_Suppliers_SupplierId1",
                table: "Medications",
                column: "SupplierId1",
                principalTable: "Suppliers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PrescriptionItems_Medications_MedicationId1",
                table: "PrescriptionItems",
                column: "MedicationId1",
                principalTable: "Medications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_ApplicationUserId",
                table: "Prescriptions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_DispensedById",
                table: "Prescriptions",
                column: "DispensedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_ProcessedById",
                table: "Prescriptions",
                column: "ProcessedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_UploadedById",
                table: "Prescriptions",
                column: "UploadedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Doctors_DoctorId1",
                table: "Prescriptions",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medications_DosageForms_DosageFormId1",
                table: "Medications");

            migrationBuilder.DropForeignKey(
                name: "FK_Medications_Suppliers_SupplierId1",
                table: "Medications");

            migrationBuilder.DropForeignKey(
                name: "FK_PrescriptionItems_Medications_MedicationId1",
                table: "PrescriptionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_ApplicationUserId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_DispensedById",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_ProcessedById",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_UploadedById",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Doctors_DoctorId1",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_ApplicationUserId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_DispensedById",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_DoctorId1",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_IsDispensed",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_ProcessedById",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_ReadyForCollection",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_Status",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_UploadDate",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_UploadedById",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_PrescriptionItems_MedicationId1",
                table: "PrescriptionItems");

            migrationBuilder.DropIndex(
                name: "IX_Medications_DosageFormId1",
                table: "Medications");

            migrationBuilder.DropIndex(
                name: "IX_Medications_SupplierId1",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "CollectionDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "CustomerNotified",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "DispensedById",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "DispensedDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "IsCollected",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "IsDispensed",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "PatientIDNumber",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "ProcessedById",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "ProcessedDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "ReadyForCollection",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "UploadDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "UploadedById",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "MedicationId1",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "DosageFormId1",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "SupplierId1",
                table: "Medications");

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

            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
