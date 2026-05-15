namespace ClinicManagementSystem.Interfaces.Services;

public interface IPatientMedicalHistoryService
{
    Task<PatientMedicalHistoryViewModel?> GetPatientHistoryAsync(int patientId);
    Task<PatientMedicalHistoryEntryFormViewModel?> BuildCreateFormAsync(int patientId);
    Task<PatientMedicalHistoryEntryFormViewModel?> BuildEditFormAsync(int entryId);
    Task<int?> CreateAsync(PatientMedicalHistoryEntryFormViewModel model);
    Task<int?> UpdateAsync(PatientMedicalHistoryEntryFormViewModel model);
    Task<int?> DeleteAsync(int entryId);
}
