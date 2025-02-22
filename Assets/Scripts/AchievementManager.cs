using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Achievement
{
    [SerializeField] private string achievementName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private int maxProgress;
    [SerializeField] private Text achievementText;
    [SerializeField] private Slider progressBar;

    public string AchievementName => achievementName;
    public string Description => description;
    public int MaxProgress => maxProgress;
    public int CurrentProgress { get; set; }
    public bool IsUnlocked { get; set; }
    public Text AchievementText => achievementText;
    public Slider ProgressBar => progressBar;
}

public class AchievementManager : MonoBehaviour
{
    [Header("Achievements")]
    [SerializeField] private Achievement[] achievements = new Achievement[0];

    [Header("Dependencies")]
    [SerializeField] private Clicker clicker;
    [SerializeField] private MessageManager messageManager;

    public Achievement[] Achievements => achievements; // Добавлено публичное свойство

    private const string UNLOCKED_KEY_PREFIX = "AchievementUnlocked_";
    private const string PROGRESS_KEY_PREFIX = "AchievementProgress_";

    #region Unity Methods
    private void Awake()
    {
        LoadAchievements();
    }

    private void Start()
    {
        ValidateComponents();
        CheckAchievements(0);
        UpdateAchievementUI();
    }

    private void OnEnable()
    {
        Clicker.OnClickAdded += CheckAchievements;
    }

    private void OnDisable()
    {
        Clicker.OnClickAdded -= CheckAchievements;
    }

    private void OnApplicationQuit()
    {
        SaveAllAchievementsProgress();
    }
    #endregion
    
    #region Initialization
    private void ValidateComponents()
    {
        if (clicker == null && !TryGetComponent(out clicker))
        {
            clicker = FindFirstObjectByType<Clicker>();
            if (clicker == null) Debug.LogWarning("Clicker не назначен и не найден на сцене!");
        }
        if (messageManager == null && !TryGetComponent(out messageManager))
        {
            messageManager = FindFirstObjectByType<MessageManager>();
            if (messageManager == null) Debug.LogWarning("MessageManager не назначен и не найден на сцене!");
        }
    }

    private void LoadAchievements()
    {
        foreach (var achievement in achievements)
        {
            achievement.IsUnlocked = SecurePlayerPrefs.GetInt($"{UNLOCKED_KEY_PREFIX}{achievement.AchievementName}", 0) == 1;
            achievement.CurrentProgress = SecurePlayerPrefs.GetInt($"{PROGRESS_KEY_PREFIX}{achievement.AchievementName}", 0);
        }
    }
    #endregion

    #region Achievement Logic
    public void CheckAchievements(int addedClicks)
    {
        if (clicker == null) return;

        int totalClicks = Clicker.GetClickCount();
        bool uiNeedsUpdate = false;

        foreach (var achievement in achievements)
        {
            if (!achievement.IsUnlocked)
            {
                UpdateProgress(achievement, totalClicks);
                if (achievement.CurrentProgress >= achievement.MaxProgress)
                {
                    UnlockAchievement(achievement);
                    uiNeedsUpdate = true;
                }
            }
        }

        if (uiNeedsUpdate || addedClicks > 0)
            UpdateAchievementUI();
    }

    private void UpdateProgress(Achievement achievement, int totalClicks)
    {
        achievement.CurrentProgress = Mathf.Min(totalClicks, achievement.MaxProgress);
        SaveAchievementProgress(achievement);
    }

    private void UnlockAchievement(Achievement achievement)
    {
        achievement.IsUnlocked = true;
        SecurePlayerPrefs.SetInt($"{UNLOCKED_KEY_PREFIX}{achievement.AchievementName}", 1);
        SaveAchievementProgress(achievement);

        messageManager?.ShowMessage($"Достижение выполнено: {achievement.AchievementName}");
        Debug.Log($"Достижение '{achievement.AchievementName}' разблокировано!");
    }
    #endregion

    #region UI Management
    public void UpdateAchievementUI()
    {
        foreach (var achievement in achievements)
        {
            UpdateText(achievement);
            UpdateProgressBar(achievement);
        }
    }

    private void UpdateText(Achievement achievement)
    {
        if (achievement.AchievementText != null)
        {
            achievement.AchievementText.text = achievement.IsUnlocked
                ? "Выполнено!"
                : $"{achievement.CurrentProgress}/{achievement.MaxProgress}";
        }
    }

    private void UpdateProgressBar(Achievement achievement)
    {
        if (achievement.ProgressBar != null)
        {
            achievement.ProgressBar.maxValue = achievement.MaxProgress;
            achievement.ProgressBar.value = achievement.CurrentProgress;
            achievement.ProgressBar.interactable = false;
        }
    }
    #endregion

    #region Data Management
    private void SaveAllAchievementsProgress()
    {
        foreach (var achievement in achievements)
            SaveAchievementProgress(achievement);
    }

    private void SaveAchievementProgress(Achievement achievement)
    {
        SecurePlayerPrefs.SetInt($"{PROGRESS_KEY_PREFIX}{achievement.AchievementName}", achievement.CurrentProgress);
    }
    #endregion
}