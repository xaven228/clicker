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

    // Ключ для PlayerPrefs
    private const string CLICK_COUNT_KEY = "ClickCount";

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
        _clickCount += amount;
        PlayerPrefs.SetInt(CLICK_COUNT_KEY, _clickCount);
        PlayerPrefs.Save();
    }

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Загружаем сохранённое значение кликов из PlayerPrefs
        _clickCount = PlayerPrefs.GetInt(CLICK_COUNT_KEY, 0);
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
        AddClicks(1); // Увеличиваем счётчик
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
}