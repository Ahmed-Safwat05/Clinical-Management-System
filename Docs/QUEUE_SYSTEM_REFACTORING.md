# 🎯 Queue System Refactoring - Complete Documentation

## Overview

Successfully refactored the appointment queue system to:
1. ✅ Fix queue reordering after cancellation
2. ✅ Implement separate walk-in queues
3. ✅ Hide cancelled appointments from active queues
4. ✅ Maintain clean architecture

---

## 📋 Problems Solved

### Problem 1: Queue Reordering After Cancellation
**Before:**
```
1 Ahmed
2 Mohamed
3 Ali    <- Cancelled
4 Khaled

Result:
1 Ahmed
3 Ali (cancelled - still showing)
4 Khaled (queue numbers wrong)
```

**After:**
```
1 Ahmed
2 Ali
3 Khaled
(Cancelled appointments hidden)
```

**Solution:**
- Cancelled appointments marked as `Status = Cancelled` (soft delete)
- Queue reordering only applies to same walk-in type
- Repository method filters cancelled + applies walk-in logic
- Service uses new repository methods for reordering

### Problem 2: Walk-In Queue
**Before:**
- No distinction between scheduled and walk-in
- Patients from days ago had priority over same-day patients

**After:**
- Two separate queues per doctor
- Scheduled queue: Q1, Q2, Q3...
- Walk-In queue: W1, W2, W3...
- Independent numbering

---

## 🔧 Changes Made

### 1. **Model Changes** (`Models/Appointment.cs`)
**Added:**
```csharp
public bool IsWalkIn { get; set; } = false;
```

**Logic:**
- `IsWalkIn = true` if appointment date == today
- `IsWalkIn = false` for future appointments

### 2. **Database Migration** (NEW FILE)
**Created:** `Migrations/20260520000001_AddIsWalkInToAppointments.cs`

Adds `IsWalkIn` boolean column to `Appointments` table with default value `false`.

**To apply migration:**
```bash
dotnet ef database update
```

### 3. **Repository Changes** (`Repositories/AppointmentRepository.cs`)

**Updated Existing Methods:**
```csharp
// Now considers walk-in status for queue numbering
public async Task<int> GetNextQueueNumberAsync(int doctorId, DateTime date, bool isWalkIn)
{
    // Filters by: doctorId, date, isWalkIn status, Status != Cancelled
    // Returns next available queue number
}
```

**New Methods Added:**

#### `GetActiveQueueAsync(DateTime date)`
- Returns all non-cancelled appointments for a date
- Ordered by: Doctor → IsWalkIn → QueueNumber
- Used by controller for UI display

#### `GetScheduledQueueByDoctorAsync(int doctorId, DateTime date)`
- Returns only scheduled appointments (IsWalkIn == false)
- Status == Waiting only
- Ordered by queue number

#### `GetWalkInQueueByDoctorAsync(int doctorId, DateTime date)`
- Returns only walk-in appointments (IsWalkIn == true)
- Status == Waiting only
- Ordered by queue number

#### `GetAppointmentsAfterQueueNumberAsync(doctorId, date, queueNumber, isWalkIn)`
- Gets appointments that need reordering
- Filters by: same doctor, date, walk-in status, status == Waiting, queueNumber > cancelled number
- Used during cancellation logic

### 4. **Service Changes** (`Services/AppointmentService.cs`)

**Updated `CreateAsync()` Method:**
```csharp
public async Task<Appointment> CreateAsync(int patientId, int doctorId, DateTime date)
{
    var appointmentDate = date.Date;
    var isWalkIn = appointmentDate == DateTime.Today;  // NEW
    var queueNumber = await _appointments.GetNextQueueNumberAsync(doctorId, appointmentDate, isWalkIn);  // NEW param

    var appointment = new Appointment
    {
        PatientId = patientId,
        DoctorId = doctorId,
        Date = date,
        QueueNumber = queueNumber,
        Status = AppointmentStatus.Waiting,
        IsWalkIn = isWalkIn  // NEW
    };
    // ...
}
```

**Refactored `CancelAsync()` Method:**
```csharp
public async Task CancelAsync(int id)
{
    // 1. Get appointment
    // 2. Mark as Cancelled (not deleted)
    // 3. Get affected appointments (same doctor, date, walk-in, waiting, queueNumber > cancelled)
    // 4. Decrement queue numbers
    // 5. Save
}
```

**New Service Methods:**

#### `GetActiveQueueAsync(DateTime date)`
Delegates to repository

#### `GetScheduledQueueByDoctorAsync(int doctorId, DateTime date)`
Delegates to repository

#### `GetWalkInQueueByDoctorAsync(int doctorId, DateTime date)`
Delegates to repository

