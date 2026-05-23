# Phase 2 - Architecture & Dependency Diagram

## System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        ASP.NET Core MVC                         │
└─────────────────────────────────────────────────────────────────┘
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
    ┌───▼────┐           ┌────▼────┐          ┌──────▼─────┐
    │Controllers           │ Views   │         │ Models    │
    ├────────┤           ├────────┤         ├─────────┤
    │Account │           │Login   │         │AppUser  │
    │Users   │           │Users   │         │Setting  │
    │Settings│           │Settings│         │UserRole │
    └───┬────┘           └────┬───┘         └──────┬──┘
        │                      │                    │
        │     ┌────────────────┼────────────────┐  │
        │     │                │                │  │
    ┌───▼──────▼─────────────────▼──────────────▼─┐│
    │          Services Layer                      ││
    ├──────────────────────────────────────────────┤│
    │  IAuthService ◄──────────────────────────┐   ││
    │  ISettingsService ◄──────────────────┐   │   ││
    │                                      │   │   ││
    │  ✓ Authentication                    │   │   ││
    │  ✓ Password Hashing                  │   │   ││
    │  ✓ Settings Management               │   │   ││
    │  ✓ User Validation                   │   │   ││
    └───┬──────────────────────────────────┼───┼───┘┘
        │                                  │   │
    ┌───▼──────────────────────────────────▼───▼────┐
    │       Repository Layer                        │
    ├──────────────────────────────────────────────┤
    │  IAppUserRepository ◄─────────────────────┐  │
    │  IRepository<T> (Generic)                 │  │
    │                                           │  │
    │  ✓ CRUD Operations                        │  │
    │  ✓ Async Query Execution                  │  │
    │  ✓ Change Tracking                        │  │
    └───┬──────────────────────────────────────┬──┘
        │                                      │
        │         ┌───────────────────────────┘
        │         │
    ┌───▼─────────▼────────────────┐
    │   Application DbContext       │
    ├───────────────────────────────┤
    │  DbSet<AppUser>               │
    │  DbSet<Setting>               │
    │  DbSet<Patient>               │
    │  DbSet<Doctor>                │
    │  DbSet<Appointment>           │
    │  DbSet<Visit>                 │
    │  DbSet<Procedure>             │
    │  DbSet<VisitProcedure>        │
    │                               │
    │  ✓ Entity Configuration       │
    │  ✓ Query Filters              │
    │  ✓ Relationships              │
    │  ✓ Seeding                    │
    └───┬───────────────────────────┘
        │
        │
    ┌───▼──────────────────────────┐
    │   SQL Server Database         │
    ├──────────────────────────────┤
    │  Tables:                      │
    │  ├─ AppUsers (Phase 1)        │
    │  ├─ Settings (Phase 2)        │
    │  ├─ Patients                  │
    │  ├─ Doctors                   │
    │  ├─ Appointments              │
    │  ├─ Visits                    │
    │  ├─ Procedures                │
    │  ├─ VisitProcedures           │
    │  └─ DoctorSchedules           │
    └───────────────────────────────┘
