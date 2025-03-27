using System;
using UnityEngine;
using UnityEngine.UI;

public class Clicker : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Button clickButton;
    [SerializeField] private Text multiplierText;
    [SerializeField] private Text[] additionalScoreTexts = new Text[0];

    [Header("Game Settings")]
    [SerializeField] private float clicksPerSecondLimit = 10f;
    [SerializeField, Range(0f, 1f)] private float bossSpawnChance = 0.05f; // Шанс появления босса

    [Header("Dependencies")]
    [SerializeField] private MessageManager messageManager;

    private static int clickCount = 0;
    private static float globalMultiplier = 1f;
    private static Clicker instance;

    private float lastClickTime;
    private int clicksThisSecond;

    private const string CLICK_COUNT_KEY = "ClickCount";
    private const string MULTIPLIER_KEY = "GlobalMultiplier";

    public static event Action<int> OnClickAdded;
    public static Clicker Instance => instance;

    #region Properties and Static Methods
    public static int GetClickCount() => clickCount;
    public static float GetGlobalMultiplier() => globalMultiplier;

    public static void SetClickCount(int value)
    {
        clickCount = Mathf.Max(0, value);
        SaveData(CLICK_COUNT_KEY, clickCount);
        instance?.UpdateAllScoreTexts();
    }

    public static void AddClicks(int amount)
    {
        int adjustedAmount = Mathf.RoundToInt(amount * globalMultiplier);
        clickCount += adjustedAmount;
        SaveData(CLICK_COUNT_KEY, clickCount);
        OnClickAdded?.Invoke(adjustedAmount);
        instance?.UpdateAllScoreTexts();
    }

    public static void RemoveClicks(int amount)
    {
        int adjustedAmount = Mathf.RoundToInt(amount * globalMultiplier);
        clickCount = Mathf.Max(0, clickCount - adjustedAmount);
        SaveData(CLICK_COUNT_KEY, clickCount);
        instance?.UpdateAllScoreTexts();
    }

    public static void SetGlobalMultiplier(float multiplier)
    {
        globalMultiplier = Mathf.Max(1f, multiplier);
        SaveData(MULTIPLIER_KEY, globalMultiplier);
        instance?.UpdateAllScoreTexts();
    }

    public static void MultiplyGlobalMultiplier(float multiplier)
    {
        globalMultiplier *= multiplier;
        SaveData(MULTIPLIER_KEY, globalMultiplier);
        instance?.UpdateAllScoreTexts();
    }

    public static void ResetProgress()
    {
        clickCount = 0;
        globalMultiplier = 1f;
        SaveData(CLICK_COUNT_KEY, clickCount);
        SaveData(MULTIPLIER_KEY, globalMultiplier);
        instance?.UpdateAllScoreTexts();
        Debug.Log("Прогресс сброшен.");
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        SetupSingleton();
        LoadData();
    }

    private void Start()
    {
        if (!ValidateComponents()) return;
        clickButton.onClick.AddListener(OnButtonClick);
        UpdateAllScoreTexts();
    }
    #endregion

    #region Initialization
    private void SetupSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void LoadData()
    {
        clickCount = SecurePlayerPrefs.GetInt(CLICK_COUNT_KEY, 0);
        globalMultiplier = SecurePlayerPrefs.GetFloat(MULTIPLIER_KEY, 1f);
    }

    private bool ValidateComponents()
    {
        if (clickButton == null)
        {
            Debug.LogError("ClickButton не назначен!");
            enabled = false;
            return false;
        }
        if (scoreText == null)
        {
            Debug.LogError("ScoreText не назначен!");
            enabled = false;
            return false;
        }
        if (multiplierText == null)
        {
            Debug.LogError("MultiplierText не назначен!");
            enabled = false;
            return false;
        }
        if (messageManager == null && !TryGetComponent(out messageManager))
        {
            messageManager = FindFirstObjectByType<MessageManager>(FindObjectsInactive.Include);
            if (messageManager == null)
            {
                Debug.LogError("MessageManager не найден или не назначен!");
                enabled = false;
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Click Handling
    public void OnButtonClick()
    {
        if (!CanClick()) return;

        AddClicks(1);
        UpdateClickRate();
        TrySpawnBoss(); // Проверяем шанс появления босса
    }

    private bool CanClick()
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick < 1f)
        {
            clicksThisSecond++;
            if (clicksThisSecond > clicksPerSecondLimit)
            {
                messageManager.ShowNotification("Превышен лимит кликов в секунду!");
                return false;
            }
        }
        else
        {
            clicksThisSecond = 1;
        }
        return true;
    }

    private void UpdateClickRate()
    {
        lastClickTime = Time.time;
    }

    private void TrySpawnBoss()
    {
        if (UnityEngine.Random.value <= bossSpawnChance)
        {
            BossController bossController = FindFirstObjectByType<BossController>();
            if (bossController != null)
            {
                bossController.StartBossFight();
            }
            else
            {
                Debug.LogError("BossController не найден!");
            }
        }
    }
    #endregion

    #region UI Updates
    public void UpdateAllScoreTexts()
    {
        UpdateText(scoreText, $" {clickCount}");
        foreach (var text in additionalScoreTexts)
            UpdateText(text, $" {clickCount}");
        UpdateText(multiplierText, $" x{globalMultiplier:F1}");
    }

    private void UpdateText(Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
    #endregion

    #region Data Management
    private static void SaveData(string key, int value) => SecurePlayerPrefs.SetInt(key, value);
    private static void SaveData(string key, float value) => SecurePlayerPrefs.SetFloat(key, value);
    #endregion
}