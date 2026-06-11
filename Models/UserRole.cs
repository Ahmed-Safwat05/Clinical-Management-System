namespace ClinicManagementSystem.Models;

public enum UserRole
{
    [Display(Name = "مدير نظام (Admin)")]
    Admin = 0,

    [Display(Name = "موظف استقبال (Receptionist)")]
    Receptionist = 1
}