```

## Phase 2 Component Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                      Presentation Layer                       │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌────────────────┐  ┌─────────────┐  ┌──────────────────┐  │
│  │ Account/Login  │  │ Users Admin  │  │ Settings Admin   │  │
│  │   (Phase 1)    │  │  (Phase 2)   │  │   (Phase 2)      │  │
│  └────┬───────────┘  └──────┬──────┘  └────────┬─────────┘  │
│       │                     │                   │             │
│  ┌────▼──────────────────────▼────────────────▼────────┐    │
│  │              Shared Layout (_Layout.cshtml)         │    │
│  ├───────────────────────────────────────────────────┤    │
│  │  • Dynamic Clinic Name (SettingsService)          │    │
│  │  • Role-based Menu Visibility                     │    │
│  │  • Profile Dropdown (Change Password + Logout)    │    │
│  │  • Admin Section (Users, Settings)                │    │
│  │  • RTL Layout & Arabic Localization               │    │
│  └──────────────────────────────────────────────────┘    │
│                                                               │
└──────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP Requests/Responses
                              │
┌──────────────────────────────────────────────────────────────┐
│                  Business Logic Layer                         │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐  ┌──────────────────┐                 │
│  │  AuthService     │  │ SettingsService  │                 │
│  ├──────────────────┤  ├──────────────────┤                 │
│  │ • Validate User  │  │ • Get Values     │                 │
│  │ • Hash Password  │  │ • Set Values     │                 │
│  │ • Create Claims  │  │ • Type Conversion│                 │
│  │ • PasswordHasher │  │ • Cache (future) │                 │
│  └──────────────────┘  └──────────────────┘                 │
│                                                               │
└──────────────────────────────────────────────────────────────┘
                              │
                              │ Data Operations
                              │
┌──────────────────────────────────────────────────────────────┐
│                  Data Access Layer                            │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐  ┌──────────────────┐                 │
│  │ AppUserRepository│  │ Repository<T>    │                 │
│  ├──────────────────┤  ├──────────────────┤                 │
│  │ • GetByUsername  │  │ • Get             │                 │
│  │ • GetById        │  │ • Add             │                 │
│  │ • GetAll         │  │ • Update          │                 │
│  │ • Add            │  │ • Delete          │                 │
│  │ • Update         │  │ • Find            │                 │
│  │ • Save           │  │ • SaveChanges     │                 │
│  └──────────────────┘  └──────────────────┘                 │
│                                                               │
│        ApplicationDbContext (Entity Framework Core)           │
│                                                               │
└──────────────────────────────────────────────────────────────┘
                              │
                              │ SQL Commands
                              │
┌──────────────────────────────────────────────────────────────┐
│                  Persistence Layer                            │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│              SQL Server Database                             │
│                                                               │
│  [AppUsers]  [Settings]  [Patients]  [Visits] ...           │
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

## Authorization Flow

```
                            User Requests Login
                                  │
                                  ▼
                    ┌─────────────────────────┐
                    │   AccountController     │
                    │     Login Action        │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼──────────────┐
                    │  Validate Credentials    │
                    │  IAuthService            │
                    │ .ValidateUserAsync()     │
                    └────────────┬──────────────┘
                                 │
                    ┌────────────▼──────────────┐
                    │ Retrieve AppUser         │
                    │ IAppUserRepository       │
                    │ .GetByUsernameAsync()    │
                    └────────────┬──────────────┘
                                 │
                    ┌────────────▼──────────────┐
                    │ Check User Active        │
                    │ Verify Password Hash     │
                    │ PasswordHasher.Verify()  │
                    └────────────┬──────────────┘
                                 │
                    ┌────────────▼──────────────┐
                    │ Valid?                   │
                    └────┬──────────────────┬───┘
                         │                  │
                    Yes  │                  │  No
                    ┌────▼──┐         ┌─────▼─────┐
                    │CREATE │         │ Show Error│
                    │CLAIMS │         │ Message   │
                    │& SIGN │         └───────────┘
                    │IN     │
                    └────┬──┘
                         │
        ┌────────────────▼───────────────────┐
        │    ClaimsPrincipal with:           │
        │    • NameIdentifier (UserId)       │
        │    • Name (Username)               │
        │    • DisplayName                   │
        │    • Role (Admin/Receptionist)     │
        └────────────────┬───────────────────┘
                         │
        ┌────────────────▼───────────────────┐
        │   SignInAsync (Cookie Auth)        │
        │   Set httpOnly Auth Cookie         │
        └────────────────┬───────────────────┘
                         │
                    ┌────▼──────────┐
                    │ Redirect to   │
                    │ Dashboard     │
                    │ or Return URL │
                    └───────────────┘
