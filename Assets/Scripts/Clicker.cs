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
    // Статическая переменная для хранения количества кликов
    private static int _clickCount = 0;
    // Глобальный множитель кликов
    private static float globalMultiplier = 1f;
    // Ключи для PlayerPrefs
    private const string CLICK_COUNT_KEY = "ClickCount";
    private const string MULTIPLIER_KEY = "GlobalMultiplier";

    // Публичный геттер для получения количества кликов
    public static int GetClickCount()
    {
        return _clickCount;
    }

    // Публичный метод для установки количества кликов
    public static void SetClickCount(int value)
    {
        _clickCount = value;
        PlayerPrefs.SetInt(CLICK_COUNT_KEY, _clickCount);
        PlayerPrefs.Save();
    }

    // Публичный метод для увеличения количества кликов
    public static void AddClicks(int amount)
    {
        _clickCount += Mathf.RoundToInt(amount * globalMultiplier); // Учитываем множитель
        PlayerPrefs.SetInt(CLICK_COUNT_KEY, _clickCount);
        PlayerPrefs.Save();
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

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Загружаем сохранённое значение кликов из PlayerPrefs
        _clickCount = PlayerPrefs.GetInt(CLICK_COUNT_KEY, 0);
        // Загружаем сохранённый множитель из PlayerPrefs
        globalMultiplier = PlayerPrefs.GetFloat(MULTIPLIER_KEY, 1f);
        UpdateAllScoreTexts(); // Обновляем все текстовые поля

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

    // Метод, вызываемый при нажатии на кнопку
    public void OnButtonClick()
    {
        AddClicks(1); // Увеличиваем счётчик с учётом множителя
        UpdateAllScoreTexts(); // Обновляем все текстовые поля
    }

    // Публичный метод для обновления всех текстовых полей
    public void UpdateAllScoreTexts()
    {
        // Обновляем основное текстовое поле
        if (scoreText != null)
        {
            scoreText.text = "Клики: " + _clickCount;
        }
        // Обновляем дополнительные текстовые поля
        foreach (var text in additionalScoreTexts)
        {
            if (text != null)
            {
                text.text = "Клики: " + _clickCount;
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