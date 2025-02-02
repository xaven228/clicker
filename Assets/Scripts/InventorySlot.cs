/*
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
	// Ссылка на заблокированную иконку
	public Image lockedIcon;

	// Ссылка на кнопку разблокировки
	public Button unlockButton;

	// Спрайт фона
	private Sprite backgroundSprite;

	// Название фона
	private string backgroundName;

	// Инициализация слота для фона
	public void Initialize(string name, Sprite sprite)
	{
		Debug.Log($"Инициализация слота для фона: {name}");

		backgroundName = name;
		backgroundSprite = sprite;

		// Настраиваем заблокированную иконку
		if (lockedIcon != null)
		{
			lockedIcon.enabled = true;
			lockedIcon.sprite = sprite; // Устанавливаем спрайт фона
		}

		// Отключаем кнопку разблокировки
		if (unlockButton != null)
		{
			unlockButton.gameObject.SetActive(false);
		}
	}

	// Метод разблокировки слота
	public void Unlock()
	{
		// Отключаем заблокированную иконку
		if (lockedIcon != null)
		{
			lockedIcon.enabled = false;
		}

		// Включаем кнопку разблокировки
		if (unlockButton != null)
		{
			unlockButton.gameObject.SetActive(true);
			unlockButton.onClick.AddListener(() => ChangeBackground());
		}
	}

	// Метод смены фона
	private void ChangeBackground()
	{
		// Находим CaseManager и меняем фон
		CaseManager caseManager = Object.FindFirstObjectByType<CaseManager>();
		if (caseManager != null && backgroundSprite != null)
		{
			CaseItem caseItem = new CaseItem
			{
				name = backgroundName,
				itemSprite = backgroundSprite
			};
			caseManager.SetBackground(caseItem);  // Устанавливаем фон
		}
	}
}
*/