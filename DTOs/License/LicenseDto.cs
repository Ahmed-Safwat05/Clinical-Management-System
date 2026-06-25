namespace ClinicManagementSystem.DTOs.License
{
    public class LicenseDto
    {
        public string? ClinicName { get; set; }
        public string? LicenseType { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? Version { get; set; }    
        public bool IsDeviceValid { get; set; } = true;
        public string DeviceFingerprint { get; set; } = string.Empty;
        public bool IsTrialExpired => DateTime.Today.Date > ExpirationDate.Date;
        public int DaysRemaining => (ExpirationDate.Date - DateTime.Today).Days;
        public bool IsLifetime => LicenseType == "Lifetime" || ExpirationDate > DateTime.Today.AddYears(50);

    }
}
