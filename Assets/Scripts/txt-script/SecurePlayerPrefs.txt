using System;
using UnityEngine;

public static class SecurePlayerPrefs
{
    private const string encryptionKey = "YourEncryptionKey123"; // Ключ для шифрования

    // Метод для шифрования строки
    private static string Encrypt(string data)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
    }

    // Метод для расшифровки строки
    private static string Decrypt(string encryptedData)
    {
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encryptedData));
    }

    // Сохранение зашифрованного значения
    public static void SetInt(string key, int value)
    {
        string encryptedValue = Encrypt(value.ToString());
        PlayerPrefs.SetString(key, encryptedValue);
    }

    // Получение расшифрованного значения
    public static int GetInt(string key, int defaultValue)
    {
        if (!PlayerPrefs.HasKey(key))
            return defaultValue;

        string encryptedValue = PlayerPrefs.GetString(key);
        try
        {
            string decryptedValue = Decrypt(encryptedValue);
            return int.Parse(decryptedValue);
        }
        catch
        {
            Debug.LogError("Ошибка при расшифровке данных!");
            return defaultValue;
        }
    }
}