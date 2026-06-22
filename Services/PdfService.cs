using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClinicManagementSystem.DTOs.Financial;

namespace ClinicManagementSystem.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateFinancialReport(FinancialReportDataDto reportData)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").Size(10)); // فونت موحد ومتناسق
                    page.ContentFromRightToLeft(); // دعم كامل للعربية

                    // 1️⃣ الهيدر (رأس الصفحة)
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(reportData.Title).FontSize(20).Bold().FontColor("#1a252f");
                            col.Item().Text(reportData.Subtitle).FontSize(11).FontColor(Colors.Grey.Medium);
                            // ⏱️ إضافة تاريخ إنشاء التقرير بدقة في الهيدر
                            col.Item().Text($"تاريخ الإصدار: {reportData.GeneratedAt:yyyy/MM/dd - hh:mm tt}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(120).AlignLeft().AlignMiddle().Column(col =>
                        {
                            col.Item().Text("المركز الطبي التخصصي").FontSize(12).Bold().FontColor("#1abc9c").AlignLeft();
                            col.Item().Text("نظام إدارة العيادات").FontSize(9).FontColor(Colors.Grey.Medium).AlignLeft();
                        });
                    });

                    // 2️⃣ المحتوى الأساسي (Revenue Report Layout)
                    page.Content().PaddingVertical(0.8f, Unit.Centimetre).Column(col =>
                    {
                        // صف الكروت الذكية (KPI Cards) - تم تنسيقها لتناسب تقرير الإيرادات فقط
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Padding(4).Border(1).BorderColor(Colors.Grey.Lighten2).Background("#ebf5fb").Padding(8).Column(c => {
                                c.Item().Text("إجمالي الإيرادات المحصلة").FontSize(9).FontColor(Colors.Grey.Darken2);
                                c.Item().Text($"{reportData.TotalRevenue:N2} ج.م").FontSize(12).Bold().FontColor("#2980b9");
                            });

                            row.RelativeItem().Padding(4).Border(1).BorderColor(Colors.Grey.Lighten2).Background("#fdedec").Padding(8).Column(c => {
                                c.Item().Text("المبالغ المستحقة (غير المحصلة)").FontSize(9).FontColor(Colors.Grey.Darken2);
                                c.Item().Text($"{reportData.OutstandingAmount:N2} ج.م").FontSize(12).Bold().FontColor("#c0392b");
                            });

                            row.RelativeItem().Padding(4).Border(1).BorderColor(Colors.Grey.Lighten2).Background("#e8f8f5").Padding(8).Column(c => {
                                c.Item().Text("متوسط إيراد الزيارة").FontSize(9).FontColor(Colors.Grey.Darken2);
                                c.Item().Text($"{reportData.AverageVisitRevenue:N2} ج.م").FontSize(12).Bold().FontColor("#16a085");
                            });
                        });

                        // صف المؤشرات التشغيلية
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Padding(4).Border(1).BorderColor(Colors.Grey.Lighten2).Background("#f4f6f7").Padding(8).Column(c => {
                                c.Item().Text("إجمالي عدد الزيارات").FontSize(9).FontColor(Colors.Grey.Darken2);
                                c.Item().Text($"{reportData.TotalVisits} زيارة").FontSize(11).Bold().FontColor("#7f8c8d");
                            });

                            row.RelativeItem().Padding(4).Border(1).BorderColor(Colors.Grey.Lighten2).Background("#f4f6f7").Padding(8).Column(c => {
                                c.Item().Text("عدد حركات الدفع").FontSize(9).FontColor(Colors.Grey.Darken2);
                                c.Item().Text($"{reportData.TotalPayments} حركة").FontSize(11).Bold().FontColor("#7f8c8d");
                            });
                        });

                        // عنوان الجدول
                        col.Item().PaddingTop(0.8f, Unit.Centimetre).Text("تفاصيل الحركات المادية والخزنة").FontSize(12).Bold().FontColor("#1a252f");

                        // 📊 الجدول التفصيلي
                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);  // م
                                columns.ConstantColumn(110); // التاريخ والوقت
                                columns.ConstantColumn(100); // نوع الحركة
                                columns.RelativeColumn();    // 🎯 الحل: تعديل الاسم لـ RelativeColumn
                                columns.ConstantColumn(80);  // القيمة
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#1a252f").Padding(5).AlignCenter().Text("م").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1a252f").Padding(5).Text("التاريخ").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1a252f").Padding(5).Text("نوع الحركة").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1a252f").Padding(5).Text("البيان / الوصف").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1a252f").Padding(5).AlignLeft().Text("القيمة").Bold().FontColor(Colors.White);
                            });

                            int index = 1;
                            foreach (var tx in reportData.Transactions)
                            {
                                // 🎯 الحل: ضبط الـ Background Color لتكون متوافقة بالكامل كـ كائنات ملونة وليس string صريح
                                var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                                table.Cell().Background(bgColor).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).AlignCenter().Text(index.ToString());
                                table.Cell().Background(bgColor).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(tx.Date.ToString("yyyy-MM-dd hh:mm tt"));
                                table.Cell().Background(bgColor).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(tx.Type);
                                table.Cell().Background(bgColor).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(tx.Description);
                                table.Cell().Background(bgColor).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).AlignLeft().Text($"{tx.Amount:N2}");

                                index++;
                            }
                        });
                    });

                    // 3️⃣ الفوتر (أسفل الصفحة)
                    page.Footer().AlignCenter().Row(row =>
                    {
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("صفحة ").FontSize(8).FontColor(Colors.Grey.Medium);
                            t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                            t.Span(" من ").FontSize(8).FontColor(Colors.Grey.Medium);
                            t.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}