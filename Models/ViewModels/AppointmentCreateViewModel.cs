namespace ClinicManagementSystem.Models.ViewModels;

public class AppointmentCreateViewModel
{
    [Required(ErrorMessage = "يرجى اختيار المريض")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "يرجى اختيار الطبيب")]
    public int DoctorId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; } = DateTime.Now;

    public IEnumerable<SelectListItem> Patients { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Doctors { get; set; } = Enumerable.Empty<SelectListItem>();
}
