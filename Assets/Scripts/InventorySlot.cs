/*
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
	// Ссылка на заблокированную иконку
	[Header("UI Elements")]
	[SerializeField] private Image lockedIcon;

	// Ссылка на кнопку разблокировки
	[SerializeField] private Button unlockButton;

	// Спрайт фона
	private Sprite backgroundSprite;

	// Название фона
	private string backgroundName;

	// Инициализация слота с фоном
	public void Initialize(string name, Sprite sprite)
	{
		// Запись в лог для дебага
		Debug.Log($"Инициализация слота для фона: {name}");

		backgroundName = name;
		backgroundSprite = sprite;

		SetupLockedIcon();
		SetupUnlockButton();
	}

	// Настроить заблокированную иконку
	private void SetupLockedIcon()
	{
		if (lockedIcon != null)
		{
			lockedIcon.enabled = true;
			lockedIcon.sprite = backgroundSprite; // Устанавливаем спрайт фона
		}
	}

	// Настроить кнопку разблокировки
	private void SetupUnlockButton()
	{
		if (unlockButton != null)
		{
			unlockButton.gameObject.SetActive(false); // Изначально кнопка скрыта
			unlockButton.onClick.RemoveAllListeners(); // Очистить старые слушатели
			unlockButton.onClick.AddListener(Unlock); // Добавить слушателя для разблокировки
		}
	}

	// Разблокировать слот
	private void Unlock()
	{
		if (lockedIcon != null)
		{
			lockedIcon.enabled = false; // Отключаем заблокированную иконку
		}

		if (unlockButton != null)
		{
			unlockButton.gameObject.SetActive(false); // Скрываем кнопку разблокировки
		}

		// Поменять фон в CaseManager
		SetBackground();
	}

	// Изменить фон
	private void SetBackground()
	{
		if (backgroundSprite != null)
		{
			// Используем новый метод для поиска CaseManager
			CaseManager caseManager = Object.FindFirstObjectByType<CaseManager>(); 

			if (caseManager != null)
			{
				BackgroundItem backgroundItem = new BackgroundItem
				{
					backgroundSprite = backgroundSprite,
					name = backgroundName // Устанавливаем имя
				};

				caseManager.SetBackground(backgroundItem);
			}
			else
			{
				Debug.LogError("CaseManager не найден в сцене!");
			}
		}
	}
}
*/