using ClinicManagementSystem.DTOs.License;
using ClinicManagementSystem.Helpers;
using System.Runtime.Versioning;
using System.Text.Json;

namespace ClinicManagementSystem.Services
{
    public interface ILicenseService
    {
        LicenseDto GetCurrentLicenseInfo();
        bool IsExpired();
        int GetRemainingDays();
        // 🎯 الميثود الجديدة لاستقبال باكيج التفعيل من الـ Controller
        bool ActivateSystemWithPackage(string fullPackageKey, out string errorMessage);
    }

    public class LicenseService : ILicenseService
    {
        private LicenseDto _currentLicense;
        private readonly string _licenseFilePath = Path.Combine(AppContext.BaseDirectory, "license.dat");
        private const string RegistryPath = @"SOFTWARE\ClinicManagementSystem";
        private const string LicensePrefix = "CMS-LIC:";

        public LicenseService()
        {
            _currentLicense = new LicenseDto();

            if (OperatingSystem.IsWindows())
            {
                InitializeOrValidateLicense();
            }
        }

        [SupportedOSPlatform("windows")]
        private void InitializeOrValidateLicense()
        {
            string currentFingerprint = HardwareFingerprint.GetCurrentFingerprint();

            // 🛡️ [Anti-Reset] فحص لو الدكتور مسح الملف عشان يجدد الـ Trial
            if (!File.Exists(_licenseFilePath))
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    var firstLaunch = key?.GetValue("FirstLaunchDate");

                    if (firstLaunch != null)
                    {
                        // 🛑 قفشناه: مسح الملف بس الـ Registry فاكره، هنرجع رخصة منتهية بتاريخ قديم
                        _currentLicense = new LicenseDto
                        {
                            ClinicName = "Trial Expired (Reset Attempted)",
                            LicenseType = "Trial Expired",
                            IsDeviceValid = true,
                            ExpirationDate = DateTime.Today.AddDays(-1), // 🎯 الحل: تاريخ امبارح هيخلي IsTrialExpired تلقائياً true
                            DeviceFingerprint = currentFingerprint,
                            Version = "1.0.0 (MVP)"
                        };
                        return;
                    }
                }

                // 🆕 أول تشغيل حقيقي ونظيف للسيستم مطلقاً على هذا الجهاز
                DateTime trialExpiry = DateTime.Today.AddDays(30);

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    key.SetValue("FirstLaunchDate", DateTime.UtcNow.ToString("O")); // حفظ التوقيت بـ ISO UTC Format
                }

