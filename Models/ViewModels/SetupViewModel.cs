namespace ClinicManagementSystem.Models.ViewModels
{
    public class SetupViewModel
    {
        public string ClinicName { get; set; } = null!;
        public string ClinicPhone { get; set; } = null!;
        public string ClinicAddress { get; set; } = null!;
        public string ClinicTagLine { get; set; } = null!;
        public IFormFile? ClinicLogo { get; set; }
        public string ReceiptFooter { get; set; } = null!;
        public string AdminDisplayName { get; set; } = null!; // مطابقة للـ AppUser
        public string AdminUsername { get; set; } = null!;
        public string AdminPassword { get; set; } = null!;
        public decimal DefaultExamPrice { get; set; }
        public decimal MaxDiscount { get; set; }
        public bool AllowDiscount { get; set; }
    }
}
