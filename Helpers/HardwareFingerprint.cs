using System.Runtime.Versioning;

namespace ClinicManagementSystem.Helpers;
    
public static class HardwareFingerprint
{
    [SupportedOSPlatform("windows")]
    public static string GetCurrentFingerprint()
    {
        try
        {
            // 1. قراءة الـ Machine GUID الفريد من الـ Registry بتاع الويندوز
            string machineGuid = string.Empty;
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (var cryptographyKey = baseKey.OpenSubKey(@"Software\Microsoft\Cryptography"))
                {
                    machineGuid = cryptographyKey?.GetValue("MachineGuid")?.ToString() ?? "UnknownGUID";
                }
            }

            // 2. دمج الـ Machine GUID مع اسم البروسيسور أو اسم الجهاز لزيادة التعقيد
            string rawId = $"{machineGuid}-{Environment.ProcessorCount}-{Environment.MachineName}";

            // 3. تشفير النص الناتج بـ SHA256 عشان نطلع الـ Hash الفريد (البصمة)
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawId));
                return Convert.ToBase64String(bytes)
                    .Replace("/", "")
                    .Replace("+", "")
                    .Replace("=", "")
                    .Substring(0, 24); // هناخد أول 24 حرف كـ بصمة شيك ونظيفة
            }
        }
        catch (Exception)
        {
            // Fallback في حال حدوث أي مشكلة في الصلاحيات
            return Guid.NewGuid().ToString();
        }
    }
}