# Configuration Guide - Post-Bug-Fix Setup

This document provides complete configuration instructions after applying all bug fixes.

---

## 📋 Required Configuration Changes

### 1. appsettings.json - Password Hash Configuration

**BEFORE (VULNERABLE - DO NOT USE):**
```json
{
  "ReceptionistAccount": {
    "Username": "receptionist",
    "Password": "MyPlainTextPassword123!"
  }
}
```

**AFTER (SECURE - USE THIS):**
```json
{
  "ReceptionistAccount": {
    "Username": "receptionist",
    "PasswordHash": "AQAAAAIAAYagAAAAEAXA..."
  }
}
```

### How to Generate Password Hash:

#### Option 1: Using PasswordHashingUtility Class
```csharp
// In Program.cs temporarily or in a utility:
using ClinicManagementSystem.Utilities;

// Generate hash
var hash = PasswordHashingUtility.GenerateHash("receptionist", "YourSecurePassword123!");
Console.WriteLine("PasswordHash: " + hash);

// Copy the output to appsettings.json
```

#### Option 2: Using .NET CLI
```bash
dotnet user-secrets set "ReceptionistAccount:PasswordHash" "AQAAAAIAAYagAAAAE..." --project ClinicManagementSystem.csproj
```

#### Option 3: Manual Generation
```csharp
var hasher = new PasswordHasher<string>();
var hash = hasher.HashPassword("receptionist", "YourSecurePassword123!");
// Copy hash to appsettings.json
```

---

## 🔒 Security Best Practices

### For Development:
```json
{
  "ReceptionistAccount": {
    "Username": "receptionist",
    "PasswordHash": "YOUR_HASHED_DEV_PASSWORD"
  }
}
```

### For Production (Using Azure Key Vault):
```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

### For Production (Using Environment Variables):
```csharp
// In Program.cs
var passwordHash = Environment.GetEnvironmentVariable("RECEPTIONIST_PASSWORD_HASH");
if (string.IsNullOrEmpty(passwordHash))
    throw new InvalidOperationException("RECEPTIONIST_PASSWORD_HASH environment variable not set");
```

---

## 🔐 Complete appsettings.json Example

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ClinicManagementDb;Trusted_Connection=true;Encrypt=false"
  },
  "ReceptionistAccount": {
    "Username": "receptionist",
    "PasswordHash": "AQAAAAIAAYagAAAAEGhv+/4J7p4VnS0..."
  },
  "DataProtection": {
    "KeysPath": ".keys"
  }
}
```

---

## 🌍 Environment-Specific Configurations

### Development (appsettings.Development.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "ClinicManagementSystem.Services": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ReceptionistAccount": {
    "Username": "receptionist",
    "PasswordHash": "DEV_HASH_HERE"
  }
}
```

### Production (appsettings.Production.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  },
  "ReceptionistAccount": {
    "Username": "receptionist",
    "PasswordHash": "PROD_HASH_FROM_KEYVAULT"
  }
}
```

---

## 📝 Logging Configuration

### Enable Detailed Service Logging:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "ClinicManagementSystem.Services.VisitService": "Debug",
      "ClinicManagementSystem.Services.AuthService": "Debug",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Enable File Logging (Add NLog):
```bash
dotnet add package NLog
dotnet add package NLog.Web.AspNetCore
```

**nlog.config:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" 
      xsi:schemaLocation="http://www.nlog-project.org/schemas/NLog.xsd NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <targets>
    <target name="logfile" xsi:type="File" 
            fileName="logs/${shortdate}.log"
            layout="${longdate} ${level:uppercase=true} ${message}" />
    <target name="console" xsi:type="ColoredConsole" 
            layout="${longdate} ${level:uppercase=true} ${message}" />
  </targets>

  <rules>
    <logger name="*" minlevel="Info" writeTo="logfile" />
    <logger name="ClinicManagementSystem.Services" minlevel="Debug" writeTo="console" />
  </rules>
