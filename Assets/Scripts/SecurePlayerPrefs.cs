using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SecurePlayerPrefs
{
    private const string ENCRYPTION_KEY = "YourEncryptionKey123"; // Секретный ключ шифрования (рекомендуется изменить)
    private static readonly byte[] Key = Encoding.UTF8.GetBytes(ENCRYPTION_KEY.PadRight(16, ' ').Substring(0, 16)); // 128-битный ключ AES
    private const string IV_SUFFIX = "_iv"; // Суффикс для вектора инициализации
    private const string HASH_SUFFIX = "_hash"; // Суффикс для хэша

    #region Encryption/Decryption
    /// <summary>
    /// Шифрует строку с использованием AES.
    /// </summary>
    private static string Encrypt(string data, out byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.GenerateIV();
            iv = aes.IV;

            using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                return Convert.ToBase64String(encryptedBytes);
            }
        }
    }

    /// <summary>
    /// Расшифровывает строку с использованием AES.
    /// </summary>
    private static string Decrypt(string encryptedData, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = iv;

            using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
    #endregion

    #region Hashing
    /// <summary>
    /// Вычисляет SHA-256 хэш для строки.
    /// </summary>
    private static string CalculateHash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    /// <summary>
    /// Проверяет целостность данных через сравнение хэшей.
    /// </summary>
    private static bool VerifyHash(string input, string storedHash)
    {
        return CalculateHash(input).Equals(storedHash, StringComparison.OrdinalIgnoreCase);
    }
    #endregion

    #region Set Methods
    /// <summary>
    /// Сохраняет зашифрованное целое число с проверочным хэшем.
    /// </summary>
    public static void SetInt(string key, int value)
    {
        SaveEncryptedValue(key, value.ToString());
    }

    /// <summary>
    /// Сохраняет зашифрованное число с плавающей точкой с проверочным хэшем.
    /// </summary>
    public static void SetFloat(string key, float value)
    {
        SaveEncryptedValue(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Сохраняет зашифрованную строку с проверочным хэшем.
    /// </summary>
    public static void SetString(string key, string value)
    {
        SaveEncryptedValue(key, value);
    }

    private static void SaveEncryptedValue(string key, string value)
    {
        byte[] iv;
        string encryptedValue = Encrypt(value, out iv);
        string hash = CalculateHash(value);

        PlayerPrefs.SetString(key, encryptedValue);
        PlayerPrefs.SetString(key + IV_SUFFIX, Convert.ToBase64String(iv));
        PlayerPrefs.SetString(key + HASH_SUFFIX, hash);
        PlayerPrefs.Save();
    }
    #endregion

    #region Get Methods
    /// <summary>
    /// Получает расшифрованное целое число с проверкой целостности.
    /// </summary>
    public static int GetInt(string key, int defaultValue)
    {
        return GetValue(key, defaultValue, int.Parse);
    }

    /// <summary>
    /// Получает расшифрованное число с плавающей точкой с проверкой целостности.
    /// </summary>
    public static float GetFloat(string key, float defaultValue)
    {
        return GetValue(key, defaultValue, s => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Получает расшифрованную строку с проверкой целостности.
    /// </summary>
    public static string GetString(string key, string defaultValue)
    {
        return GetValue(key, defaultValue, s => s);
    }

    private static T GetValue<T>(string key, T defaultValue, Func<string, T> parser)
    {
        if (!PlayerPrefs.HasKey(key) || !PlayerPrefs.HasKey(key + IV_SUFFIX))
        {
            return defaultValue;
        }

        try
        {
            string encryptedValue = PlayerPrefs.GetString(key);
            byte[] iv = Convert.FromBase64String(PlayerPrefs.GetString(key + IV_SUFFIX));
            string storedHash = PlayerPrefs.GetString(key + HASH_SUFFIX);

            string decryptedValue = Decrypt(encryptedValue, iv);
            if (VerifyHash(decryptedValue, storedHash))
            {
                return parser(decryptedValue);
            }
            else
            {
                Debug.LogWarning($"Обнаружено вмешательство в данные для ключа: {key}");
                return defaultValue;
            }
        }
        catch (FormatException ex)
        {
            Debug.LogError($"Ошибка формата при расшифровке ключа {key}: {ex.Message}");
            return defaultValue;
        }
        catch (CryptographicException ex)
        {
            Debug.LogError($"Ошибка шифрования при расшифровке ключа {key}: {ex.Message}");
            return defaultValue;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Неизвестная ошибка при расшифровке ключа {key}: {ex.Message}");
            return defaultValue;
        }
    }
    #endregion

    #region Utility
    /// <summary>
    /// Очищает все сохранённые данные в PlayerPrefs.
    /// </summary>
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
    #endregion
}