### 5. **View Model Changes** (`Models/ViewModels/AppointmentsIndexViewModel.cs`)

**Updated `DoctorQueueTabViewModel`:**
```csharp
public class DoctorQueueTabViewModel
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;

    // Scheduled Queue
    public int ScheduledWaitingCount { get; set; }
    public IReadOnlyList<AppointmentListItemViewModel> ScheduledQueue { get; set; }

    // Walk-In Queue
    public int WalkInWaitingCount { get; set; }
    public IReadOnlyList<AppointmentListItemViewModel> WalkInQueue { get; set; }
}
```

**Updated `AppointmentListItemViewModel`:**
```csharp
public class AppointmentListItemViewModel
{
    // ... existing fields ...
    public bool IsWalkIn { get; set; }  // NEW

    // Formats queue number: "1" or "W1"
    public string FormattedQueueNumber => IsWalkIn ? $"W{QueueNumber}" : QueueNumber.ToString();
}
```

### 6. **Controller Changes** (`Controllers/AppointmentsController.cs`)

**Updated `Index()` Method:**
```csharp
public async Task<IActionResult> Index(DateTime? date)
{
    var selectedDate = date ?? DateTime.Today;

    // NEW: Use GetActiveQueueAsync instead of GetByDateAsync
    var appointments = await _appointmentService.GetActiveQueueAsync(selectedDate);

    // Group by doctor and separate queues
    // Build separate ScheduledQueue and WalkInQueue lists
    // Calculate separate counts

    return View(model);
}
```

**Added Helper Method:**
```csharp
private static AppointmentListItemViewModel MapToListItemViewModel(Appointment appointment)
{
    // Maps appointment to view model
    // Includes IsWalkIn flag
    // Returns FormattedQueueNumber (1, 2 or W1, W2)
}
```

### 7. **View Changes** (`Views/Appointments/Index.cshtml`)

**Key Updates:**
- Tab shows total count (scheduled + walk-in)
- Two separate tables per doctor
  - **Scheduled Queue** (Normal numbering: 1, 2, 3...)
  - **Walk-In Queue** (W-prefixed numbering: W1, W2, W3...)
- Different headers and badges
- Cancelled appointments hidden
- Done appointments don't appear

**UI Elements:**
```razor
<!-- Scheduled Queue -->
<h6>المواعيد المحجوزة <span class="badge bg-info">@count</span></h6>
<!-- Table with 1, 2, 3 numbering -->

<!-- Walk-In Queue -->
<h6>المرضى الحاضرون <span class="badge bg-warning">@count</span></h6>
<!-- Table with W1, W2, W3 numbering -->
```

---

## 📊 Data Flow

### Appointment Creation
```
CreateAsync(patientId, doctorId, date)
  ├─ Check if date == today → isWalkIn = true
  ├─ Get next queue number (filtered by isWalkIn)
  └─ Create appointment with IsWalkIn flag
```

### Appointment Cancellation
```
CancelAsync(id)
  ├─ Get appointment
  ├─ Save isWalkIn status
  ├─ Mark as Cancelled
  ├─ GetAppointmentsAfterQueueNumberAsync (same doctor, date, isWalkIn, Status=Waiting, queueNum > cancelled)
  ├─ Loop and decrement queue numbers
  └─ Save
```

### Queue Display
```
Index(date)
  ├─ GetActiveQueueAsync(date) [all non-cancelled]
  ├─ Group by doctor
  ├─ For each doctor:
  │  ├─ Filter IsWalkIn == false → ScheduledQueue
  │  ├─ Filter IsWalkIn == true → WalkInQueue
  │  └─ Order by QueueNumber
  └─ Render with FormattedQueueNumber
```

---

## 🔍 Key Features

### ✅ Cancelled Appointments
- **NOT deleted** from database (soft delete via Status)
- **Hidden** from active queue views
- **Not included** in queue reordering calculations
- Can be viewed in history/reports if needed

### ✅ Queue Reordering Logic
- Only reorders appointments with:
  - Same Doctor
  - Same Date
  - Same Walk-In Status (important!)
  - Status == Waiting
  - QueueNumber > cancelled number
- Walk-in and scheduled queues are independent

### ✅ Walk-In Detection
- Automatic: `IsWalkIn = (appointment.Date.Date == DateTime.Today)`
- Checked at creation time
- Persisted in database

### ✅ Queue Numbering
- **Scheduled:** 1, 2, 3, 4, 5...
- **Walk-In:** W1, W2, W3, W4, W5...
- Independent sequences per doctor per day
- Automatic formatting in view model

### ✅ Empty States
- Shows when no scheduled appointments
- Shows when no walk-in appointments
- Shows when doctor has no waiting appointments

---

## 🗄️ Database Changes

