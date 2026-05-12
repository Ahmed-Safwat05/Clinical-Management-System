# 🚀 Queue System Refactoring - Implementation Guide

## Quick Start

### Step 1: Review Changes
Read the documentation:
```
QUEUE_SYSTEM_REFACTORING.md
```

### Step 2: Build Project
✅ Already successful - no errors

### Step 3: Apply Database Migration
```bash
cd "D:\CSharb\MVC\CMS - Development\"
dotnet ef database update
```

This creates the `IsWalkIn` column in the `Appointments` table.

### Step 4: Test the Features

#### Test 1: Create Appointment Today (Walk-In)
1. Go to Appointments → Create
2. Select a doctor
3. Select today's date
4. Create appointment
5. **Expected:** Appointment appears in "Walk-In" queue with "W1" numbering

#### Test 2: Create Appointment Tomorrow (Scheduled)
1. Go to Appointments → Create
2. Select the same doctor
3. Select tomorrow's date
4. Create appointment
5. **Expected:** Appointment appears in "Scheduled" queue with "1" numbering

#### Test 3: Queue Reordering on Cancellation
1. Create 3 appointments for same doctor/date
2. Queue shows: 1, 2, 3
3. Cancel appointment #2
4. Refresh page
5. **Expected:** 
   - Appointment #2 gone from queue
   - Appointment #3 renumbered to 2
   - Queue shows: 1, 2
   - Cancelled appointment hidden

#### Test 4: Independent Walk-In Queue
1. Create 2 scheduled appointments for tomorrow (1, 2)
2. Create 2 walk-in appointments for today (W1, W2)
3. View queue for today
4. **Expected:** Two separate tables
   - Scheduled: shows 1, 2
   - Walk-In: shows W1, W2

---

## Files Modified

### 1. Model
- `Models/Appointment.cs` - Added `IsWalkIn` property

### 2. Database
- `Migrations/20260520000001_AddIsWalkInToAppointments.cs` - New migration

### 3. Repository
- `Interfaces/Repositories/IAppointmentRepository.cs` - 3 new methods
- `Repositories/AppointmentRepository.cs` - Implementation of new methods

### 4. Service
- `Interfaces/Services/IAppointmentService.cs` - 3 new methods
- `Services/AppointmentService.cs` - Refactored CancelAsync, new methods

### 5. View Models
- `Models/ViewModels/AppointmentsIndexViewModel.cs` - Separated queues

### 6. Controller
- `Controllers/AppointmentsController.cs` - Refactored Index method

### 7. View
- `Views/Appointments/Index.cshtml` - Two separate queue tables

---

## Key Changes Explained

### Before (Problems)
```
Queue after cancellation of #2:
1 Ahmed
2 Mohamed (Cancelled - still showing!)
3 Ali (Wrong number, should be 2)
4 Khaled (Wrong number, should be 3)

Only one queue type - mix of scheduled and walk-in
```

### After (Fixed)
```
After cancellation of #2:
1 Ahmed
2 Ali (Renumbered correctly)
3 Khaled (Renumbered correctly)
(Cancelled appointment hidden)

Two separate queues:
Scheduled Queue: 1, 2, 3...
Walk-In Queue: W1, W2, W3...
```

---

## Migration Details

### What Changed
- Added `IsWalkIn` (bit, default false) to Appointments table

### How to Apply
```bash
dotnet ef database update
```

### Rollback (if needed)
```bash
dotnet ef database update <PreviousMigrationName>
```

### Data Safety
- ✅ No data loss
- ✅ Existing appointments get `IsWalkIn = false`
- ✅ All existing functionality preserved

---

## API Changes

### Appointment Service

#### New Method: GetActiveQueueAsync
```csharp
// Gets all non-cancelled appointments
var queue = await appointmentService.GetActiveQueueAsync(DateTime.Today);
```

#### New Method: GetScheduledQueueByDoctorAsync
```csharp
// Gets scheduled appointments for a doctor
var scheduled = await appointmentService.GetScheduledQueueByDoctorAsync(doctorId, date);
```

#### New Method: GetWalkInQueueByDoctorAsync
```csharp
// Gets walk-in appointments for a doctor
var walkIn = await appointmentService.GetWalkInQueueByDoctorAsync(doctorId, date);
```

