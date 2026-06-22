using ClosedXML.Excel;
using System.Reflection;

namespace ClinicManagementSystem.Services
{
    public class ExcelService : IExcelService
    {
        public byte[] ExportExcel<T>(IEnumerable<T> data, string sheetName)
        {
            using (var workbook = new XLWorkbook())
            {
                // 1. إنشاء الشيت وضبط الاتجاه إلى اليمين (RTL) لأن السيستم عربي
                var worksheet = workbook.Worksheets.Add(sheetName);
                worksheet.RightToLeft = true;

                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                int currentRow = 1;

                // 2. تصميم الـ Header (العناوين)
                for (int i = 0; i < properties.Length; i++)
                {
                    // لو حابب تستخدم DisplayName Attribute مستقبلاً، أو ياخد اسم الـ Property علطول
                    worksheet.Cell(currentRow, i + 1).Value = properties[i].Name;
                }

                // ستايل الـ Header (لون كحلي غامق وكلام أبيض Bold)
                var headerRange = worksheet.Range(1, 1, 1, properties.Length);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a252f");
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // 3. تعبئة البيانات (Data Rows)
                foreach (var item in data)
                {
                    currentRow++;
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var value = properties[i].GetValue(item, null);
                        worksheet.Cell(currentRow, i + 1).Value = value?.ToString() ?? "-";
                    }
                }

                // 4. تحسين الشكل العام (Borders & AutoFit)
                var dataRange = worksheet.Range(1, 1, currentRow, properties.Length);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // تظبيط مقاسات الأعمدة أوتوماتيك على قد الكلام
                worksheet.Columns().AdjustToContents();

                // 5. حفظ الملف في الـ Memory وترجيعه كـ Array of bytes
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
