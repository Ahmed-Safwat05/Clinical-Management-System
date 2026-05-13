namespace ClinicManagementSystem.Models;

public enum AuditActionType
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Login = 4,
    Logout = 5,
    InventoryConsume = 6,
    InventoryAdd = 7,
    PasswordChange = 8,
    Activate = 9,
    Deactivate = 10
}
