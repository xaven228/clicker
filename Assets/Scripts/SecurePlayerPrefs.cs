using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SecurePlayerPrefs
{
    private const string encryptionKey = "YourEncryptionKey123"; // Ключ для шифрования
    private static readonly byte[] key = Encoding.UTF8.GetBytes(encryptionKey.Substring(0, 16)); // 128-bit key
    private static readonly byte[] iv = new byte[16]; // Инициализационный вектор (можно использовать случайный для повышения безопасности)

    // Метод для шифрования строки
    private static string Encrypt(string data)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] encryptedData = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

            return Convert.ToBase64String(encryptedData);
        }
    }

    // Метод для расшифровки строки
    private static string Decrypt(string encryptedData)
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

    // Сохранение зашифрованного значения
    public static void SetInt(string key, int value)
    {
        string encryptedValue = Encrypt(value.ToString());
        PlayerPrefs.SetString(key, encryptedValue);
        PlayerPrefs.Save(); // Убедимся, что данные сохраняются
    }

    // Получение расшифрованного значения
    public static int GetInt(string key, int defaultValue)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            return defaultValue;
        }

        string encryptedValue = PlayerPrefs.GetString(key);

        try
        {
            string decryptedValue = Decrypt(encryptedValue);
            return int.Parse(decryptedValue);
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

    // Дополнительно можно добавить методы для работы с другими типами данных, например, для string, float и т.д.
}
