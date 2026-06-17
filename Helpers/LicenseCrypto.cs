namespace ClinicManagementSystem.Helpers
{
    public static class LicenseCrypto
    {
        // 🎯 مفتاح سري خاص بيك إنت وفولد فريد (طوله 32 حرف للـ AES-256)
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("Ahm3dSafwatClin1cManag3m3nt2026!"); // 32 bytes
        private static readonly byte[] Iv = Encoding.UTF8.GetBytes("C1in1cSystemIv26"); // 16 bytes

        // ميثود التشفير
        public static string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = Iv;

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                using StreamWriter sw = new(cs);
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        // ميثود فك التشفير
        public static string Decrypt(string cipherText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = Iv;

            using MemoryStream ms = new(Convert.FromBase64String(cipherText));
            using (CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            {
                using StreamReader sr = new(cs);
                return sr.ReadToEnd();
            }
        }
    }
}
