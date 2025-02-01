using UnityEngine;
using UnityEngine.UI;

public class Clicker : MonoBehaviour
{
    // Ссылка на текстовое поле для отображения счётчика (основное)
    public Text scoreText;
    // Массив текстовых полей, куда будут копироваться данные счётчика
    public Text[] additionalScoreTexts;
    // Ссылка на кнопку, по которой будут работать клики
    public Button clickButton;
    // Ссылка на AchievementManager для проверки прогресса достижений
    public AchievementManager achievementManager;
    // Ссылка на BossManager для управления боссом
    public BossManager bossManager;
    // Шанс появления босса (в процентах)
    public float bossSpawnChance = 10f;
    // Переменные для ограничения частоты кликов
    private float lastClickTime = 0f;
    private const float minClickInterval = 0.1f; // Минимальный интервал между кликами (в секундах)

    // Статическая переменная для хранения количества кликов
    private static int _clickCount = 0;

    // Глобальный множитель кликов
    private static float globalMultiplier = 1f;

    // Ключи для PlayerPrefs
    private const string CLICK_COUNT_KEY = "ClickCount";
    private const string MULTIPLIER_KEY = "GlobalMultiplier";

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Загружаем сохранённое значение кликов из PlayerPrefs
        _clickCount = PlayerPrefs.GetInt(CLICK_COUNT_KEY, 0);

        // Загружаем сохранённый множитель из PlayerPrefs
        globalMultiplier = PlayerPrefs.GetFloat(MULTIPLIER_KEY, 1f);

        // Обновляем все текстовые поля
        UpdateAllScoreTexts();

        // Проверяем, что кнопка назначена
        if (clickButton != null)
        {
            // Назначаем обработчик события для кнопки
            clickButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Кнопка для кликов не назначена!");
        }
    }

    // Метод, вызываемый при нажатии на основную кнопку кликов
    public void OnButtonClick()
    {
        // Проверяем, прошло ли достаточно времени с момента последнего клика
        if (Time.time - lastClickTime < minClickInterval)
        {
            Debug.LogWarning("Слишком быстрое нажатие! Клик игнорируется.");
            return;
        }

        // Если босс не активен, проверяем шанс его появления
        if (bossManager != null && !bossManager.isBossActive && Random.Range(0f, 100f) <= bossSpawnChance)
        {
            MessageManager.Instance.ShowMessage("Босс появился!", Color.red);
            bossManager.StartBossFight();
        }
        else
        {
            AddClicks(1); // Увеличиваем счётчик с учётом множителя
        }

        // Проверяем прогресс достижений
        if (achievementManager != null)
        {
            achievementManager.CheckAchievements();
        }

        UpdateAllScoreTexts(); // Обновляем все текстовые поля
        lastClickTime = Time.time; // Обновляем время последнего клика
    }

    // Метод для увеличения количества кликов
    public static void AddClicks(int amount)
    {
        _clickCount += Mathf.RoundToInt(amount * globalMultiplier); // Учитываем множитель
        PlayerPrefs.SetInt(CLICK_COUNT_KEY, _clickCount); // Сохраняем значение в PlayerPrefs
    }

    // Метод для установки глобального множителя
    public static void SetGlobalMultiplier(float multiplier)
    {
        globalMultiplier = multiplier;
        PlayerPrefs.SetFloat(MULTIPLIER_KEY, globalMultiplier);
        PlayerPrefs.Save();
    }

    // Метод для увеличения глобального множителя
    public static void IncreaseGlobalMultiplier(float bonus)
    {
        globalMultiplier += bonus;
        PlayerPrefs.SetFloat(MULTIPLIER_KEY, globalMultiplier);
        PlayerPrefs.Save();
    }

    // Публичный метод для получения глобального множителя
    public static float GetGlobalMultiplier()
    {
        return globalMultiplier;
    }

    // Метод для обновления всех текстовых полей
    public void UpdateAllScoreTexts()
    {
        // Обновляем основное текстовое поле
        if (scoreText != null)
        {
            scoreText.text = $"Клики: {_clickCount}";
        }

        // Обновляем дополнительные текстовые поля
        foreach (var text in additionalScoreTexts)
        {
            if (text != null)
            {
                text.text = $"Клики: {_clickCount}";
            }
        }
    }

    // Метод для сброса прогресса (для тестирования)
    public static void ResetProgress()
    {
        _clickCount = 0;
        globalMultiplier = 1f;
        PlayerPrefs.DeleteKey(CLICK_COUNT_KEY);
        PlayerPrefs.DeleteKey(MULTIPLIER_KEY);
        PlayerPrefs.Save();
        Debug.Log("Прогресс сброшен.");
    }
}