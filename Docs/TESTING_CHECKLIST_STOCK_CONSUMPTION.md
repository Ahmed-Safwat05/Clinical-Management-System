# Manual Testing Checklist - Stock Management & Visit Consumption

## Pre-Testing Setup

- [ ] Ensure database is updated with latest migration
- [ ] Build solution successfully
- [ ] Run application without errors
- [ ] Create test product with low stock (qty < minimum)
- [ ] Create test product with sufficient stock
- [ ] Login as Admin user

---

## Test 1: Product Details Page Access

**Objective:** Verify product details page loads and displays information

**Steps:**
1. [ ] Navigate to المخزون (Inventory) in sidebar
2. [ ] Click "عرض" button on a product
3. [ ] Page loads without errors
4. [ ] Verify product name displayed
5. [ ] Verify product details visible (price, unit, status)
6. [ ] Verify current quantity shown
7. [ ] Verify minimum quantity shown
8. [ ] Verify created date displayed

**Expected Result:** Product details page displays all information correctly

---

## Test 2: Low Stock Warning Badge

**Objective:** Verify low stock warning displays

**Steps:**
1. [ ] On product details, check product with quantity <= minimum
2. [ ] Low stock warning badge should display
3. [ ] Badge shows "منخفضة" (Low)
4. [ ] Quantity text appears in orange/warning color
5. [ ] On products with sufficient stock, no warning shows
6. [ ] Return to products index, see low-stock badge there too

**Expected Result:** Low stock warnings display correctly

---

## Test 3: Add Stock Functionality

**Objective:** Test adding stock to a product

**Steps:**
1. [ ] On product details page, locate "إضافة كمية" button
2. [ ] Enter quantity: 10
3. [ ] Enter reason: "استلام من المورد" (Received from supplier)
4. [ ] Click "إضافة كمية" (Add Quantity)
5. [ ] Success message appears: "تم إضافة الكمية بنجاح"
6. [ ] Product quantity increases by 10
7. [ ] Transaction appears in history table

**Expected Result:** Stock added successfully, transaction recorded

---

## Test 4: Remove Stock Functionality

**Objective:** Test removing stock from a product

**Steps:**
1. [ ] On product details page, locate "سحب كمية" button
2. [ ] Enter quantity: 5
3. [ ] Enter reason: "معايرة مخزون" (Stock adjustment)
4. [ ] Click "سحب كمية" (Remove Quantity)
5. [ ] Success message appears: "تم سحب الكمية بنجاح"
6. [ ] Product quantity decreases by 5
7. [ ] Transaction appears in history table with "إخراج" (Out) badge

**Expected Result:** Stock removed successfully, transaction recorded

---

## Test 5: Prevent Negative Stock

**Objective:** Test that negative stock cannot be created

**Steps:**
1. [ ] Note current product quantity (e.g., 5)
2. [ ] Try to remove quantity: 10 (more than available)
3. [ ] Error message appears: "الكمية المتاحة 5 أقل من المطلوبة 10"
4. [ ] Stock does NOT decrease
5. [ ] No transaction created

**Expected Result:** Negative stock prevented with error message

---

## Test 6: Transaction History Display

**Objective:** Test transaction history table

**Steps:**
1. [ ] On product details page, scroll to "سجل حركات المخزون"
2. [ ] Verify multiple transactions display
3. [ ] For each transaction, check:
   - [ ] Type badge (إدخال/إخراج)
   - [ ] Quantity displayed
   - [ ] Reason shown
   - [ ] Visit link (if applicable)
   - [ ] Date and time
4. [ ] Transactions sorted by date (newest first)
5. [ ] If no transactions, show "لا توجد حركات مخزون"

**Expected Result:** Transaction history displays correctly and is sortable

---

## Test 7: Visit Consumption - Add Product

**Objective:** Test adding product consumption to a visit

