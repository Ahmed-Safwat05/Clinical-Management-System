namespace ClinicManagementSystem.Interfaces.Services
{
    public interface IPrescriptionItemService
    {
        Task<PrescriptionItem?> CreateAsync(PrescriptionItem item);
        Task<bool> UpdateAsync(PrescriptionItem item);
        Task<bool> DeleteAsync(int id);
    }
}
