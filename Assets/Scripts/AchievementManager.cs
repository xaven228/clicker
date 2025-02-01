using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Achievement
{
    public string achievementName;  // Название достижения
    public string description;      // Описание достижения
    public int maxProgress;         // Максимальный прогресс (например, 100 или 1000)
    public int currentProgress;     // Текущий прогресс
    public bool isUnlocked;         // Статус выполнения достижения

    // Для отображения статуса
    public Text achievementText;    // Текстовое поле для отображения статуса
    public Image achievementIcon;   // Иконка для отображения статуса (можно использовать для цветовой индикации)
}

public class AchievementManager : MonoBehaviour
{
    public Achievement[] achievements;  // Массив достижений
    public Clicker clicker;  // Ссылка на Clicker для получения текущего количества кликов
    public MessageManager messageManager;  // Ссылка на MessageManager для вывода сообщений

    private const string UNLOCKED_KEY_PREFIX = "AchievementUnlocked_";

    private void OnEnable()
    {
        // Подписываемся на событие добавления кликов
        Clicker.OnClickAdded += CheckAchievements;
    }

    private void OnDisable()
    {
        // Отписываемся от события
        Clicker.OnClickAdded -= CheckAchievements;
    }

    private void Start()
    {
        // Загружаем состояние достижений
        LoadAchievements();

        // Выполняем начальную проверку достижений
        CheckAchievements(0);  // Начальная проверка достижений с нулевым количеством кликов
    }

    public void CheckAchievements(int addedClicks)
    {
        int totalClicks = Clicker.GetClickCount();  // Получаем общее количество кликов

        foreach (var achievement in achievements)
        {
            if (!achievement.isUnlocked)
            {
                // Обновляем прогресс для каждого достижения
                if (totalClicks >= achievement.maxProgress && achievement.currentProgress < achievement.maxProgress)
                {
                    achievement.currentProgress = achievement.maxProgress;  // Завершаем достижение
                    UnlockAchievement(achievement);  // Разблокируем достижение
                }
                else if (totalClicks < achievement.maxProgress)
                {
                    achievement.currentProgress = totalClicks;  // Обновляем прогресс
                }
            }
        }

        // Обновляем UI с новыми данными
        UpdateAchievementUI();
    }

    private void UnlockAchievement(Achievement achievement)
    {
        achievement.isUnlocked = true;

        // Сохраняем прогресс достижения в PlayerPrefs
        string unlockedKey = UNLOCKED_KEY_PREFIX + achievement.achievementName;
        PlayerPrefs.SetInt(unlockedKey, 1);  // 1 означает, что достижение выполнено
        PlayerPrefs.Save();

        // Показываем сообщение
        if (messageManager != null)
        {
            messageManager.ShowMessage($"Достижение выполнено: {achievement.achievementName}");
        }

        Debug.Log($"Достижение '{achievement.achievementName}' выполнено!");
    }

    private void LoadAchievements()
    {
        foreach (var achievement in achievements)
        {
            string unlockedKey = UNLOCKED_KEY_PREFIX + achievement.achievementName;
            achievement.isUnlocked = PlayerPrefs.GetInt(unlockedKey, 0) == 1;
            achievement.currentProgress = PlayerPrefs.GetInt("AchievementProgress_" + achievement.achievementName, 0);  // Загружаем сохранённый прогресс
            Debug.Log($"Загружено достижение: {achievement.achievementName}, выполнено: {achievement.isUnlocked}, прогресс: {achievement.currentProgress}");
        }
    }

    public void UpdateAchievementUI()
    {
        foreach (var achievement in achievements)
        {
            // Обновление текста и иконки для отображения статуса достижения
            if (achievement.achievementText != null)
            {
                if (achievement.isUnlocked)
                {
                    achievement.achievementText.text = $"Достижение выполнено: {achievement.achievementName}";
                    if (achievement.achievementIcon != null)
                        achievement.achievementIcon.color = Color.green;  // Зеленый цвет для выполненного достижения
                }
                else
                {
                    achievement.achievementText.text = $"Прогресс: {achievement.currentProgress}/{achievement.maxProgress} - {achievement.achievementName}";
                    if (achievement.achievementIcon != null)
                        achievement.achievementIcon.color = Color.red;  // Красный цвет для невыполненного достижения
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveAllAchievementsProgress();
    }

    private void SaveAllAchievementsProgress()
    {
        foreach (var achievement in achievements)
        {
            SaveAchievementProgress(achievement);  // Сохраняем прогресс каждого достижения
        }
    }

    public void SaveAchievementProgress(Achievement achievement)
    {
        string progressKey = "AchievementProgress_" + achievement.achievementName;
        PlayerPrefs.SetInt(progressKey, achievement.currentProgress);  // Сохраняем текущий прогресс
        PlayerPrefs.Save();
    }
}