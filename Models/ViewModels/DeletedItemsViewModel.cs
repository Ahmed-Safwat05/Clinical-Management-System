namespace ClinicManagementSystem.Models.ViewModels;

public class DeletedItemsViewModel
{
    public IReadOnlyList<Patient> DeletedPatients { get; set; } = Array.Empty<Patient>();
    public IReadOnlyList<Doctor> DeletedDoctors { get; set; } = Array.Empty<Doctor>();
}