**Steps:**
1. [ ] Navigate to الزيارات (Visits)
2. [ ] Open visit details
3. [ ] Scroll to "المنتجات المستهلكة" section
4. [ ] Select product from dropdown
5. [ ] Enter quantity
6. [ ] Click "استهلاك المنتج" or similar
7. [ ] Success message: "تم استهلاك المنتج بنجاح"
8. [ ] Product appears in consumption list
9. [ ] Product stock decreases
10. [ ] Transaction created (check products page)

**Expected Result:** Product consumption recorded and stock updated

---

## Test 8: Prevent Over-Consumption

**Objective:** Test that over-consumption is prevented

**Steps:**
1. [ ] Note a product quantity in visit (e.g., 3)
2. [ ] Try to consume 5 units (more than available)
3. [ ] Error message appears: "الكمية المتاحة 3 أقل من المطلوبة 5"
4. [ ] Consumption NOT created
5. [ ] Stock does NOT change

**Expected Result:** Over-consumption prevented with clear error

---

## Test 9: User Deactivation

**Objective:** Test user deactivation functionality

**Steps:**
1. [ ] As Admin, navigate to المستخدمين (Users)
2. [ ] Click "تعطيل" (Deactivate) on a receptionist user
3. [ ] Confirmation dialog may appear
4. [ ] Success message: "تم تعطيل المستخدم '{name}' بنجاح"
5. [ ] User status shows as inactive
6. [ ] User row appears disabled/grayed out
7. [ ] Try to deactivate self - error: "لا يمكن تعطيل حسابك الخاص"
8. [ ] Try to deactivate last admin - error: "لا يمكن تعطيل آخر مسؤول فعال"

**Expected Result:** Deactivation works with proper guards

---

## Test 10: User Reactivation

**Objective:** Test user reactivation functionality

**Steps:**
1. [ ] On users page with deactivated user
2. [ ] Click "تفعيل" (Reactivate) button
3. [ ] Success message: "تم تفعيل المستخدم '{name}' بنجاح"
4. [ ] User status changes to active
5. [ ] User row no longer appears disabled
6. [ ] User can log in again (optional: test login)

**Expected Result:** Reactivation works correctly

---

## Test 11: Soft Delete - No Hard Deletion

**Objective:** Test that users are not hard deleted

**Steps:**
1. [ ] Deactivate a user
2. [ ] In database, query AppUsers table
3. [ ] User still exists with IsActive = 0
4. [ ] User does not appear in normal queries (filtered)
5. [ ] Can be reactivated (not lost)

**Expected Result:** Soft delete preserves data while hiding inactive users

---

## Test 12: Stock Transactions - Visit Link

**Objective:** Test visit linkage in transactions

**Steps:**
1. [ ] Create a visit and consume a product
2. [ ] Go to product details
3. [ ] In transaction history, find the consumption transaction
4. [ ] "الزيارة" (Visit) column shows visit ID link
5. [ ] Click link to go to visit details
6. [ ] Verify visit shows the consumption

**Expected Result:** Visit links work bidirectionally

---

## Test 13: Low Stock Sidebar Badge

**Objective:** Test low stock indicator in sidebar

**Steps:**
1. [ ] Refresh page
2. [ ] Look at المخزون (Inventory) link in sidebar
3. [ ] If low-stock products exist, badge shows count
4. [ ] Number matches count of products with qty <= minimum
5. [ ] Consume/add products and refresh
6. [ ] Badge count updates accordingly

**Expected Result:** Sidebar badge shows accurate low-stock count

---

## Test 14: Form Validations

**Objective:** Test form validation messages

**Steps:**
1. [ ] On product details, try add stock with:
   - [ ] Empty quantity - error shown
   - [ ] Zero quantity - error: "يجب أن تكون أكبر من صفر"
   - [ ] Negative quantity - error or prevented
   - [ ] Empty reason - error shown
2. [ ] Verify error messages in Arabic
3. [ ] Verify form values persist after error

**Expected Result:** Validation works with helpful error messages

---

## Test 15: Permission & Authorization

**Objective:** Test user authorization

**Steps:**
1. [ ] As Receptionist, navigate to Users page
2. [ ] Should be forbidden or redirected (if access exists)
3. [ ] Cannot access Product Details Edit controls
4. [ ] Cannot create/deactivate users
5. [ ] Can view inventory products (if allowed)
6. [ ] Cannot modify user settings

