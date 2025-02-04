using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Clicker : MonoBehaviour
{
    public Text scoreText;
    public Text[] additionalScoreTexts;
    public Button clickButton;

    private static int _clickCount = 0;
    public static Clicker Instance;

    private static float globalMultiplier = 1f;

    public static event System.Action<int> OnClickAdded;

    private const string CLICK_COUNT_KEY = "ClickCount";
    private const string MULTIPLIER_KEY = "GlobalMultiplier";

    public float clicksPerSecondLimit = 10f;
    private float lastClickTime = 0f;
    private int clicksThisSecond = 0;

    private static List<string> activatedItems = new List<string>();

    // Новые переменные для босса
    public GameObject bossPanel; // Панель с боссом
    public Slider bossHealthBar; // HP бар босса
    public Text bossTimerText; // Таймер на убийство босса
    public int bossMaxHealth = 100; // Максимальное HP босса
    private int bossCurrentHealth; // Текущее HP босса
    public float bossTimeLimit = 30f; // Время на убийство босса
    private float bossTimeRemaining; // Оставшееся время
    private bool isBossFightActive = false; // Флаг активности боя с боссом
    public int bossReward = 100; // Награда за победу над боссом
    public int bossPenalty = 50; // Штраф за проигрыш

    public static int GetClickCount()
    {
        return _clickCount;
    }

    public static void SetClickCount(int value)
    {
        _clickCount = value;
        SaveData(CLICK_COUNT_KEY, _clickCount);
    }

    public static void AddClicks(int amount)
    {
        _clickCount += Mathf.RoundToInt(amount * globalMultiplier);
        SaveData(CLICK_COUNT_KEY, _clickCount);
        OnClickAdded?.Invoke(amount);
    }

    public static void RemoveClicks(int amount)
    {
        _clickCount = Mathf.Max(0, _clickCount - amount);
        SaveData(CLICK_COUNT_KEY, _clickCount);
    }

    public static void SetGlobalMultiplier(float multiplier)
    {
        globalMultiplier = multiplier;
        SaveData(MULTIPLIER_KEY, globalMultiplier);
    }

    public static float GetGlobalMultiplier()
    {
        return globalMultiplier;
    }

    public static void MultiplyGlobalMultiplier(float multiplier)
    {
        globalMultiplier *= multiplier;
        SaveData(MULTIPLIER_KEY, globalMultiplier);
    }

    private static void SaveData(string key, int value)
    {
        SecurePlayerPrefs.SetInt(key, value);
    }

    private static void SaveData(string key, float value)
    {
        SecurePlayerPrefs.SetFloat(key, value);
    }

    public void ShowNotification(string message)
    {
        Debug.Log(message);
    }

    void Start()
    {
        _clickCount = SecurePlayerPrefs.GetInt(CLICK_COUNT_KEY, 0);
        globalMultiplier = SecurePlayerPrefs.GetFloat(MULTIPLIER_KEY, 1f);
        UpdateAllScoreTexts();

        if (clickButton != null)
        {
            clickButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Кнопка для кликов не назначена!");
        }

        // Инициализация босса
        bossCurrentHealth = bossMaxHealth;
        bossHealthBar.maxValue = bossMaxHealth;
        bossHealthBar.value = bossCurrentHealth;
        bossPanel.SetActive(false); // Скрываем панель босса при старте
    }

    public void OnButtonClick()
    {
        if (Time.time - lastClickTime < 1f)
        {
            clicksThisSecond++;
        }
        else
        {
            clicksThisSecond = 1;
        }

        if (clicksThisSecond > clicksPerSecondLimit)
        {
            Debug.LogWarning("Лимит кликов в секунду превышен. Клик проигнорирован.");
            return;
        }

        AddClicks(1);
        UpdateAllScoreTexts();
        lastClickTime = Time.time;

        // Если бой с боссом активен, наносим урон боссу
        if (isBossFightActive)
        {
            DamageBoss(1); // Наносим 1 урон за клик
        }
    }

    void Awake()
    {
        LoadClickCount();

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void UpdateAllScoreTexts()
    {
        if (scoreText != null)
        {
            scoreText.text = "Клики: " + _clickCount;
        }

        foreach (var text in additionalScoreTexts)
        {
            if (text != null)
            {
                text.text = "Клики: " + _clickCount;
            }
        }
    }

    private void LoadClickCount()
    {
        _clickCount = SecurePlayerPrefs.GetInt("ClickCount", 0);
    }

    public static void ResetProgress()
    {
        _clickCount = 0;
        globalMultiplier = 1f;
        SaveData(CLICK_COUNT_KEY, _clickCount);
        SecurePlayerPrefs.SetFloat(MULTIPLIER_KEY, globalMultiplier);
        PlayerPrefs.Save();
        Debug.Log("Прогресс сброшен.");
    }

    // Метод для начала боя с боссом
    public void StartBossFight()
    {
        if (!isBossFightActive)
        {
            isBossFightActive = true;
            bossCurrentHealth = bossMaxHealth;
            bossTimeRemaining = bossTimeLimit;
            bossPanel.SetActive(true);
            StartCoroutine(BossFightTimer());
        }
    }

    // Корутина для таймера боя с боссом
    private IEnumerator BossFightTimer()
    {
        while (bossTimeRemaining > 0 && isBossFightActive)
        {
            bossTimeRemaining -= Time.deltaTime;
            bossTimerText.text = "Осталось времени: " + Mathf.RoundToInt(bossTimeRemaining).ToString() + " сек.";
            yield return null;
        }

        if (isBossFightActive)
        {
            EndBossFight(false); // Время вышло, игрок проиграл
        }
    }

    // Метод для нанесения урона боссу
    public void DamageBoss(int damage)
    {
        if (isBossFightActive)
        {
            bossCurrentHealth -= damage;
            bossHealthBar.value = bossCurrentHealth;

            if (bossCurrentHealth <= 0)
            {
                EndBossFight(true); // Босс побежден
            }
        }
    }

    // Метод для завершения боя с боссом
    private void EndBossFight(bool isVictory)
    {
        isBossFightActive = false;
        bossPanel.SetActive(false);

        if (isVictory)
        {
            AddClicks(bossReward);
            ShowNotification("Победа! Вы получили " + bossReward + " кликов!");
        }
        else
        {
            RemoveClicks(bossPenalty);
            ShowNotification("Поражение! Вы потеряли " + bossPenalty + " кликов!");
        }
    }
}