namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IPatientMedicalHistoryRepository : IRepository<PatientMedicalHistoryEntry>
{
    Task<IReadOnlyList<PatientMedicalHistoryEntry>> GetByPatientIdAsync(int patientId);
    Task<PatientMedicalHistoryEntry?> GetByIdWithPatientAsync(int id);
}