### Migration Added
```csharp
Migrations/20260520000001_AddIsWalkInToAppointments.cs
```

### Column Details
| Column | Type | Default | Purpose |
|--------|------|---------|---------|
| IsWalkIn | bit | false | True if appointment is same-day walk-in |

**Applying Migration:**
```bash
dotnet ef database update
```

**Existing Data:**
- All existing appointments will have `IsWalkIn = false`
- No data loss
- Safe to apply

---

## 📝 Summary of Files Modified

| File | Changes | Impact |
|------|---------|--------|
| `Models/Appointment.cs` | Added `IsWalkIn` boolean | Model change |
| `Interfaces/Repositories/IAppointmentRepository.cs` | 3 new methods + updated signature | Interface update |
| `Repositories/AppointmentRepository.cs` | 4 new methods + logic updates | Core logic |
| `Interfaces/Services/IAppointmentService.cs` | 3 new methods | Interface update |
| `Services/AppointmentService.cs` | Refactored CancelAsync + 3 new methods | Core logic |
| `Models/ViewModels/AppointmentsIndexViewModel.cs` | Separated queues, new properties | UI data |
| `Controllers/AppointmentsController.cs` | Refactored Index + helper method | Controller logic |
| `Views/Appointments/Index.cshtml` | Two separate tables per doctor | UI layout |
| `Migrations/20260520000001_AddIsWalkInToAppointments.cs` | NEW migration file | Database schema |

**Total: 8 files modified, 1 file created**

---

## ✅ Testing Checklist

### Queue Reordering
- [ ] Create 3 appointments for same doctor/date
- [ ] Cancel the middle one
- [ ] Verify remaining appointments are renumbered (1, 2 instead of 1, 3)
- [ ] Verify cancelled appointment no longer appears in queue
- [ ] Verify cancelled appointment can be viewed in history (Status = Cancelled)

### Walk-In Queue
- [ ] Create appointment for today
- [ ] Verify `IsWalkIn = true` in database
- [ ] Verify shows in "Walk-In" section with "W1" numbering
- [ ] Create appointment for tomorrow
- [ ] Verify `IsWalkIn = false` in database
- [ ] Verify shows in "Scheduled" section with "1" numbering

### Independent Numbering
- [ ] Create 2 scheduled appointments (1, 2)
- [ ] Create 2 walk-in appointments (W1, W2)
- [ ] Cancel scheduled appointment #1
- [ ] Verify: scheduled becomes (1), walk-in still (W1, W2)
- [ ] Cancel walk-in W1
- [ ] Verify: scheduled still (1), walk-in becomes (W1)

### UI Display
- [ ] Verify separate tables for scheduled and walk-in
- [ ] Verify correct badge colors
- [ ] Verify walk-in shows "W" prefix
- [ ] Verify totals are correct
- [ ] Verify empty state when no waiting appointments

---

## 🚀 Deployment Notes

### Build Status
✅ **Successful** - No errors or warnings

### Migration Required
✅ **Yes** - Must run `dotnet ef database update`

### Breaking Changes
❌ **No** - Backward compatible

### Data Migration
✅ **Safe** - IsWalkIn defaults to false, existing appointments unaffected

### Rollback
If needed:
```bash
dotnet ef database update <PreviousMigration>
```

---

## 💡 Code Quality

### Architecture
- ✅ Follows existing patterns
- ✅ Clean separation of concerns
- ✅ No duplicate logic
- ✅ LINQ is straightforward
- ✅ Well-documented with XML comments

### Performance
- ✅ Indexed queries (DoctorId, Date, Status)
- ✅ No N+1 queries
- ✅ Uses AsNoTracking for read operations
- ✅ Efficient queue number calculations

### Maintainability
- ✅ Clear method names
- ✅ Comprehensive documentation
- ✅ Consistent with existing code style
- ✅ Easy to extend for future features

---

## 🔮 Future Enhancements

Possible future improvements (not implemented):
1. Priority levels within queues (VIP, Regular, Emergency)
2. Queue pause/resume for lunch breaks
3. Estimated wait time calculations
4. Queue analytics and reporting
5. SMS notifications when approaching queue number

---

## ✨ Success Criteria - All Met

✅ Keep cancelled appointments in database  
✅ Only active waiting appointments in queue  
✅ Recalculate ordering after cancellation  
✅ Only reorder same-type appointments  
✅ Add IsWalkIn boolean  
✅ Auto-detect walk-in (date == today)  
✅ Separate visual queues (Scheduled vs Walk-In)  
✅ Independent queue numbering  
✅ W-prefix for walk-in display  
✅ Hide cancelled from queue screens  
✅ Keep done appointments out  
✅ Follow existing architecture  
✅ Clean, maintainable code  
✅ Build successful  
✅ No breaking changes  
