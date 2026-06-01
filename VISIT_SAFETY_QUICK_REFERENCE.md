# Visit Safety - Quick Reference Guide

## 🎯 What Changed?

### User Perspective
- **Before**: Clicking "حذف" would permanently delete a visit
- **After**: Clicking "إلغاء الزيارة" opens a confirmation modal, voids the visit (soft delete), and records the reason

### Developer Perspective
- Added VoidConfirm actions to VisitsController
- Added status badges (نشطة/ملغاة) to Index and Details views
- Excluded voided visits from all revenue calculations
- Implemented double-void prevention

---

## 📝 Modified Files (6 Total)

```
Controllers/VisitsController.cs
├─ Added: VoidConfirm() GET action (returns partial or redirects)
└─ Added: VoidConfirm() POST action (handles void with reason)

Views/Visits/Index.cshtml
├─ Added: "الحالة" (Status) column
├─ Replaced: "حذف" button with "إلغاء الزيارة" button
├─ Added: Void confirmation Bootstrap modal
└─ Added: JavaScript form submission handler (AJAX)

Views/Visits/Details.cshtml
├─ Added: Status badge (نشطة/ملغاة) in details grid
└─ Added: Alert box showing VoidedAt and VoidReason (if voided)

Services/FinancialReportService.cs
├─ Filter: GetVisitPeriodSummaryAsync() - exclude voided
├─ Filter: GetPaymentStatusAnalyticsAsync() - exclude voided
├─ Filter: GetTopDebtorsAsync() - exclude voided
├─ Filter: GetDailyCollectedRevenueAsync() - exclude voided
└─ Filter: GetMonthlyCollectedRevenueAsync() - exclude voided

Services/HomeService.cs
├─ Filter: GetHomeAsync() - TodayVisitsCount exclude voided
├─ Filter: GetHomeAsync() - TodayRevenue exclude voided
├─ Filter: GetHomeAsync() - TotalRevenue exclude voided
├─ Filter: BuildDailyRevenueAsync() - exclude voided
└─ Filter: BuildMonthlyRevenueAsync() - exclude voided

Services/DashboardService.cs
├─ Filter: GetDashboardAsync() - MonthlyVisitsCount exclude voided
├─ Filter: GetDashboardAsync() - YearlyVisitsCount exclude voided
└─ Filter: GetDashboardAsync() - TopDoctor exclude voided
```

---

## 🔍 Key Code Snippets

### 1. Void Button in Index.cshtml
```razor
<button type="button" class="btn btn-sm btn-danger void-visit-btn" 
		data-visit-id="@visit.Id" 
		data-bs-toggle="modal" 
		data-bs-target="#voidConfirmModal">
	إلغاء الزيارة
</button>
```

### 2. Status Badge Display
```razor
@if (visit.IsVoided)
{
	<span class="status-badge danger">ملغاة</span>
}
else
{
	<span class="status-badge success">نشطة</span>
}
```

### 3. Void Modal Form
```html
<form id="voidConfirmForm" method="post" asp-action="VoidConfirm">
	@Html.AntiForgeryToken()
	<input type="hidden" id="voidVisitId" name="id" />
	<textarea id="voidReason" name="voidReason" 
			  placeholder="أدخل سبب الإلغاء إن وجد..."></textarea>
</form>
```

### 4. Post Void Action
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> VoidConfirm(int id, string? voidReason)
{
	var visit = await _visitService.GetDetailsAsync(id);
	if (visit?.Status == VisitStatus.Voided) 
		return Json(new { success = false, message = "هذه الزيارة مُلغاة بالفعل" });

	await _visitService.VoidAsync(id, voidReason);
	return Json(new { success = true, message = "تم إلغاء الزيارة بنجاح" });
}
```

### 5. Revenue Filter (Example)
```csharp
// Before
var revenue = await _context.Visits
	.Where(x => x.Date >= start && x.Date < end)
	.SumAsync(x => x.TotalPrice);

// After
var revenue = await _context.Visits
	.Where(x => x.Date >= start && x.Date < end && x.Status != VisitStatus.Voided)
	.SumAsync(x => x.TotalPrice);
```

---

## ✅ Verification Checklist

Run this before/after deployment:

```powershell
# 1. Build succeeds
dotnet build

# 2. Visit Index shows status column
# Navigate to /Visits - should see "الحالة" column with badges

