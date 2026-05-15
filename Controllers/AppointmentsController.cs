namespace ClinicManagementSystem.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IPatientService patientService,
        IDoctorService doctorService)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
        _doctorService = doctorService;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;

        // Get active (non-cancelled) appointments for the selected date
        var appointments = await _appointmentService.GetActiveQueueAsync(selectedDate);

        // Group by doctor and separate into scheduled and walk-in queues
        var model = new AppointmentsIndexViewModel
        {
            SelectedDate = selectedDate,
            DoctorQueues = appointments
                .GroupBy(x => new { x.DoctorId, DoctorName = x.Doctor?.Name ?? "-" })
                .Select(group => new DoctorQueueTabViewModel
                {
                    DoctorId = group.Key.DoctorId,
                    DoctorName = group.Key.DoctorName,

                    // Scheduled Queue (IsWalkIn == false && Status == Waiting)
                    ScheduledWaitingCount = group.Count(x => !x.IsWalkIn && x.Status == AppointmentStatus.Waiting),
                    ScheduledQueue = group
                        .Where(x => !x.IsWalkIn)
                        .OrderBy(x => x.QueueNumber)
                        .Select(x => MapToListItemViewModel(x))
                        .ToList(),

                    // Walk-In Queue (IsWalkIn == true && Status == Waiting)
                    WalkInWaitingCount = group.Count(x => x.IsWalkIn && x.Status == AppointmentStatus.Waiting),
                    WalkInQueue = group
                        .Where(x => x.IsWalkIn)
                        .OrderBy(x => x.QueueNumber)
                        .Select(x => MapToListItemViewModel(x))
                        .ToList()
                })
                .OrderBy(x => x.DoctorName)
                .ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildCreateModelAsync(new AppointmentCreateViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildCreateModelAsync(model));
        }

        await _appointmentService.CreateAsync(model.PatientId, model.DoctorId, model.Date);
        return RedirectToAction(nameof(Index), new { date = model.Date.ToString("yyyy-MM-dd") });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, DateTime date)
    {
        try
        {
            await _appointmentService.CancelAsync(id);
            TempData["SuccessMessage"] = "تم إلغاء الموعد بنجاح";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"حدث خطأ أثناء إلغاء الموعد: {ex.Message}";
        }
        return RedirectToAction(nameof(Index), new { date = date.ToString("yyyy-MM-dd") });
    }

    private async Task<AppointmentCreateViewModel> BuildCreateModelAsync(AppointmentCreateViewModel model)
    {
        var patients = await _patientService.SearchAsync(null);
        var doctors = await _doctorService.GetAllAsync();

        model.Patients = patients.Select(x => new SelectListItem($"{x.Name} - {x.Age}", x.Id.ToString()));
        model.Doctors = doctors.Select(x => new SelectListItem($"{x.Name} ({x.Specialty})", x.Id.ToString()));
        return model;
    }

    private static string GetStatusText(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Done => "مؤكد",
            AppointmentStatus.Cancelled => "ملغي",
            _ => "في الانتظار"
        };
    }

    private static string GetStatusBadgeClass(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Done => "confirmed",
            AppointmentStatus.Cancelled => "cancelled",
            _ => "waiting"
        };
    }

    /// <summary>
    /// Maps an Appointment entity to AppointmentListItemViewModel.
    /// Includes IsWalkIn flag for queue display formatting.
    /// </summary>
    private static AppointmentListItemViewModel MapToListItemViewModel(Appointment appointment)
    {
        return new AppointmentListItemViewModel
        {
            Id = appointment.Id,
            DoctorName = appointment.Doctor?.Name ?? "-",
            PatientName = appointment.Patient?.Name ?? "-",
            Date = appointment.Date,
            Time = appointment.Date.ToString("hh:mm tt"),
            QueueNumber = appointment.QueueNumber,
            IsWalkIn = appointment.IsWalkIn,
            CanCreateVisit = appointment.Status == AppointmentStatus.Waiting,
            StatusText = GetStatusText(appointment.Status),
            StatusBadgeClass = GetStatusBadgeClass(appointment.Status)
        };
    }
}
