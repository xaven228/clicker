using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Achievement
{
    public string name; // Название достижения
    public int targetClicks; // Цель по количеству кликов
    public bool isUnlocked = false; // Флаг, указывающий, разблокировано ли достижение
}

public class AchievementManager : MonoBehaviour
{
    // Массив достижений
    public Achievement[] achievements;

    // Ссылка на Clicker для получения текущего количества кликов
    public Clicker clicker;

    // Ссылка на MessageManager для вывода сообщений о разблокировке
    public MessageManager messageManager;

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Проверяем, что все ссылки назначены
        if (clicker == null)
        {
            Debug.LogError("Ссылка на Clicker не назначена!");
            return;
        }

        // Загружаем состояние достижений
        LoadAchievements();

        // Проверяем достижения при старте игры
        CheckAchievements();
    }

    // Метод для проверки достижений
    public void CheckAchievements()
    {
        int currentClicks = Clicker.GetClickCount(); // Получаем текущее количество кликов

        foreach (var achievement in achievements)
        {
            // Если достижение ещё не разблокировано и цель достигнута
            if (!achievement.isUnlocked && currentClicks >= achievement.targetClicks)
            {
                UnlockAchievement(achievement);
            }
        }
    }

    // Метод для разблокировки достижения
    private void UnlockAchievement(Achievement achievement)
    {
        achievement.isUnlocked = true;

        // Сохраняем состояние достижения
        SaveAchievement(achievement);

        // Выводим сообщение о разблокировке
        if (messageManager != null)
        {
            messageManager.ShowMessage($"Достижение разблокировано: {achievement.name}");
        }
        else
        {
            Debug.LogWarning("Ссылка на MessageManager не назначена! Сообщение о разблокировке не показано.");
        }

        Debug.Log($"Достижение '{achievement.name}' разблокировано!");
    }

    // Метод для сохранения состояния достижения
    private void SaveAchievement(Achievement achievement)
    {
        string key = $"Achievement_{achievement.name}_IsUnlocked";
        PlayerPrefs.SetInt(key, achievement.isUnlocked ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Состояние достижения '{achievement.name}' сохранено: {achievement.isUnlocked}");
    }

    // Метод для загрузки состояния достижений
    private void LoadAchievements()
    {
        foreach (var achievement in achievements)
        {
            string key = $"Achievement_{achievement.name}_IsUnlocked";
            achievement.isUnlocked = PlayerPrefs.GetInt(key, 0) == 1;
            Debug.Log($"Состояние достижения '{achievement.name}' загружено: {achievement.isUnlocked}");
        }
    }
}