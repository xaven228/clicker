using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

	// Статическая переменная для доступа к экземпляру Clicker
	public static Clicker Instance;

	// Глобальный множитель кликов
	private static float globalMultiplier = 1f;

	// Событие для добавления кликов
	public static event System.Action<int> OnClickAdded;

	// Ключи для PlayerPrefs
	private const string CLICK_COUNT_KEY = "ClickCount";
	private const string MULTIPLIER_KEY = "GlobalMultiplier";

	// Публичное поле для лимита кликов в секунду
	public float clicksPerSecondLimit = 10f; // Лимит кликов в секунду, настраиваемый в инспекторе
	private float lastClickTime = 0f; // Время последнего клика
	private int clicksThisSecond = 0; // Количество кликов за текущую секунду

	// Список для хранения активных предметов
	private static List<string> activatedItems = new List<string>(); // Массив активированных предметов (фонов)

	// Публичный геттер для получения количества кликов
	public static int GetClickCount()
	{
		return _clickCount;
	}

	// Публичный метод для установки количества кликов
	public static void SetClickCount(int value)
	{
		_clickCount = value;
		SaveData(CLICK_COUNT_KEY, _clickCount);
	}

	// Метод для увеличения количества кликов
	public static void AddClicks(int amount)
	{
		_clickCount += Mathf.RoundToInt(amount * globalMultiplier); // Учитываем множитель
		SaveData(CLICK_COUNT_KEY, _clickCount);

		// Триггерим событие после добавления кликов
		OnClickAdded?.Invoke(amount);  // Вызов события с количеством добавленных кликов
	}

	// Новый метод для уменьшения количества кликов
	public static void RemoveClicks(int amount)
	{
		_clickCount = Mathf.Max(0, _clickCount - amount); // Уменьшаем клики, не позволяя уйти в минус
		SaveData(CLICK_COUNT_KEY, _clickCount);
	}

	// Метод для установки глобального множителя
	public static void SetGlobalMultiplier(float multiplier)
	{
		globalMultiplier = multiplier;
		SaveData(MULTIPLIER_KEY, globalMultiplier);
	}

	// Метод для получения глобального множителя
	public static float GetGlobalMultiplier()
	{
		return globalMultiplier;
	}

	// Метод для увеличения глобального множителя
	public static void IncreaseGlobalMultiplier(float amount)
	{
		globalMultiplier += amount;
		SaveData(MULTIPLIER_KEY, globalMultiplier);
	}

	// Метод для сохранения данных в PlayerPrefs (для int значений)
	private static void SaveData(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
		PlayerPrefs.Save();
	}

	// Метод для сохранения данных в PlayerPrefs (для float значений)
	private static void SaveData(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
		PlayerPrefs.Save();
	}

	// Метод для отображения уведомлений
	public void ShowNotification(string message)
	{
		// Тут можно реализовать логику для UI уведомлений.
		// Например, показать текстовое уведомление на экране.

		Debug.Log(message); // Временно выводим сообщение в консоль
		// Можно добавить компонент для UI, например Text или Image, чтобы выводить это сообщение.
	}

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

	// Метод, вызываемый при нажатии на кнопку
	public void OnButtonClick()
	{
		// Проверяем, не превышен ли лимит кликов в секунду
		if (Time.time - lastClickTime < 1f)
		{
			clicksThisSecond++;
		}
		else
		{
			clicksThisSecond = 1; // Начинаем отсчёт кликов с нового периода
		}

		if (clicksThisSecond > clicksPerSecondLimit)
		{
			// Если лимит превышен, игнорируем клик и выводим сообщение
			Debug.LogWarning("Лимит кликов в секунду превышен. Клик проигнорирован.");
			return;
		}

		AddClicks(1); // Увеличиваем счётчик с учётом множителя
		UpdateAllScoreTexts(); // Обновляем все текстовые поля

		lastClickTime = Time.time; // Обновляем время последнего клика
	}

	void Awake()
	{
		// Убедимся, что Instance всегда ссылается на текущий объект Clicker
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy(gameObject);  // Уничтожаем лишний экземпляр
		}
	}

	// Метод для обновления всех текстовых полей
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
		SaveData(CLICK_COUNT_KEY, _clickCount);
		PlayerPrefs.DeleteKey(MULTIPLIER_KEY);
		PlayerPrefs.Save();
		Debug.Log("Прогресс сброшен.");
	}




	// Метод, вызываемый каждый кадр
	void Update()
	{
		// Нет необходимости делать дополнительные проверки на аномалии здесь,
		// поскольку проверка кликов в секунду уже происходит при каждом нажатии на кнопку.
	}
}
