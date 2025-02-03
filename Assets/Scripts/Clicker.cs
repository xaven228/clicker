using UnityEngine;
using UnityEngine.UI;
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

	// Изменённый метод: теперь множитель умножается
	public static void MultiplyGlobalMultiplier(float multiplier)
	{
		globalMultiplier *= multiplier;
		SaveData(MULTIPLIER_KEY, globalMultiplier);
	}

	private static void SaveData(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
		PlayerPrefs.Save();
	}

	private static void SaveData(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
		PlayerPrefs.Save();
	}

	public void ShowNotification(string message)
	{
		Debug.Log(message);
	}

	void Start()
	{
		_clickCount = PlayerPrefs.GetInt(CLICK_COUNT_KEY, 0);
		globalMultiplier = PlayerPrefs.GetFloat(MULTIPLIER_KEY, 1f);
		UpdateAllScoreTexts();

		if (clickButton != null)
		{
			clickButton.onClick.AddListener(OnButtonClick);
		}
		else
		{
			Debug.LogError("Кнопка для кликов не назначена!");
		}
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
	}

	void Awake()
	{
		LoadClickCount(); // Загружаем количество кликов сразу при старте
		
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
	_clickCount = PlayerPrefs.GetInt("ClickCount", 0);
}

	public static void ResetProgress()
	{
		_clickCount = 0;
		globalMultiplier = 1f;
		SaveData(CLICK_COUNT_KEY, _clickCount);
		PlayerPrefs.DeleteKey(MULTIPLIER_KEY);
		PlayerPrefs.Save();
		Debug.Log("Прогресс сброшен.");
	}
}
