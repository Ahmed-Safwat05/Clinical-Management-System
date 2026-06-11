using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteBehaviorsAndAddVisitIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientMedicalHistoryEntries_Patients_PatientId",
                table: "PatientMedicalHistoryEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Visits_VisitId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitProcedures_Visits_VisitId",
                table: "VisitProcedures");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitProductConsumptions_Visits_VisitId",
                table: "VisitProductConsumptions");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Visits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VisitProcedures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_Date",
                table: "Visits",
                column: "Date");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientMedicalHistoryEntries_Patients_PatientId",
                table: "PatientMedicalHistoryEntries",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Visits_VisitId",
                table: "Payments",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitProcedures_Visits_VisitId",
                table: "VisitProcedures",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitProductConsumptions_Visits_VisitId",
                table: "VisitProductConsumptions",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientMedicalHistoryEntries_Patients_PatientId",
                table: "PatientMedicalHistoryEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Visits_VisitId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitProcedures_Visits_VisitId",
                table: "VisitProcedures");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitProductConsumptions_Visits_VisitId",
                table: "VisitProductConsumptions");

            migrationBuilder.DropIndex(
                name: "IX_Visits_Date",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VisitProcedures");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Appointments");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientMedicalHistoryEntries_Patients_PatientId",
                table: "PatientMedicalHistoryEntries",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Visits_VisitId",
                table: "Payments",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitProcedures_Visits_VisitId",
                table: "VisitProcedures",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitProductConsumptions_Visits_VisitId",
                table: "VisitProductConsumptions",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
