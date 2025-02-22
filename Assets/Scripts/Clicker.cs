using System;
using System.Collections;
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
    [SerializeField, Range(0f, 1f)] private float bossSpawnChance = 0.05f;
    [SerializeField] private int bossMaxHealth = 100;
    [SerializeField] private float bossTimeLimit = 30f;
    [SerializeField] private int bossReward = 100;

    [Header("Dependencies")]
    [SerializeField] private MessageManager messageManager;

    private static int clickCount = 0;
    private static float globalMultiplier = 1f;
    private static Clicker instance;

    private float lastClickTime;
    private int clicksThisSecond;
    private bool isBossFightActive;
    private int bossCurrentHealth;
    private float bossTimeRemaining;

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
			if (clickButton == null) { Debug.LogError("ClickButton не назначен!"); enabled = false; return false; }
			if (scoreText == null) { Debug.LogError("ScoreText не назначен!"); enabled = false; return false; }
			if (multiplierText == null) { Debug.LogError("MultiplierText не назначен!"); enabled = false; return false; }
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
        TrySpawnBoss();
    }

    private bool CanClick()
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick < 1f)
        {
            clicksThisSecond++;
            if (clicksThisSecond > clicksPerSecondLimit)
            {
                Debug.LogWarning("Превышен лимит кликов в секунду!");
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
        if (!isBossFightActive && UnityEngine.Random.value <= bossSpawnChance)
        {
            StartBossFight();
        }
    }
    #endregion

    #region Boss Fight
    public void StartBossFight()
    {
        if (isBossFightActive) return;

        isBossFightActive = true;
        bossCurrentHealth = bossMaxHealth;
        bossTimeRemaining = bossTimeLimit;
        StartCoroutine(BossFightTimer());
        messageManager.ShowMessage("Босс появился! У вас есть 30 секунд!");
    }

    private IEnumerator BossFightTimer()
    {
        while (bossTimeRemaining > 0 && isBossFightActive)
        {
            bossTimeRemaining -= Time.deltaTime;
            yield return null;
        }
        if (isBossFightActive)
            EndBossFight(false);
    }

    public void OnBossButtonClick()
    {
        if (isBossFightActive)
            DamageBoss(1);
    }

    private void DamageBoss(int damage)
    {
        bossCurrentHealth -= damage;
        if (bossCurrentHealth <= 0)
            EndBossFight(true);
    }

    private void EndBossFight(bool victory)
    {
        isBossFightActive = false;
        if (victory)
        {
            AddClicks(bossReward);
            messageManager.ShowWinMessage($"Победа! Вы получили {bossReward} кликов!");
        }
        else
        {
            messageManager.ShowLoseMessage("Время вышло! Босс сбежал.");
        }
    }
    #endregion

    #region UI Updates
    public void UpdateAllScoreTexts()
    {
        UpdateText(scoreText, $"Клики: {clickCount}");
        foreach (var text in additionalScoreTexts)
            UpdateText(text, $"Клики: {clickCount}");
        UpdateText(multiplierText, $"Множитель: x{globalMultiplier:F1}");
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