**Expected Result:** Authorization checks working correctly

---

## Test 16: Data Consistency

**Objective:** Test data integrity

**Steps:**
1. [ ] Add 10 units to product A
2. [ ] Remove 3 units from product A
3. [ ] Consume 2 units in a visit
4. [ ] Final quantity should be 10 - 3 - 2 = 5
5. [ ] Check database: quantity = 5
6. [ ] Check transaction count: 3 records

**Expected Result:** Data stays consistent

---

## Test 17: Performance - Large Transaction List

**Objective:** Test performance with many transactions

**Steps:**
1. [ ] Create 50+ transactions on a product (batch via DB if needed)
2. [ ] Navigate to product details
3. [ ] Page loads in reasonable time (<2 seconds)
4. [ ] Transaction table shows (with pagination if implemented)
5. [ ] Scrolling is smooth

**Expected Result:** Performance acceptable

---

## Test 18: Error Recovery

**Objective:** Test error handling and recovery

**Steps:**
1. [ ] Try to consume product in non-existent visit
2. [ ] Error message appears, not application crash
3. [ ] Back button or redirect to safe page
4. [ ] Try add stock with invalid product ID
5. [ ] Error shown, no crash

**Expected Result:** Graceful error handling

---

## Test 19: Consumption Cost Calculation

**Objective:** Test cost calculation for consumption

**Steps:**
1. [ ] Product A: cost = 50 LE, consume 2 units
2. [ ] Total consumption cost should be 100 LE
3. [ ] View visit details or consumption list
4. [ ] Cost calculation matches (optional: if shown in UI)

**Expected Result:** Cost calculations accurate

---

## Test 20: UI/UX - Arabic RTL Layout

**Objective:** Test Arabic RTL display

**Steps:**
1. [ ] Product Details page
   - [ ] Text right-aligned
   - [ ] Input fields correct orientation
   - [ ] Buttons positioned correctly
   - [ ] Tables properly aligned
2. [ ] Transaction table headers correct
3. [ ] Form labels positioned correctly
4. [ ] Success/error messages readable

**Expected Result:** Arabic RTL layout correct

---

## Test 21: Browser Compatibility

**Objective:** Test across browsers

- [ ] Chrome: All features work
- [ ] Firefox: All features work
- [ ] Edge: All features work
- [ ] Mobile Safari (if applicable)

**Expected Result:** Consistent behavior across browsers

---

## Test 22: Data Export Readiness

**Objective:** Verify data can be queried for reports

**Steps:**
1. [ ] Query StockTransactions by ProductId - works
2. [ ] Query VisitProductConsumptions by VisitId - works
3. [ ] Calculate total consumption cost - works
4. [ ] Filter transactions by type (In/Out) - works

**Expected Result:** Data structure supports reporting

---

## Regression Tests

### Existing Features Still Work
- [ ] Products CRUD (Create, Read, Edit, Delete)
- [ ] Visit creation and details
- [ ] Appointment management
- [ ] Patient management
- [ ] Doctor management
- [ ] Settings page (Admin)
- [ ] User login/logout
- [ ] Admin dashboard

---

## Critical Issues Found

| Issue | Severity | Status | Notes |
|-------|----------|--------|-------|
| (None so far) | - | - | - |

---

## Performance Metrics

| Operation | Target | Actual | Status |
|-----------|--------|--------|--------|
| Product Details Load | <1s | ? | ? |
| Add Stock | <1s | ? | ? |
| Remove Stock | <1s | ? | ? |
| Consume Product | <2s | ? | ? |
| Deactivate User | <1s | ? | ? |

---

## Sign-off

- [ ] All tests passed
- [ ] No critical issues
- [ ] Ready for production
- [ ] Documented for team

**Tested By:** _______________  
**Date:** _______________  
**Build Version:** _______________

---

## Notes

Add any additional observations or issues found during testing below:

```
[Add notes here]
```
