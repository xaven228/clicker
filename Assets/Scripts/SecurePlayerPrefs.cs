using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SecurePlayerPrefs
{
    private const string encryptionKey = "YourEncryptionKey123"; // Ключ для шифрования
    private static readonly byte[] key = Encoding.UTF8.GetBytes(encryptionKey.Substring(0, 16)); // 128-bit key
    private const string HASH_SUFFIX = "_hash";

    // Метод для шифрования строки
    private static string Encrypt(string data, out byte[] iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.GenerateIV();
            iv = aesAlg.IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] encryptedData = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

            return Convert.ToBase64String(encryptedData);
        }
    }

    // Метод для расшифровки строки
    private static string Decrypt(string encryptedData, byte[] iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] decryptedData = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedData);
        }
    }

    // Метод для вычисления хэша
    private static string CalculateHash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    // Метод для проверки хэша
    private static bool VerifyHash(string input, string hash)
    {
        string newHash = CalculateHash(input);
        return newHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }

    // Сохранение зашифрованного значения для int и хэширование
    public static void SetInt(string key, int value)
    {
        byte[] iv;
        string encryptedValue = Encrypt(value.ToString(), out iv);
        PlayerPrefs.SetString(key, encryptedValue);
        PlayerPrefs.SetString(key + "_iv", Convert.ToBase64String(iv));

        string hash = CalculateHash(value.ToString());
        PlayerPrefs.SetString(key + HASH_SUFFIX, hash);

        PlayerPrefs.Save(); // Убедимся, что данные сохраняются
    }

    // Получение расшифрованного значения для int с проверкой хэша
    public static int GetInt(string key, int defaultValue)
    {
        if (!PlayerPrefs.HasKey(key) || !PlayerPrefs.HasKey(key + "_iv"))
        {
            return defaultValue;
        }

        string encryptedValue = PlayerPrefs.GetString(key);
        byte[] iv = Convert.FromBase64String(PlayerPrefs.GetString(key + "_iv"));
        string savedHash = PlayerPrefs.GetString(key + HASH_SUFFIX);

        try
        {
            string decryptedValue = Decrypt(encryptedValue, iv);
            if (VerifyHash(decryptedValue, savedHash))
            {
                return int.Parse(decryptedValue);
            }
            else
            {
                Debug.LogWarning($"Data tampering detected for key: {key}");
                return defaultValue;
            }
        }
        catch (FormatException ex)
        {
            Debug.LogError($"Ошибка при расшифровке данных для ключа {key}: {ex.Message}");
            return defaultValue;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Неизвестная ошибка при расшифровке данных для ключа {key}: {ex.Message}");
            return defaultValue;
        }
    }

    // Сохранение зашифрованного значения для float и хэширование
    public static void SetFloat(string key, float value)
    {
        byte[] iv;
        string encryptedValue = Encrypt(value.ToString(), out iv);
        PlayerPrefs.SetString(key, encryptedValue);
        PlayerPrefs.SetString(key + "_iv", Convert.ToBase64String(iv));

        string hash = CalculateHash(value.ToString());
        PlayerPrefs.SetString(key + HASH_SUFFIX, hash);

        PlayerPrefs.Save();
    }

    // Получение расшифрованного значения для float с проверкой хэша
    public static float GetFloat(string key, float defaultValue)
    {
        if (!PlayerPrefs.HasKey(key) || !PlayerPrefs.HasKey(key + "_iv"))
        {
            return defaultValue;
        }

        string encryptedValue = PlayerPrefs.GetString(key);
        byte[] iv = Convert.FromBase64String(PlayerPrefs.GetString(key + "_iv"));
        string savedHash = PlayerPrefs.GetString(key + HASH_SUFFIX);

        try
        {
            string decryptedValue = Decrypt(encryptedValue, iv);
            if (VerifyHash(decryptedValue, savedHash))
            {
                return float.Parse(decryptedValue);
            }
            else
            {
                Debug.LogWarning($"Data tampering detected for key: {key}");
                return defaultValue;
            }
        }
        catch (FormatException ex)
        {
            Debug.LogError($"Ошибка при расшифровке данных для ключа {key}: {ex.Message}");
            return defaultValue;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Неизвестная ошибка при расшифровке данных для ключа {key}: {ex.Message}");
            return defaultValue;
        }
    }

    // Сохранение зашифрованного значения для string и хэширование
    public static void SetString(string key, string value)
    {
        byte[] iv;
        string encryptedValue = Encrypt(value, out iv);
        PlayerPrefs.SetString(key, encryptedValue);
        PlayerPrefs.SetString(key + "_iv", Convert.ToBase64String(iv));

        string hash = CalculateHash(value);
        PlayerPrefs.SetString(key + HASH_SUFFIX, hash);

        PlayerPrefs.Save();
    }

    // Получение расшифрованного значения для string с проверкой хэша
    public static string GetString(string key, string defaultValue)
    {
        if (!PlayerPrefs.HasKey(key) || !PlayerPrefs.HasKey(key + "_iv"))
        {
            return defaultValue;
        }

        string encryptedValue = PlayerPrefs.GetString(key);
        byte[] iv = Convert.FromBase64String(PlayerPrefs.GetString(key + "_iv"));
        string savedHash = PlayerPrefs.GetString(key + HASH_SUFFIX);

        try
        {
            string decryptedValue = Decrypt(encryptedValue, iv);
            if (VerifyHash(decryptedValue, savedHash))
            {
                return decryptedValue;
            }
            else
            {
                Debug.LogWarning($"Data tampering detected for key: {key}");
                return defaultValue;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка при расшифровке данных для ключа {key}: {ex.Message}");
            return defaultValue;
        }
    }

    // Очищаем PlayerPrefs
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}