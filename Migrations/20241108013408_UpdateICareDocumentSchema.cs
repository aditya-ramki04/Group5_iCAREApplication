using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iCareWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateICareDocumentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Roles_RoleID",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_RoleID",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientTreatments",
                table: "PatientTreatments");

            migrationBuilder.DropColumn(
                name: "LastLogin",
                table: "User");

            migrationBuilder.RenameTable(
                name: "PatientTreatments",
                newName: "PatientTreatment");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Patient",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Patient",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DocumentCount",
                table: "Patient",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "iCareDocuments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientTreatment",
                table: "PatientTreatment",
                column: "PatientTreatmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAssignment_PatientId",
                table: "PatientAssignment",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAssignment_WorkerId",
                table: "PatientAssignment",
                column: "WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientAssignment_Patient_PatientId",
                table: "PatientAssignment",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientAssignment_User_WorkerId",
                table: "PatientAssignment",
                column: "WorkerId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientAssignment_Patient_PatientId",
                table: "PatientAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientAssignment_User_WorkerId",
                table: "PatientAssignment");

            migrationBuilder.DropIndex(
                name: "IX_PatientAssignment_PatientId",
                table: "PatientAssignment");

            migrationBuilder.DropIndex(
                name: "IX_PatientAssignment_WorkerId",
                table: "PatientAssignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientTreatment",
                table: "PatientTreatment");

            migrationBuilder.DropColumn(
                name: "DocumentCount",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "iCareDocuments");

            migrationBuilder.RenameTable(
                name: "PatientTreatment",
                newName: "PatientTreatments");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogin",
                table: "User",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Patient",
                keyColumn: "PhoneNumber",
                keyValue: null,
                column: "PhoneNumber",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Patient",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Patient",
                keyColumn: "Address",
                keyValue: null,
                column: "Address",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Patient",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientTreatments",
                table: "PatientTreatments",
                column: "PatientTreatmentId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleID",
                table: "User",
                column: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Roles_RoleID",
                table: "User",
                column: "RoleID",
                principalTable: "Roles",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