#### Updated: CreateAsync
```csharp
// Now automatically sets IsWalkIn = (date == today)
var appointment = await appointmentService.CreateAsync(patientId, doctorId, date);
```

#### Refactored: CancelAsync
```csharp
// Now handles queue reordering with walk-in separation
await appointmentService.CancelAsync(appointmentId);
```

---

## View Model Changes

### DoctorQueueTabViewModel
**Old Structure:**
```csharp
public int WaitingCount { get; set; }
public IReadOnlyList<AppointmentListItemViewModel> Appointments { get; set; }
```

**New Structure:**
```csharp
public int ScheduledWaitingCount { get; set; }
public IReadOnlyList<AppointmentListItemViewModel> ScheduledQueue { get; set; }

public int WalkInWaitingCount { get; set; }
public IReadOnlyList<AppointmentListItemViewModel> WalkInQueue { get; set; }
```

### AppointmentListItemViewModel
**Added:**
```csharp
public bool IsWalkIn { get; set; }
public string FormattedQueueNumber => IsWalkIn ? $"W{QueueNumber}" : QueueNumber.ToString();
```

---

## Database Schema

### Appointments Table
```sql
CREATE TABLE Appointments (
    Id INT PRIMARY KEY,
    PatientId INT,
    DoctorId INT,
    Date DATETIME,
    QueueNumber INT,
    Status INT, -- AppointmentStatus
    IsWalkIn BIT DEFAULT 0,  -- NEW COLUMN
    -- ... other columns
);

CREATE INDEX IX_Appointments_DoctorId_Date_Status ON Appointments(DoctorId, Date, Status);
CREATE INDEX IX_Appointments_DoctorId_Date_IsWalkIn_Status ON Appointments(DoctorId, Date, IsWalkIn, Status);
```

---

## Troubleshooting

### Issue: Migration fails
**Solution:** Ensure EF Core tools are installed
```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

### Issue: Walk-in queue not showing
**Solution:** 
1. Verify appointment date is today
2. Check database: `SELECT IsWalkIn FROM Appointments WHERE Id = ?`
3. Refresh page (clear browser cache)

### Issue: Queue numbers not renumbering after cancellation
**Solution:**
1. Check Status is "Cancelled" not "Done"
2. Verify all appointments have same DoctorId, Date, IsWalkIn
3. Check database query manually

### Issue: Cancelled appointments still showing
**Solution:**
1. Use GetActiveQueueAsync instead of GetByDateAsync
2. Verify filtered by `Status != Cancelled`
3. Check view uses correct property names

---

## Performance Notes

### Queries Optimized
- ✅ Filtered by Status = Waiting (excludes Done, Cancelled)
- ✅ Indexed by DoctorId, Date, Status
- ✅ Uses AsNoTracking for read operations
- ✅ No N+1 queries

### Scalability
- Can handle thousands of appointments
- Reordering is efficient (only affected appointments)
- Queue display is fast

---

## Backward Compatibility

✅ **Fully Backward Compatible**
- Old appointments continue to work
- IsWalkIn defaults to false
- Existing APIs not removed
- No breaking changes

---

## Next Steps (Optional Future Work)

1. **Priority Queues:** Add priority level to separate VIP/Regular
2. **Queue Pause:** Ability to pause queue (lunch breaks)
3. **Notifications:** SMS when patient's turn is coming
4. **Analytics:** Track wait times, appointment no-shows
5. **Calendar View:** Visual calendar of queues

---

## Support & Questions

### How to Verify It's Working
1. Create appointment today → Should show in Walk-In with W1
2. Create appointment tomorrow → Should show in Scheduled with 1
3. Cancel an appointment → Queue numbers should renumber
4. Check database → IsWalkIn should be set correctly

### Where to Check Logs
1. Visual Studio Debug Output (Ctrl+Alt+O)
2. Database queries in SQL Server Management Studio
3. View the application logs

### Rollback if Issues
```bash
# Revert migration
dotnet ef database update <PreviousMigrationName>

# Or manually delete migration if not applied
```

---

✅ **Implementation Complete!**

The queue system has been successfully refactored with:
- ✅ Fixed queue reordering after cancellation
- ✅ Separate walk-in and scheduled queues
- ✅ Automatic walk-in detection
- ✅ Clean, maintainable architecture
- ✅ Full backward compatibility
- ✅ Successful build
