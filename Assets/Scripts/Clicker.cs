using UnityEngine;
using UnityEngine.UI;

public class Clicker : MonoBehaviour
{
    // Ссылка на текстовое поле для отображения счётчика
    public Text scoreText;

    // Ссылка на кнопку, по которой будут работать клики
    public Button clickButton;

    // Статическая переменная для хранения количества кликов
    private static int clickCount = 0;

    // Ключ для PlayerPrefs
    private const string CLICK_COUNT_KEY = "ClickCount";

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Загружаем сохранённое значение кликов из PlayerPrefs
        clickCount = PlayerPrefs.GetInt(CLICK_COUNT_KEY, 0);
        UpdateScoreText();

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
        clickCount++; // Увеличиваем счётчик
        UpdateScoreText(); // Обновляем текстовое поле

        // Сохраняем новое значение кликов в PlayerPrefs
        PlayerPrefs.SetInt(CLICK_COUNT_KEY, clickCount);
        PlayerPrefs.Save(); // Убедимся, что данные записаны
    }

    // Метод для обновления текстового поля
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Клики: " + clickCount;
        }
    }

    // Геттер для получения количества кликов из других скриптов
    public static int GetClickCount()
    {
        return clickCount;
    }
}