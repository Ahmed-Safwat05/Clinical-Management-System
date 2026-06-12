# 🏥 Clinical Management System

ASP.NET MVC-based Clinical Management System designed to streamline patient onboarding, doctor scheduling, dynamic queue management, and comprehensive clinical billing. Built using clean architecture principles, repository pattern, and secure role-based workflows.

---

## 🚀 Key Features

- **Multi-role Authentication & Authorization:** Secure cookie-based access control separating System Admins from Front-desk Receptionists.
- **Dynamic Queue Management:** Automated, real-time token and queue generation per doctor for walk-in and scheduled appointments.
- **Advanced Soft Delete System:** Built using **EF Core Global Query Filters** and overridden `SaveChanges()` to archive patient data safely while preserving historical medical and financial ledger records.
- **Dual Theme Support:** Fully responsive interface with an optimized **Dark/Light Mode** toggle for clinical night-shift environments.
- **Audit Logging & Security:** Detailed tracking of critical data alterations (Who modified what and when) for absolute data compliance.
- **Financial Tracking:** Handles complex visit procedures, partial payments, and updates remaining balance states seamlessly.

---

## 🏗️ Architecture & Patterns

- **Pattern:** Model-View-Controller (MVC) with strict Separation of Concerns.
- **Data Access:** Repository Pattern & Unit of Work for decoupled database testing and interactions.
- **Business Logic:** Dedicated Service Layer mediating between Controllers and Repositories.
- **DI:** Native Dependency Injection ensuring loosely coupled components.

---

## 🛠️ Tech Stack

- **Backend:** .NET / ASP.NET MVC (C#)
- **Data:** Entity Framework Core (Code-First), LINQ, SQL Server
- **Frontend:** HTML5, CSS3 (Bootstrap 5 with custom variables), JavaScript (AJAX / Fetch API)
- **DevOps & Tools:** Git, GitHub, Migration Workflows

---
## 📸 System Showcase

<details>
<summary><b>👁️ Click here to expand all screenshots</b></summary>

### 🔐 Authentication & Control Panels
| Login | User Management |Add User| Audit Logs |
| :---: | :---: | :---: | :---: |
| ![Login](Screenshots/login.png) | ![User Management](Screenshots/users.png) | ![Add User](Screenshots/adduser.png) | ![Audit Logs](Screenshots/auditlogs.png) |

### 📊 Home & Themes

| Home (Dark) | Home (Light) |
| :---: | :---: | :---: |
| ![Dark Home](Screenshots/darkhome.png) | ![Light Home](Screenshots/lighthome.png) |

### 👥 Patients Management
| Patients List | Add Patient | Patient History | Patient Details | Patient Search |
| :---: | :---: | :---: | :---: | :---: |
| ![Patients](Screenshots/patients.png) | ![Patient Add](Screenshots/patientadd.png) | ![Patient History](Screenshots/patienthistory.png) | ![Patient Details](Screenshots/patientdetails.png) | ![Patient Search](Screenshots/patientsearch.png) |
 
### Doctors Management
| Doctors List | Add Doctor |
| :---: | :---: |
| ![Doctors](Screenshots/doctors.png) | ![Doctor Add](Screenshots/doctoradd.png) |

### Appointments
| Appointments List | New Appointment |
| :---: | :---: |
| ![Appointments](Screenshots/appointments.png) | ![New Appointment](Screenshots/newappointment.png) |

### 🩺 Medical Visits & Billing
| Visits List | New Visit | Visit Details | Add Payment |
| :---: | :---: | :---: | :---: |
| ![Visits](Screenshots/visits.png) | ![New Visit](Screenshots/newvisit.png) | ![Visit Details](Screenshots/visitdetails.png) | ![Add Payment](Screenshots/addpayment.png) |

### 📦 Inventory & System Setup
| Procedures Setup | Product Inventory | Product Details | Add Product |
| :---: | :---: | :---: | :---: |
| ![Procedures](Screenshots/procedures.png) | ![Inventory](Screenshots/inventory.png) | ![Product Details](Screenshots/productdetails.png) | ![Add Product](Screenshots/addproduct.png) |

### 📊 Dashboard & Restore Trash
| Dashboard | Trash |
| :---: | :---: |
| ![Dashboard](Screenshots/dashboard.png) | ![Trash](Screenshots/deleted.png) |

</details>
---

## 📂 Documentation

Detailed system documentation, architecture decisions, and refactoring logs are available inside the `Docs/` folder.

## 🚧 Future Improvements

- [ ] Transitioning to a distributed architecture using **ASP.NET Core Web API**.
- [ ] Implementing **JWT Authentication** and Secure Token Management.
- [ ] Automated **PDF Generation** for patient prescriptions and invoices.
- [ ] Containerization using **Docker** and deployment pipelines to Cloud (Azure/AWS).
- [ ] Real-time updates for patient queue tracking using **SignalR**.