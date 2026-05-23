# Phase 2 - Quick User Guide

## Admin Panel - Features Overview

### 1. USER MANAGEMENT

**Access:** Click "المستخدمين" (Users) in Admin Section of Sidebar
**URL:** `/Users`

#### Create New Receptionist User
1. Click "إضافة مستخدم جديد" (Add New User) button
2. Enter:
   - **اسم المستخدم** (Username): Must be unique, 3-100 characters
   - **الاسم الكامل** (Display Name): User's full name
   - **كلمة المرور** (Password): Min 6 characters
   - **تأكيد كلمة المرور** (Confirm Password): Must match
3. Click "إضافة المستخدم" (Add User)

#### Manage Existing Users
- **Table columns:** Username, Full Name, Role, Status, Created Date, Actions
- **Actions available:**
  - 🔑 **Change Password:** Click key icon to change user's password
  - 🔓 **Toggle Status:** Activate/deactivate user account

#### User Status
- **نشط** (Active) - Green badge, user can login
- **معطل** (Inactive) - Gray badge, user cannot login

---

### 2. SETTINGS PAGE

**Access:** Click "الإعدادات" (Settings) in Admin Section of Sidebar
**URL:** `/Settings`

#### Available Settings

1. **اسم العيادة** (Clinic Name)
   - Current: Displayed in sidebar brand
   - Change and click "حفظ الإعدادات" (Save Settings)
   - Updates immediately in navigation

2. **سعر الكشف الافتراضي** (Default Exam Price)
   - Enter price amount (e.g., 100)
   - Used as default for new visits
   - Validation: Must be 0 or positive

3. **أقصى خصم** (Maximum Discount)
   - Enter discount amount (e.g., 50)
   - Limits discounts on visits
   - Validation: Must be 0 or positive

4. **السماح بالخصم** (Allow Discount)
   - Toggle checkbox to enable/disable discounts
   - When disabled, discount fields are ignored

#### Data Management Section
⚠️ **Warning:** These are irreversible operations

- **حذف كل الزيارات** (Delete All Visits)
- **حذف كل المواعيد** (Delete All Appointments)
- **تصفير بيانات التشغيل** (Reset Operational Data)

---

### 3. CHANGE PASSWORD

**Self-Change:**
1. Click profile image in top-right
2. Click "تغيير كلمة المرور" (Change Password)
3. Enter:
   - **كلمة المرور الحالية** (Current Password)
   - **كلمة المرور الجديدة** (New Password)
   - **تأكيد كلمة المرور الجديدة** (Confirm New Password)
4. Click "حفظ كلمة المرور الجديدة" (Save New Password)
5. You'll be logged out and must login with new password

**Admin-Change (for any user):**
1. Go to Users → List
2. Click 🔑 icon for the user
3. Enter new password (current password NOT required)
4. User must login with new password next time

---

## Role-Based Access

### Admin Can:
✅ View and manage users  
✅ Create receptionist accounts  
✅ Change any user's password  
✅ Access settings page  
✅ View admin menu items  
✅ Activate/deactivate users  

### Receptionist Can:
✅ View patients  
✅ View appointments  
✅ View visits  
✅ View reports  
✅ Change own password  
❌ Cannot create users  
❌ Cannot access settings  
❌ Cannot see admin menu  

---

## Default Credentials

### Admin Account
- **Username:** `admin`
- **Password:** `admin1`

### Receptionist Account
- **Username:** `reception`
- **Password:** `reception1`

⚠️ Change default passwords immediately after first login!

---

## Troubleshooting

### Issue: "Invalid username or password"
- Verify username is correct
- Check caps lock is off
- Ensure password is exactly correct (case-sensitive)

### Issue: "Page not authorized / 403 Forbidden"
- You don't have admin role
- Ask admin to change your role or create a new admin account

### Issue: "Username already exists"
- Choose a different username
- Usernames must be unique

### Issue: Cannot deactivate user
- You may be trying to deactivate the last active admin
- Must have at least one active admin account

### Issue: Changes not saved to settings
- Verify all validation messages (if any)
- Click "حفظ الإعدادات" button
- Check browser console for errors

---

## Best Practices

1. **Change default passwords first** - Don't leave default credentials
2. **Backup important data** before using "Delete All" functions
3. **Use strong passwords** - Min 6 characters, preferably longer
4. **Review active users regularly** - Deactivate unused accounts
5. **Keep admin role secure** - Don't share admin credentials
6. **Set appropriate clinic name** - Shows to all users in sidebar

---

## Contact & Support

For technical issues or questions about Phase 2 features, refer to the system administrator.

