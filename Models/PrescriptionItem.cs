namespace ClinicManagementSystem.Models
{
    public class PrescriptionItem
    {
        public int Id { get; set; }

        public int VisitId { get; set; }
        public Visit Visit { get; set; } = null!;

        public string MedicationName { get; set; } = string.Empty;

        public string Dosage { get; set; } = string.Empty;

        public string Frequency { get; set; } = string.Empty;

        public string Duration { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
