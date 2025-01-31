using UnityEngine;
using UnityEngine.UI; // Добавляем поддержку старого компонента Text

public class Clicker : MonoBehaviour
{
    private static int clickCount = 0;

    public static int GetClickCount()
    {
        return clickCount;
    }

    public static void AddClicks(int amount)
    {
        clickCount += amount;

        if (clickCount < 0)
        {
            clickCount = 0;
        }

        SaveClickCount();

        AchievementManager achievementManager = FindObjectOfType<AchievementManager>();
        if (achievementManager != null)
        {
            achievementManager.CheckAchievements();
        }

        Debug.Log($"Количество кликов изменено: {clickCount}");
    }

    private static void SaveClickCount()
    {
        PlayerPrefs.SetInt("ClickCount", clickCount);
        PlayerPrefs.Save();
        Debug.Log($"Количество кликов сохранено: {clickCount}");
    }

    public static void LoadClickCount()
    {
        clickCount = PlayerPrefs.GetInt("ClickCount", 0);
        Debug.Log($"Количество кликов загружено: {clickCount}");
    }

    void Start()
    {
        LoadClickCount();
    }

    public void UpdateAllScoreTexts()
    {
        // Находим все текстовые поля с тегом "ScoreText" и обновляем их
        Text[] scoreTexts = FindObjectsOfType<Text>();
        foreach (Text text in scoreTexts)
        {
            if (text.CompareTag("ScoreText"))
            {
                text.text = $"Клики: {clickCount}";
            }
        }
    }

    public static void ResetClickCount()
    {
        clickCount = 0;
        SaveClickCount();
        Debug.Log("Количество кликов сброшено.");
    }
}