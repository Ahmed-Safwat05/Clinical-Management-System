# Bug Fix Verification & Testing Guide

This document provides step-by-step instructions to verify all bug fixes are working correctly.

---

## 🔴 BUG #1: Password Hashing Verification

### Prerequisites:
- Stop your application
- Generate a password hash using the `PasswordHashingUtility.cs`

### Test Steps:

1. **Generate Hash:**
   - Open Package Manager Console
   - Run this in your app:
     ```csharp
     var hash = PasswordHashingUtility.GenerateHash("receptionist", "TestPassword123!");
     Console.WriteLine(hash);
     ```
   - Copy the output

2. **Update appsettings.json:**
   ```json
   "ReceptionistAccount": {
     "Username": "receptionist",
     "PasswordHash": "PASTE_THE_HASH_HERE"
   }
   ```

3. **Start Application**

4. **Test Login:**
   - ✅ Should FAIL with plain-text password: `TestPassword123!`
   - ✅ Should FAIL with incorrect password: `WrongPassword`
   - ✅ Should FAIL with old config key (no plain-text password)

5. **Verification:**
   - Open browser console (F12)
   - Check Network tab for authentication attempts
   - Verify only hashed passwords appear in logs (never plain-text)

---

## 🟠 BUG #2: Role-Based Authorization Verification

### Test Steps:

1. **Try Accessing Settings Page:**
   - Login as receptionist
   - Navigate to `/Settings/Index`
   - ❌ Should be BLOCKED or redirected to unauthorized page

2. **Verify in Code:**
   - Check `SettingsController.cs` has `[Authorize(Policy = "AdminOnly")]`
   - Verify `Program.cs` has authorization policy

3. **Check User Claims:**
   - Add breakpoint in `SettingsController` constructor
   - Inspect `User.IsInRole("Admin")` - should be false
   - Inspect `User.IsInRole("Receptionist")` - should be true

4. **Note for Future:**
   - System currently logs all users as "Receptionist"
   - To enable admin features, create admin role assignment logic
   - Example: Check database or configuration for admin users

---

## 🔴 BUG #3: Procedure Insertion Verification

### Prerequisites:
- Have at least one doctor, patient, and procedure in database
- Have an appointment in "Waiting" status

### Test Steps:

1. **Create a Visit from Appointment:**
   - Navigate to Appointments → Index
   - Find a waiting appointment
   - Click "Create Visit"
   - Select 1+ procedures with quantities > 0
   - Submit the form

2. **Verify in Database:**
   - Open SQL Server Management Studio
   - Query: `SELECT * FROM VisitProcedures WHERE VisitId = [YOUR_VISIT_ID]`
   - ✅ Should have rows (procedures are now saved)
   - ❌ Should NOT be empty (old behavior)

3. **Check Visit Details:**
   - Navigate back to Visits → Index
   - Click on the newly created visit
   - ✅ Should display procedures with quantities and prices
   - ✅ Should show subtotal calculations

4. **Verify Database Changes:**
   ```sql
   SELECT vp.*, p.Name, p.Price 
   FROM VisitProcedures vp
   JOIN Procedures p ON vp.ProcedureId = p.Id
   WHERE vp.VisitId = [YOUR_VISIT_ID]
   ```
   - Should return procedure records

---

## 🟠 BUG #4: Exception Logging Verification

### Test Steps:

1. **Trigger a Validation Error:**
   - Navigate to Visits → Create
   - Try to submit with invalid data (patient or doctor as 0)
   - ❌ Should not save
   - ✅ Should show error message

2. **Check Console Output:**
   - Switch to Visual Studio Debug Output console
   - Should see: `warn: ClinicManagementSystem.Services.VisitService[0] Validation error while creating visit for patient...`
   - ✅ Error is logged with patient ID

3. **Test Database Exception:**
   - Create a visit with valid data
   - During creation, disconnect from database
   - ✅ Should see in Output: `fail: ClinicManagementSystem.Services.VisitService[0] Unexpected error while creating visit...`

4. **Enable Detailed Logging:**
   - In `appsettings.json`:
     ```json
     "Logging": {
       "LogLevel": {
         "ClinicManagementSystem.Services": "Debug"
       }
     }
     ```
   - Restart app and run tests again
   - More detailed logs should appear

---

## 🟠 BUG #5: Query Filter Null Safety Verification

### Test Steps:

1. **Create Edge Case Data:**
   - Try to create scenarios where relationships might be null
   - (Note: Model configuration prevents true nulls, but filters are safer now)

2. **Stress Test Complex Queries:**
   - Load Appointments list
   - Load Visits list
   - Load VisitProcedures through nested relationships
   - ✅ Should not crash with NullReferenceException

3. **Check EF Query Generation:**
   - Enable SQL Logging in `Program.cs`:
     ```csharp
     builder.Services.AddLogging(logging =>
     {
         logging.AddConsole();
         logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
     });
     ```
   - Watch SQL output for proper NULL checks in WHERE clauses

---

## 🔴 BUG #6: Delete Action Validation Verification

### Test Steps:

1. **Delete a Patient:**
   - Navigate to Patients → Index
   - Click Delete on any patient
   - ✅ Should see success message: "تم حذف المريض بنجاح"
   - ✅ Patient should disappear from list

