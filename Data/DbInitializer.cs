using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            // 🎯 إنشاء الجداول لو مش موجودة
            context.Database.EnsureCreated();

            // 1. توليد الحسابات (Admin & Reception) وتشفير الباسوردات أوتوماتيك
            if (!context.AppUsers.Any())
            {
                var passwordHasher = new PasswordHasher<AppUser>();

                var adminUser = new AppUser
                {
                    Username = "admin",
                    DisplayName = "د. أحمد محمد (المدير)",
                    IsActive = true,
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "admin1");

                var receptionUser = new AppUser
                {
                    Username = "reception",
                    DisplayName = "أستاذة سارة (استقبال)",
                    IsActive = true,
                    Role = UserRole.Receptionist,
                    CreatedAt = DateTime.UtcNow
                };
                receptionUser.PasswordHash = passwordHasher.HashPassword(receptionUser, "123456");

                context.AppUsers.AddRange(adminUser, receptionUser);
                context.SaveChanges();
            }

            // لو الداتا الباقية موجودة اخرج، لو مش موجودة كمل فرش
            if (context.Doctors.Any() || context.Patients.Any())
            {
                return;
            }

            var rand = new Random();
            var today = DateTime.Today;

            // 2. إضافة المنتجات والمستلزمات الطبية للمخزن (Products)
            var products = new List<Product>
            {
                new Product { Name = "علبة كمامات طبية", CostPrice = 45m, IsActive = true, MinimumQuantity = 15 },
                new Product { Name = "كحول طبي 70% (لتر)", CostPrice = 60m, IsActive = true, MinimumQuantity = 15 },
                new Product { Name = "علبة سرنجات 3 سم", CostPrice = 75m, IsActive = true, MinimumQuantity = 15 },
                new Product { Name = "بكرة بلاستر طبي عريض", CostPrice = 35m, IsActive = true, MinimumQuantity = 15 },
                new Product { Name = "باكت قطن طبي كبير", CostPrice = 40m, IsActive = true, MinimumQuantity = 15 },
                new Product { Name = "علبة شاش معقم", CostPrice = 50m, IsActive = true, MinimumQuantity = 15 }
            };
            context.Products.AddRange(products);
            context.SaveChanges();

            // 3. عمل حركات مخزنية لتوريد البضاعة (Stock In) عشان المخزن ميبقاش صفر
            foreach (var prod in products)
            {
                context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = prod.Id,
                    Quantity = 100, // رصيد افتتاحي محترم
                    Type = StockTransactionType.In,
                    CreatedAt = today.AddDays(-30),
                    Reason = "رصيد افتتاحي للديمو"
                });
            }
            context.SaveChanges();

            // 4. إضافة قائمة الإجراءات الطبية الافتراضية
            var procedures = new List<Procedure>
            {
                new Procedure { Name = "غيار على جرح معقم كبير", Price = 80m },
                new Procedure { Name = "جلسة استنشاق / بخار", Price = 50m },
                new Procedure { Name = "عمل رسم قلب كهربائي (ECG)", Price = 120m },
                new Procedure { Name = "تركيب محلول طبي مع كانيولا", Price = 60m },
                new Procedure { Name = "خياطة جرح سطحي (غرز)", Price = 150m }
            };
            context.Procedures.AddRange(procedures);
            context.SaveChanges();

            // 5. إضافة الأطباء
            var doctors = new List<Doctor>
            {
                new Doctor { Name = "د. أحمد محمد", Specialty = "باطنة", IsDeleted = false },
                new Doctor { Name = "د. محمد علي", Specialty = "أطفال", IsDeleted = false },
                new Doctor { Name = "د. محمود حسن", Specialty = "جلدية", IsDeleted = false },
                new Doctor { Name = "د. كريم سامي", Specialty = "عظام", IsDeleted = false },
                new Doctor { Name = "د. ياسر عبد الرحمن", Specialty = "أنف وأذن", IsDeleted = false }
            };
            context.Doctors.AddRange(doctors);
            context.SaveChanges();

            // 6. إضافة المرضى (30 مريض)
            var patientNames = new List<string>
            {
                "أحمد سعيد", "محمد رمضان", "إسلام خالد", "يوسف أحمد", "محمود ناصر",
                "مصطفى محمود", "عبد الرحمن عمرو", "كريم طارق", "هاني عادل", "عمرو دياب",
                "علي حسن", "سامح عبد العزيز", "بلال محمد", "خالد وليد", "تامر حسني",
                "فاطمة أحمد", "سارة محمد", "منى زكي", "ندى أحمد", "مروة علي",
                "هبة الله مصطفى", "إيمان السيد", "شيماء جمال", "رنا ياسر", "مي عز الدين",
                "دعاء كمال", "آية رأفت", "شروق طارق", "نهى محمود", "ياسمين صبري"
            };

            var patients = patientNames.Select(name => new Patient
            {
                Name = name,
                Phone = "01" + rand.Next(10000000, 99999999).ToString(),
                IsDeleted = false
            }).ToList();
            context.Patients.AddRange(patients);
            context.SaveChanges();

            // 7. توليد داتا الزيارات التاريخية لشهر فات (25 زيارة)
            for (int i = 1; i <= 25; i++)
            {
                var randomPatient = patients[rand.Next(patients.Count)];
                var randomDoctor = doctors[rand.Next(doctors.Count)];
                var pastDate = today.AddDays(-rand.Next(1, 30)).AddHours(rand.Next(14, 21));

                var visit = new Visit
                {
                    PatientId = randomPatient.Id,
                    DoctorId = randomDoctor.Id,
                    Date = pastDate,
                    ExaminationPrice = 150m,
                    ProceduresPrice = 0m,
                    Discount = i % 5 == 0 ? 50m : 0m,
                    IsDeleted = false,
                    Status = VisitStatus.Active
                };

                // إضافة الإجراء الطبي للزيارة التاريخية
                decimal calculatedProceduresPrice = 0m;
                var visitProceduresList = new List<VisitProcedure>();

                if (i % 3 == 0)
                {
                    var randomProcedure = procedures[rand.Next(procedures.Count)];
                    int qty = rand.Next(1, 3);
                    decimal subTotal = randomProcedure.Price * qty;

                    visitProceduresList.Add(new VisitProcedure
                    {
                        ProcedureId = randomProcedure.Id,
                        Quantity = qty,
                        SubTotal = subTotal,
                        IsDeleted = false
                    });

                    calculatedProceduresPrice = subTotal;
                }

                visit.ProceduresPrice = calculatedProceduresPrice;
                visit.TotalPrice = (visit.ExaminationPrice + visit.ProceduresPrice) - visit.Discount;

                // هندسة المديونيات
                if (i % 5 == 0)
                    visit.PaidAmount = 0m;
                else if (i % 6 == 0)
                    visit.PaidAmount = visit.TotalPrice - 50m;
                else
                    visit.PaidAmount = visit.TotalPrice;

                context.Visits.Add(visit);
                context.SaveChanges();

                // حفظ جدول الربط للزيارة التاريخية
                if (visitProceduresList.Any())
                {
                    foreach (var vp in visitProceduresList)
                    {
                        vp.VisitId = visit.Id;
                        context.VisitProcedures.Add(vp);
                    }
                    context.SaveChanges();
                }

                // إضافة القيد المالي في الـ Payments
                if (visit.PaidAmount > 0)
                {
                    context.Payments.Add(new Payment
                    {
                        VisitId = visit.Id,
                        Amount = visit.PaidAmount,
                        CreatedAt = pastDate,
                        CreatedBy = "Admin",
                        IsDeleted = false
                    });
                }

                // حركة مخزنية (استهلاك مستلزمات للزيارة)
                if (i % 2 == 0)
                {
                    var randomProduct = products[rand.Next(products.Count)];
                    int consumedQty = rand.Next(1, 4);

                    var stockTx = new StockTransaction
                    {
                        ProductId = randomProduct.Id,
                        VisitId = visit.Id,
                        Quantity = consumedQty,
                        Type = StockTransactionType.Out,
                        CreatedAt = pastDate,
                        Reason = $"استهلاك مستلزمات للزيارة رقم {visit.Id}"
                    };
                    context.StockTransactions.Add(stockTx);
                    context.SaveChanges();

                    context.VisitProductConsumptions.Add(new VisitProductConsumption
                    {
                        VisitId = visit.Id,
                        ProductId = randomProduct.Id,
                        StockTransactionId = stockTx.Id,
                        QuantityConsumed = consumedQty
                    });
                }
            }
            context.SaveChanges();

            // 8. توليد حجوزات وزيارات اليوم (15 حجز متوزعين على مدار اليوم)
            var clinicStartTime = DateTime.Today.AddHours(15); // بتبدأ 3 العصر

            for (int i = 1; i <= 15; i++)
            {
                var patient = patients[i - 1];
                var doctor = doctors[rand.Next(doctors.Count)];
                var appointmentTime = clinicStartTime.AddMinutes((i - 1) * 20);

                var appointment = new Appointment
                {
                    PatientId = patient.Id,
                    DoctorId = doctor.Id,
                    Date = appointmentTime,
                    QueueNumber = i,
                    IsDeleted = false
                };

                if (i <= 8)
                {
                    appointment.Status = AppointmentStatus.Waiting;
                }
                else if (i <= 12)
                {
                    appointment.Status = AppointmentStatus.Done;

                    var visitToday = new Visit
                    {
                        PatientId = patient.Id,
                        DoctorId = doctor.Id,
                        Date = appointmentTime,
                        ExaminationPrice = 150m,
                        ProceduresPrice = 0m, // هيتحسب ديناميكياً تحت
                        Discount = 0m,
                        IsDeleted = false,
                        Status = VisitStatus.Active
                    };

                    // 🎯 تعديل ذكي: ربط زيارات اليوم (الحالة رقم 9 ورقم 11) بإجراء حقيقي من الجدول
                    decimal todayProcedurePrice = 0m;
                    VisitProcedure? todayVp = null;

                    if (i == 9 || i == 11)
                    {
                        var randomProcedure = procedures[rand.Next(procedures.Count)];
                        todayProcedurePrice = randomProcedure.Price * 1; // كمية 1

                        todayVp = new VisitProcedure
                        {
                            ProcedureId = randomProcedure.Id,
                            Quantity = 1,
                            SubTotal = todayProcedurePrice,
                            IsDeleted = false
                        };
                    }

                    visitToday.ProceduresPrice = todayProcedurePrice;
                    visitToday.TotalPrice = visitToday.ExaminationPrice + visitToday.ProceduresPrice;
                    visitToday.PaidAmount = visitToday.TotalPrice;

                    context.Visits.Add(visitToday);
                    context.SaveChanges(); // حفظ عشان ناخد الـ VisitId للربط

                    // حفظ تفاصيل الإجراء لزيارة النهاردة لو وجد
                    if (todayVp != null)
                    {
                        todayVp.VisitId = visitToday.Id;
                        context.VisitProcedures.Add(todayVp);
                        context.SaveChanges();
                    }

                    context.Payments.Add(new Payment
                    {
                        VisitId = visitToday.Id,
                        Amount = visitToday.PaidAmount,
                        CreatedAt = appointmentTime,
                        CreatedBy = "Admin",
                        IsDeleted = false
                    });
                }
                else
                {
                    appointment.Status = AppointmentStatus.Cancelled;
                }

                context.Appointments.Add(appointment);
            }
            context.SaveChanges();
        }
    }
}