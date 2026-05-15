namespace ClinicManagementSystem.Repositories;

public class PatientMedicalHistoryRepository : Repository<PatientMedicalHistoryEntry>, IPatientMedicalHistoryRepository
{
    public PatientMedicalHistoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PatientMedicalHistoryEntry>> GetByPatientIdAsync(int patientId)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public Task<PatientMedicalHistoryEntry?> GetByIdWithPatientAsync(int id)
    {
        return DbSet
            .Include(x => x.Patient)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