```

## Role-Based Authorization

```
┌─────────────────────────────────────────────────────────────┐
│                Request Authorization Check                   │
└──────┬──────────────────────────────────────────────────────┘
       │
    ┌──▼────────────────────────────────────┐
    │ Controller has [Authorize] Attribute? │
    └──┬───────────────────────────────────┬┘
       │ No                                 │ Yes
       │                                    │
    ┌──▼──────────────┐        ┌──────────▼──────────────┐
    │ Allow Access    │        │ Extract Role from      │
    │ (No Auth)       │        │ Claims Principal       │
    └─────────────────┘        └──────────┬─────────────┘
                                          │
                             ┌────────────▼──────────────┐
                             │ Check Role Requirements   │
                             │ [Authorize(Roles="Admin")]│
                             └────────┬─────────────────┘
                                      │
                          ┌───────────▼───────────┐
                          │ Role Matches?         │
                          └───┬─────────────────┬─┘
                              │                 │
                         Yes  │                 │  No
                         ┌────▼──┐         ┌────▼────┐
                         │ALLOW  │         │ Return  │
                         │ACCESS │         │ 403     │
                         └────────┘        │Forbidden│
                                           └─────────┘
```

## Data Model Relationships

```
                        ┌─────────────┐
                        │   AppUser   │
                        ├─────────────┤
                        │ Id (PK)     │
                        │ Username(U) │
                        │ PasswordH   │
                        │ DisplayName │
                        │ Role (Enum) │
                        │ IsActive    │
                        │ CreatedAt   │
                        └────┬────────┘
                             │
                             │ 1:Many
                             │ (Future: Associate with actions/logs)
                             │
                        ┌─────────────┐
                        │  Setting    │
                        ├─────────────┤
                        │ Id (PK)     │
                        │ Key         │
                        │ Value       │
                        └─────────────┘

                        Existing Tables (Unchanged):
                        ┌──────────────────────────┐
                        │ Patient, Doctor, etc.    │
                        │ (Relationships           │
                        │ maintained as-is)        │
                        └──────────────────────────┘
```

## Configuration Flow

```
Program.cs
    │
    ├─ AddDbContext<ApplicationDbContext>
    │
    ├─ AddAuthentication (Cookie-based)
    │
    ├─ AddAuthorization
    │  └─ AddPolicy("AdminOnly", Admin role required)
    │
    ├─ AddScoped Services
    │  ├─ IAuthService → AuthService
    │  ├─ ISettingsService → SettingsService
    │  ├─ IAppUserRepository → AppUserRepository
    │  └─ IRepository<T> → Repository<T>
    │
    ├─ Database Migration
    │  └─ dotnet ef database update
    │
    └─ Application Starts
       └─ DatabaseInitializer.SeedUsersAsync()
          └─ Seed admin & receptionist if none exist
```

## Security Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                  Security Layers                             │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│ Layer 1: HTTPS/TLS                                           │
│ └─ All communication encrypted in transit                    │
│                                                               │
│ Layer 2: Authentication (Cookie-based)                       │
│ └─ User validates with username/hashed password              │
│ └─ Server issues httpOnly Auth Cookie                        │
│ └─ Cookie sent with every authenticated request              │
│                                                               │
│ Layer 3: Password Hashing                                    │
│ └─ PasswordHasher<AppUser> with salt                         │
│ └─ Never store plain-text passwords                          │
│ └─ Verify against stored hash at login                       │
│                                                               │
│ Layer 4: Authorization (Role-based)                          │
│ └─ [Authorize(Roles = "Admin")] attributes                   │
│ └─ Role claim extracted from ClaimsPrincipal                 │
│ └─ Request rejected if role doesn't match                    │
│                                                               │
│ Layer 5: CSRF Protection                                     │
│ └─ [ValidateAntiForgeryToken] on all POST/PUT/DELETE         │
│ └─ Hidden token field in forms                               │
│ └─ Token validated on server                                 │
│                                                               │
│ Layer 6: Input Validation                                    │
│ └─ Server-side validation on all inputs                      │
│ └─ ViewModels with [Required], [Range], etc.                 │
│ └─ SQL injection prevented by Entity Framework               │
│                                                               │
│ Layer 7: Secrets Management                                  │
│ └─ No API keys in code                                       │
│ └─ Connection strings in secure configuration                │
│ └─ Environment-specific settings                             │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## Summary

Phase 2 architecture maintains clean separation of concerns:
- **Presentation:** Views with role-based visibility
- **Business Logic:** Services for authentication and settings
- **Data Access:** Repositories for CRUD operations
- **Security:** Multi-layer authorization and validation
- **Database:** Existing schema with new AppUsers table

All components follow established patterns and best practices.