2. **Verify TempData Display:**
   - Check that message appears after redirect
   - Look in View for: `@TempData["SuccessMessage"]` and `@TempData["ErrorMessage"]`

3. **Test Delete Doctor:**
   - Navigate to Doctors → Index
   - Click Delete on any doctor
   - ✅ Should see success message in Arabic

4. **Test Cancel Appointment:**
   - Navigate to Appointments → Index
   - Click Cancel on a waiting appointment
   - ✅ Should see success message

5. **Test Delete Visit:**
   - Navigate to Visits → Index
   - Click Delete on any visit
   - ✅ Should see success message

6. **Force an Error (Optional):**
   - Add a check in PatientService.DeleteAsync to throw an exception
   - Test delete again
   - ✅ Should show error message: "حدث خطأ أثناء حذف المريض: ..."

---

## 🟠 BUG #7: Cascade Delete Verification

### Prerequisites:
- SQL Server Management Studio open
- Sample visit with associated procedures

### Test Steps:

1. **Create a Visit with Procedures:**
   - Create new visit with 2-3 procedures selected
   - Note the VisitId (check in SQL)

2. **Verify VisitProcedures Created:**
   ```sql
   SELECT * FROM VisitProcedures WHERE VisitId = [YOUR_VISIT_ID]
   ```
   - ✅ Should see rows for each procedure

3. **Delete the Visit:**
   - In UI: Navigate to Visits and delete the visit
   - OR in SQL: `DELETE FROM Visits WHERE Id = [YOUR_VISIT_ID]`

4. **Verify Cascade Delete:**
   ```sql
   SELECT * FROM VisitProcedures WHERE VisitId = [YOUR_VISIT_ID]
   ```
   - ✅ Should return 0 rows (automatically deleted)
   - ❌ Should NOT have orphaned records

5. **Test Procedure Protection:**
   - Try to delete a procedure that's used in active visits
   - ✅ Should fail gracefully (cascade delete set to RESTRICT for procedures)
   - Should preserve data integrity

---

## 📊 Comprehensive Testing Checklist

### Authentication & Authorization:
- [ ] Login with correct hashed password succeeds
- [ ] Login with plain-text password fails
- [ ] Settings page is blocked for regular users
- [ ] Role claims are properly set

### Data Operations:
- [ ] Procedures are saved with visits (not lost)
- [ ] Visit procedures have correct prices and quantities
- [ ] Procedures are deleted with visit (cascade)
- [ ] Delete confirmations appear

### Error Handling:
- [ ] Validation errors are logged
- [ ] Database errors are caught and logged
- [ ] Users see friendly error messages
- [ ] App doesn't crash on errors

### Query Safety:
- [ ] No NullReferenceExceptions when loading lists
- [ ] Deleted items are filtered from results
- [ ] Related data loads correctly
- [ ] Complex relationships work

### Database Integrity:
- [ ] No orphaned records after deletes
- [ ] Foreign key constraints maintained
- [ ] Transaction rollbacks work on errors
- [ ] Query filters prevent soft-deleted items from appearing

---

## 🔍 Debugging Tips

### View Database Logging:
```csharp
// Add to Program.cs
builder.Services.AddLogging(logging =>
{
    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Debug);
});
```

### Access Browser DevTools:
- Press F12 in browser
- Check Network tab for failed requests
- Check Console for JavaScript errors

### Check Application Logs:
- Visual Studio → Debug → Windows → Output
- Filter by your application name
- Look for Warning and Error entries

### Breakpoint Debugging:
- Set breakpoint in AuthService.ValidateReceptionist()
- Step through password verification
- Inspect PasswordVerificationResult

---

## ✅ Sign-Off Checklist

When all tests pass, mark these items:

- [ ] BUG #1: Password hashing works correctly
- [ ] BUG #2: Authorization policy blocks unauthorized access
- [ ] BUG #3: Procedures are inserted with visits
- [ ] BUG #4: Exceptions are logged properly
- [ ] BUG #5: Query filters handle nulls safely
- [ ] BUG #6: Delete actions show success/error messages
- [ ] BUG #7: Cascade delete removes orphaned records
- [ ] BUG #8: N+1 queries optimized (already correct)

---

## 📝 Common Issues & Solutions

### Issue: "PasswordHash not found" error
**Solution:** Update appsettings.json key from "Password" to "PasswordHash"

### Issue: Settings page still accessible
**Solution:** 
- Hard refresh browser (Ctrl+Shift+R)
- Restart Visual Studio
- Clear authentication cookie

### Issue: Procedures not showing in visit details
**Solution:**
- Verify procedures were actually selected in form
- Check database for VisitProcedures records
- Check View is using `@Model.Procedures` correctly

### Issue: Delete shows error but item is deleted
**Solution:** 
- Verify SaveChangesAsync() succeeded in service
- Add database logs to see actual SQL errors
- Check for soft-delete logic interfering

### Issue: Hot reload warnings
**Solution:**
- These are expected for interface changes
- Restart Visual Studio
- Clear .vs folder if needed

---

## 📞 Need Help?

If tests fail:
1. Check the corresponding bug fix in BUGFIX_SUMMARY.md
2. Enable detailed logging
3. Use SQL Profiler to see actual queries
4. Review the code changes in each modified file