                CreateTrialLicenseFile(currentFingerprint, trialExpiry);
            }
            else
            {
                try
                {
                    string encryptedContent = File.ReadAllText(_licenseFilePath).Trim();
                    // 🎯 تعديل 1: استخدام كلاس التشفير الموحد مباشرة
                    string decryptedJson = LicenseCrypto.Decrypt(encryptedContent);

                    var licenseData = JsonSerializer.Deserialize<LicenseDataModel>(decryptedJson) ?? new LicenseDataModel();

                    if (licenseData != null)
                    {
                        // 🎯 تم إزالة سطر الـ WRONG-DEVICE-ID التجريبي ليعود كود الإنتاج سليم 100%
                        _currentLicense = new LicenseDto
                        {
                            ClinicName = licenseData.ClinicName,
                            LicenseType = licenseData.LicenseType,
                            ExpirationDate = licenseData.ExpirationDate,
                            Version = "1.0.0 (MVP)",
                            IsDeviceValid = licenseData.Fingerprint == currentFingerprint, // فحص البصمة الحقيقي
                            DeviceFingerprint = currentFingerprint
                        };
                    }
                }
                catch (Exception)
                {
                    // لو حصل تلاعب، اقفل السيستم واعرض البصمة برضه عشان التفعيل
                    _currentLicense = new LicenseDto
                    {
                        IsDeviceValid = false,
                        DeviceFingerprint = currentFingerprint
                    };
                }
            }
        }

        [SupportedOSPlatform("windows")]
        public bool ActivateSystemWithPackage(string fullPackageKey, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // 🎯 تعديل 2: استخدام الـ Range Operator بدلاً من Replace لضمان دقة الـ Parsing
                if (string.IsNullOrEmpty(fullPackageKey) || !fullPackageKey.StartsWith(LicensePrefix))
                {
                    errorMessage = "كود التفعيل غير صحيح أو مكتوب بصيغة خاطئة.";
                    return false;
                }

                string cipherText = fullPackageKey[LicensePrefix.Length..].Trim();

                // فك تشفير الحزمة الـ كاملة القادمة من الـ Generator
                string decryptedJson = LicenseCrypto.Decrypt(cipherText);

                var payload = JsonSerializer.Deserialize<LicensePayloadModel>(decryptedJson);
                if (payload == null)
                {
                    errorMessage = "حزمة التفعيل تالفة أو غير صالحة.";
                    return false;
                }

                // 🛑 فحص الأمان الحرج والأهم (هل البصمة مطابقة للجهاز الحالي؟)
                string currentFingerprint = HardwareFingerprint.GetCurrentFingerprint();
                if (payload.Fingerprint != currentFingerprint)
                {
                    errorMessage = "عذراً، كود التفعيل هذا مخصص لجهاز آخر ومقفل برمجياً.";
                    return false;
                }

                // بناء كائن التخزين الجديد وتحديث ملف الـ license.dat المحلي
                var updatedStorage = new LicenseDataModel
                {
                    Fingerprint = currentFingerprint,
                    ClinicName = payload.ClinicName,
                    LicenseType = payload.LicenseType,
                    ExpirationDate = payload.ExpirationDate,
                    LicenseKey = cipherText,
                    ActivatedAt = DateTime.UtcNow // 🎯 تعديل 3: تسجيل تاريخ التفعيل بالـ UTC دايماً
                };

                string jsonToStore = JsonSerializer.Serialize(updatedStorage);
                string encryptedFileContent = LicenseCrypto.Encrypt(jsonToStore);

                File.WriteAllText(_licenseFilePath, encryptedFileContent);

                // تحديث الـ Memory State فوراً لفتح السيستم في نفس اللحظة
                _currentLicense = new LicenseDto
                {
                    ClinicName = updatedStorage.ClinicName,
                    LicenseType = updatedStorage.LicenseType,
                    ExpirationDate = updatedStorage.ExpirationDate,
                    Version = $"1.0.0 ({payload.LicenseType})",
                    IsDeviceValid = true,
                    DeviceFingerprint = currentFingerprint
                };

                return true;
            }
            catch (Exception)
            {
                errorMessage = "فشل فك تشفير كود التفعيل. تأكد من نسخ الكود بالكامل وبشكل صحيح.";
                return false;
            }
        }

        private void CreateTrialLicenseFile(string fingerprint, DateTime expiryDate)
        {
            _currentLicense = new LicenseDto
            {
                ClinicName = "Trial Version Clinic",
                LicenseType = "Trial Version",
                ExpirationDate = expiryDate,
                Version = "1.0.0 (MVP)",
                IsDeviceValid = true,
                DeviceFingerprint = fingerprint
            };

            var licenseData = new LicenseDataModel
            {
                Fingerprint = fingerprint,
                ClinicName = _currentLicense.ClinicName,
                LicenseType = _currentLicense.LicenseType,
                ExpirationDate = _currentLicense.ExpirationDate,
                ActivatedAt = DateTime.UtcNow // 🎯 تعديل 3: وقت الـ UTC
            };

            string jsonString = JsonSerializer.Serialize(licenseData);
            string encryptedData = LicenseCrypto.Encrypt(jsonString);
            File.WriteAllText(_licenseFilePath, encryptedData);
        }

        public LicenseDto GetCurrentLicenseInfo() => _currentLicense;
        public bool IsExpired()
        {
            if (!_currentLicense.IsDeviceValid) return true;

            return DateTime.Today.Date > _currentLicense.ExpirationDate.Date;
        }
        public int GetRemainingDays() => _currentLicense.DaysRemaining;
    }

    // 🎯 الموديل الخاص بقراءة حزمة البيانات القادمة من الـ Generator المشفر
    public class LicensePayloadModel
    {
        public string Fingerprint { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
    }

    public class LicenseDataModel
    {
        public string Fingerprint { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public string LicenseKey { get; set; } = string.Empty;
        public DateTime ActivatedAt { get; set; }
    }
}