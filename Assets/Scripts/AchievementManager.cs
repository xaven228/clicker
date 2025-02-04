using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Achievement
{
    public string achievementName;
    public string description;
    public int maxProgress;
    public int currentProgress;
    public bool isUnlocked;

    public Text achievementText;
    public Slider progressBar; // Добавлен слайдер прогресса
}

public class AchievementManager : MonoBehaviour
{
    public Achievement[] achievements;
    public Clicker clicker;
    public MessageManager messageManager;

    private const string UNLOCKED_KEY_PREFIX = "AchievementUnlocked_";

    private void OnEnable()
    {
        Clicker.OnClickAdded += CheckAchievements;
    }

    private void OnDisable()
    {
        Clicker.OnClickAdded -= CheckAchievements;
    }

    private void Start()
    {
        LoadAchievements();
        CheckAchievements(0);
        CheckInitialAchievements();
    }

    public void CheckAchievements(int addedClicks)
    {
        int totalClicks = Clicker.GetClickCount();

        foreach (var achievement in achievements)
        {
            if (!achievement.isUnlocked)
            {
                if (totalClicks >= achievement.maxProgress && achievement.currentProgress < achievement.maxProgress)
                {
                    achievement.currentProgress = achievement.maxProgress;
                    UnlockAchievement(achievement);
                }
                else if (totalClicks < achievement.maxProgress)
                {
                    achievement.currentProgress = totalClicks;
                }
            }
        }

        UpdateAchievementUI();
    }

    private void CheckInitialAchievements()
    {
        foreach (var achievement in achievements)
        {
            if (achievement.currentProgress >= achievement.maxProgress && !achievement.isUnlocked)
            {
                UnlockAchievement(achievement);
            }
        }
        UpdateAchievementUI();
    }

    private void UnlockAchievement(Achievement achievement)
    {
        achievement.isUnlocked = true;
        PlayerPrefs.SetInt(UNLOCKED_KEY_PREFIX + achievement.achievementName, 1);
        PlayerPrefs.Save();

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
            achievement.currentProgress = PlayerPrefs.GetInt("AchievementProgress_" + achievement.achievementName, 0);
        }
    }

    public void UpdateAchievementUI()
    {
        foreach (var achievement in achievements)
        {
            if (achievement.achievementText != null)
            {
                if (achievement.isUnlocked)
                {
                    achievement.achievementText.text = $"Выполнено!";
                }
                else
                {
                    achievement.achievementText.text = $"{achievement.currentProgress}/{achievement.maxProgress}";
                }
            }

            if (achievement.progressBar != null)
            {
                achievement.progressBar.maxValue = achievement.maxProgress;
                achievement.progressBar.value = achievement.currentProgress;
                achievement.progressBar.interactable = false; // Блокируем слайдер от изменения вручную
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
            SaveAchievementProgress(achievement);
        }
    }
private void Awake()
{
    LoadAchievements();
    CheckAchievements(0); // Проверяем достижения сразу после загрузки
}


    public void SaveAchievementProgress(Achievement achievement)
    {
        PlayerPrefs.SetInt("AchievementProgress_" + achievement.achievementName, achievement.currentProgress);
        PlayerPrefs.Save();
    }
}
