using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Achievement
{
    public string name; // Название достижения
    public int target; // Цель (например, количество кликов)
    public Slider progressBar; // Шкала прогресса
    public Text progressText; // Текстовое поле для отображения прогресса
    [HideInInspector] public bool isUnlocked; // Флаг выполнения
}

public class AchievementManager : MonoBehaviour
{
    // Массив достижений
    public Achievement[] achievements;

    // Ссылка на Clicker для получения текущего количества кликов
    public Clicker clicker;

    // Ссылка на MessageManager для вывода сообщений
    public MessageManager messageManager;

    // Префикс для ключей PlayerPrefs
    private const string UNLOCKED_KEY_PREFIX = "AchievementUnlocked_";

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

        // Обновляем интерфейс для всех достижений
        UpdateAchievementUI();
    }

    // Метод для проверки прогресса достижений
    public void CheckAchievements()
    {
        int totalClicks = Clicker.GetClickCount(); // Получаем общее количество кликов за всё время

        foreach (var achievement in achievements)
        {
            // Если достижение ещё не разблокировано
            if (!achievement.isUnlocked)
            {
                // Проверяем, достигнута ли цель
                if (totalClicks >= achievement.target)
                {
                    UnlockAchievement(achievement);
                }
            }
        }

        // Обновляем интерфейс
        UpdateAchievementUI();
    }

    // Метод для разблокировки достижения
    private void UnlockAchievement(Achievement achievement)
    {
        achievement.isUnlocked = true;

        // Сохраняем состояние достижения
        SaveAchievementProgress(achievement);

        // Выводим уведомление о выполнении достижения
        if (messageManager != null)
        {
            messageManager.ShowMessage($"Достижение выполнено: {achievement.name}");
        }

        Debug.Log($"Достижение '{achievement.name}' разблокировано!");
    }

    // Метод для сохранения состояния достижения
    private void SaveAchievementProgress(Achievement achievement)
    {
        string unlockedKey = UNLOCKED_KEY_PREFIX + achievement.name;

        PlayerPrefs.SetInt(unlockedKey, achievement.isUnlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Метод для загрузки состояния достижений
    private void LoadAchievements()
    {
        foreach (var achievement in achievements)
        {
            string unlockedKey = UNLOCKED_KEY_PREFIX + achievement.name;

            achievement.isUnlocked = PlayerPrefs.GetInt(unlockedKey, 0) == 1;

            Debug.Log($"Загружено достижение: {achievement.name}, выполнено: {achievement.isUnlocked}");
        }
    }

    // Метод для обновления интерфейса достижений
    private void UpdateAchievementUI()
    {
        int totalClicks = Clicker.GetClickCount(); // Получаем общее количество кликов за всё время

        foreach (var achievement in achievements)
        {
            if (achievement.progressBar != null && achievement.progressText != null)
            {
                // Делаем Slider неинтерактивным
                achievement.progressBar.interactable = false;

                // Обновляем шкалу прогресса
                achievement.progressBar.maxValue = achievement.target;
                achievement.progressBar.value = Mathf.Min(totalClicks, achievement.target);

                // Обновляем текстовое поле
                string status = achievement.isUnlocked ? "Выполнено" : "Не выполнено";
                achievement.progressText.text = $"{achievement.name} - {status}\n{Mathf.Min(totalClicks, achievement.target)}/{achievement.target}";
            }
        }
    }
}