</nlog>
```

---

## 🗄️ Database Configuration

### Connection String Examples

**Local SQL Server:**
```
Server=localhost;Database=ClinicManagementDb;Trusted_Connection=true;Encrypt=false
```

**Local SQL Express:**
```
Server=(localdb)\mssqllocaldb;Database=ClinicManagementDb;Trusted_Connection=true;Encrypt=false
```

**Remote SQL Server:**
```
Server=your-server.database.windows.net,1433;Initial Catalog=ClinicDb;Persist Security Info=False;User ID=sqladmin;Password=YourPassword;Encrypt=True;Connection Timeout=30;
```

### Database Migrations After Bug Fixes

No database migrations are required. The bug fixes only affect:
- Application logic (password hashing)
- Query behavior (null checks)
- Delete cascade rules (no migration needed)
- Logging (no database changes)

However, to be safe, you may want to add cascade delete rules in a migration:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Drop existing foreign key
    migrationBuilder.DropForeignKey(
        name: "FK_VisitProcedures_Visits_VisitId",
        table: "VisitProcedures");

    // Add back with cascade delete
    migrationBuilder.AddForeignKey(
        name: "FK_VisitProcedures_Visits_VisitId",
        table: "VisitProcedures",
        column: "VisitId",
        principalTable: "Visits",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);
}
```

---

## 🔑 Authorization Policy Configuration

The fixes add an "AdminOnly" policy. Here's how to use it:

### Current Setup:
```csharp
// Program.cs - Already configured
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});
```

### Usage on Controllers:
```csharp
[Authorize(Policy = "AdminOnly")]
public class SettingsController : Controller
{
    // Only users with Admin role can access
}
```

### Adding Admin Role to Users:

You need to implement admin role assignment. Examples:

**Hardcoded admin user:**
```csharp
private bool IsAdmin(string username)
{
    return username == "admin" || 
           username == "superadmin@clinic.com";
}

// Then in AuthService.CreatePrincipal:
public ClaimsPrincipal CreatePrincipal(string username)
{
    var role = IsAdmin(username) ? "Admin" : "Receptionist";
    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, username),
        new(ClaimTypes.Role, role),
        new("DisplayName", role)
    };
    // ...
}
```

**Database-backed admin users:**
```csharp
public async Task<bool> IsAdminAsync(string username)
{
    var admin = await _context.Admins
        .FirstOrDefaultAsync(a => a.Username == username);
    return admin != null;
}
```

---

## 🚀 Deployment Checklist

Before deploying to production:

- [ ] Update `appsettings.Production.json` with production password hash
- [ ] Remove `appsettings.Development.json` from production deployment
- [ ] Enable HTTPS
- [ ] Set connection string to production database
- [ ] Configure logging output (file or cloud)
- [ ] Set up Azure Key Vault or equivalent
- [ ] Enable application insights
- [ ] Review authorization policies
- [ ] Test all delete operations
- [ ] Test password validation
- [ ] Backup production database before first deployment

---

## 📊 Configuration Validation

### Check if Configuration is Correct:

```csharp
// Add to Program.cs after building app
using var scope = app.Services.CreateScope();
var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

var username = config["ReceptionistAccount:Username"];
var hash = config["ReceptionistAccount:PasswordHash"];

if (string.IsNullOrEmpty(hash))
    Console.WriteLine("⚠️  WARNING: PasswordHash is not configured!");
else if (hash.StartsWith("MyPassword") || hash.Contains("plaintext"))
    Console.WriteLine("⚠️  ERROR: Plain-text password detected! Use PasswordHasher");
else
    Console.WriteLine("✅ Password hash configured correctly");
```

---

## 🆘 Troubleshooting Configuration Issues

### Issue: "Invalid hash format" error
**Solution:**
- Use `PasswordHasher<string>` to generate valid hash
- Don't manually create or modify hashes
- Copy entire hash string including special characters

### Issue: Settings page still accessible
**Solution:**
- Verify `[Authorize(Policy = "AdminOnly")]` is on SettingsController
- Restart application
- Clear browser cache
- Check user role is correctly set

### Issue: Database connection fails
**Solution:**
- Verify connection string in appsettings.json
- Test connection in SQL Server Management Studio
- Ensure user has database access
- Check firewall rules for remote connections

### Issue: Logging not appearing
**Solution:**
- Verify logging is configured in `Logging` section
- Restart application
- Check Visual Studio Output window
- Verify log file path has write permissions

---

## 📚 Additional Resources

- [Microsoft: Password Hashing](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/)
- [Microsoft: Authorization Policies](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- [Microsoft: Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Entity Framework Core: Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships/)

---

## ✅ Verification Checklist

After configuration:

- [ ] Password hash is configured (not plain-text)
- [ ] Authorization policy "AdminOnly" is defined
- [ ] Connection string works
- [ ] Logging is enabled
- [ ] appsettings.Development.json is in .gitignore
- [ ] No passwords appear in source control
- [ ] Database cascade delete is configured
- [ ] All services have proper dependency injection
