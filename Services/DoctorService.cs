using ClinicManagementSystem.Interfaces.Repositories;
using ClinicManagementSystem.Interfaces.Services;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctors;
    private readonly IAuditService _auditService;

    public DoctorService(IDoctorRepository doctors, IAuditService auditService)
    {
        _doctors = doctors;
        _auditService = auditService;
    }

    public Task<IReadOnlyList<Doctor>> GetAllAsync() => _doctors.GetWithSchedulesAsync();

    public Task<Doctor?> GetByIdAsync(int id) => _doctors.GetByIdAsync(id);

    public Task<IReadOnlyList<Doctor>> GetDeletedAsync() => _doctors.GetDeletedAsync();

    public async Task CreateAsync(Doctor doctor)
    {
        await _doctors.AddAsync(doctor);
        await _doctors.SaveChangesAsync();

        await _auditService.LogAsync(
            AuditActionType.Create,
            nameof(Doctor),
            $"Created doctor {doctor.Name}",
            doctor.Id);
    }

    public async Task UpdateAsync(Doctor doctor)
    {
        _doctors.Update(doctor);
        await _doctors.SaveChangesAsync();

        await _auditService.LogAsync(
            AuditActionType.Update,
            nameof(Doctor),
            $"Updated doctor {doctor.Name}",
            doctor.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var doctor = await _doctors.GetByIdAsync(id);
        if (doctor is null)
        {
            return;
        }

        doctor.IsDeleted = true;
        _doctors.Update(doctor);
        await _doctors.SaveChangesAsync();

        await _auditService.LogAsync(
            AuditActionType.Delete,
            nameof(Doctor),
            $"Deleted doctor {doctor.Name}",
            doctor.Id);
    }

    public async Task RestoreAsync(int id)
    {
        var doctor = await _doctors.GetDeletedByIdAsync(id);
        if (doctor is null)
        {
            return;
        }

        doctor.IsDeleted = false;
        _doctors.Update(doctor);
        await _doctors.SaveChangesAsync();
    }
}
