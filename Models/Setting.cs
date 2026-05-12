namespace ClinicManagementSystem.Models;

public class Setting
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Key { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Value { get; set; } = string.Empty;
}
