using UnityEngine;
using UnityEngine.UI;

public class Clicker : MonoBehaviour
{
    // Статическое поле для хранения количества кликов
    private static int clickCount = 0;

    // Глобальный множитель кликов (по умолчанию 1)
    private static float globalMultiplier = 1f;

    // Метод для получения текущего количества кликов
    public static int GetClickCount()
    {
        return clickCount;
    }

    // Метод для добавления кликов
    public static void AddClicks(int amount)
    {
        // Учитываем глобальный множитель при добавлении кликов
        clickCount += Mathf.FloorToInt(amount * globalMultiplier);

        if (clickCount < 0)
        {
            clickCount = 0;
        }

        SaveClickCount();
        Debug.Log($"Количество кликов изменено: {clickCount}");
    }

    // Метод для установки точного значения кликов
    public static void SetClickCount(int newCount)
    {
        clickCount = newCount;

        if (clickCount < 0)
        {
            clickCount = 0;
        }

        SaveClickCount();
        Debug.Log($"Количество кликов установлено: {clickCount}");
    }

    // Метод для получения текущего значения глобального множителя
    public static float GetGlobalMultiplier()
    {
        return globalMultiplier;
    }

    // Метод для увеличения глобального множителя
    public static void IncreaseGlobalMultiplier(float bonus)
    {
        globalMultiplier += bonus;
        Debug.Log($"Глобальный множитель увеличен на {bonus}. Текущий множитель: {globalMultiplier}");
    }

    // Метод для сохранения количества кликов
    private static void SaveClickCount()
    {
        PlayerPrefs.SetInt("ClickCount", clickCount);
        PlayerPrefs.Save();
        Debug.Log($"Количество кликов сохранено: {clickCount}");
    }

    // Метод для загрузки количества кликов
    public static void LoadClickCount()
    {
        clickCount = PlayerPrefs.GetInt("ClickCount", 0);
        Debug.Log($"Количество кликов загружено: {clickCount}");
    }

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Загружаем количество кликов при старте игры
        LoadClickCount();

        // Обновляем текстовые поля
        UpdateAllScoreTexts();
    }

    // Метод для обновления текстовых полей с количеством кликов
    public void UpdateAllScoreTexts()
    {
        // Находим все текстовые поля с тегом "ScoreText" и обновляем их
        Text[] scoreTexts = FindObjectsByType<Text>(FindObjectsSortMode.None); // Заменяем FindObjectsOfType
        foreach (Text text in scoreTexts)
        {
            if (text.CompareTag("ScoreText"))
            {
                text.text = $"Клики: {clickCount}";
            }
        }
    }

    // Метод для сброса количества кликов (для тестирования)
    public static void ResetClickCount()
    {
        clickCount = 0;
        SaveClickCount();
        Debug.Log("Количество кликов сброшено.");
    }
}