# 3. Create and void a visit
# Create a visit, click "إلغاء الزيارة", enter reason, confirm

# 4. Verify Details shows void info
# Navigate to voided visit - should show alert with date and reason

# 5. Verify revenue is correct
# Dashboard revenue should exclude the voided visit

# 6. Test double-void prevention
# Try voiding the same visit again - should show error message

# 7. Check audit log
# Audit log should show void action with timestamp and reason
```

---

## 🚀 How to Test

### Test Case 1: Basic Void
```
1. Create visit: 100 ج.م
2. Click "إلغاء الزيارة"
3. Enter reason: "خطأ في البيانات"
4. Click "تأكيد الإلغاء"
5. Verify: Status shows "ملغاة", Details show alert
```

### Test Case 2: Revenue Accuracy
```
1. Create 2 active visits: 50 + 75 = 125 ج.م
2. Void first visit (50 ج.م)
3. Check Dashboard: Revenue = 75 ج.م ✓
4. Check Home: Revenue = 75 ج.م ✓
```

### Test Case 3: Double-Void Prevention
```
1. Create and void a visit
2. Try to void again
3. Verify: Error message "هذه الزيارة مُلغاة بالفعل"
```

---

## 🐛 Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| Modal doesn't appear | Bootstrap JS not loaded | Check `site.js` is included |
| AJAX request fails | Anti-forgery token invalid | Verify `@Html.AntiForgeryToken()` in form |
| Reason not saved | Null/empty string | Service trims and checks for null |
| Revenue still includes voided | Filter not applied | Verify `&& x.Status != VisitStatus.Voided` in query |
| Double-void allowed | Service check missing | Verify service has `if (visit.Status == VisitStatus.Voided) return;` |

---

## 📚 Related Files (No Changes)

These files were NOT modified (already have void support):

- `Models/Visit.cs` - Has Status, VoidedAt, VoidReason properties
- `Models/VisitStatus.cs` - Has Active/Voided enum values
- `Services/VisitService.cs` - VoidAsync() already implemented
- `Data/ApplicationDbContext.cs` - DbSet<Visit> already configured
- `Repositories/VisitRepository.cs` - CRUD operations unchanged

---

## 🔐 Security Notes

- ✅ Anti-forgery tokens protect against CSRF
- ✅ Double-void check prevents accidental duplicates
- ✅ Audit logging tracks who voided what and when
- ✅ VoidReason is user input and should be treated as untrusted
- ✅ Authorization checks should be in VisitsController (implicit in base controller)

---

## 📊 Database Impact

**No migrations required** - all columns already exist:
- Visit.Status (VisitStatus enum)
- Visit.VoidedAt (DateTime?)
- Visit.VoidReason (string?)

**Data integrity preserved**:
- ✅ Existing voided visits remain in database
- ✅ All related records (payments, procedures, consumptions) unchanged
- ✅ Only Visit.Status is modified during void operation

---

## 🎯 Success Criteria

All requirements met:

- ✅ Delete button replaced with "إلغاء الزيارة"
- ✅ No physical deletes (soft voiding)
- ✅ Void reason captured and stored
- ✅ Status badges displayed (نشطة/ملغاة)
- ✅ Void details shown in Details page
- ✅ Revenue excludes voided visits
- ✅ Double-void prevention working
- ✅ Build successful (no compilation errors)
- ✅ No breaking changes

---

## 💡 Tips & Tricks

### To manually void a visit in database:
```sql
UPDATE Visits 
SET Status = 1, VoidedAt = GETUTCDATE(), VoidReason = 'Manual void'
WHERE Id = @visitId;
```

### To find all voided visits:
```csharp
var voidedVisits = await _context.Visits
	.Where(x => x.Status == VisitStatus.Voided)
	.ToListAsync();
```

### To recalculate revenue excluding voided:
```csharp
var revenue = await _context.Visits
	.Where(x => x.Status != VisitStatus.Voided)
	.SumAsync(x => x.TotalPrice);
```

---

## 📞 Support

For issues or questions:
1. Check build log for compilation errors
2. Review browser console for JavaScript errors
3. Check database for Visit record status
4. Review audit log for void action record
5. Verify anti-forgery token is present in HTML

---

**Last Updated**: 2025  
**Version**: 1.0  
**Status**: Complete & Production Ready
