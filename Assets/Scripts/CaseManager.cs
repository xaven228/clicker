using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RouletteManager : MonoBehaviour
{
	
	[Header("Настройки рулетки")]
	public List<CaseItem> caseItems;  // Список предметов с шансом появления
	public int totalItemsCount = 1000; // Количество экземпляров в рулетке
	public int clickCost = 10; // Стоимость запуска рулетки в кликах

	// Добавляем переменную Instance
	public static RouletteManager Instance { get; private set; }
	
	public RectTransform roulettePanel; // Панель рулетки
	public GameObject itemPrefab; // Префаб элемента рулетки
	public float itemSpacing = 100f; // Расстояние между элементами

	private List<GameObject> itemObjects = new List<GameObject>(); // Список объектов на панели
	private List<CaseItem> rouletteItems = new List<CaseItem>(); // Список предметов на рулетке
	private Clicker clicker; // Ссылка на скрипт Clicker

	[Header("UI")]
	public Image backgroundImage;  // Ссылка на компонент Image фона
	public Button startButton;  // Ссылка на кнопку запуска рулетки
	public GameObject roulettePanelUI;  // Ссылка на панель UI с рулеткой
	public Button closeButton;  // Кнопка закрытия панели рулетки

	// Добавляем ссылку на объект стрелки
	public GameObject arrowSprite;  // Стрелка, указывающая на центр
	public float arrowOffsetY = -50f; // Смещение стрелки по оси Y (для того, чтобы она была под панелью)

	// Ссылка на MessageManager для отображения сообщений
	public MessageManager messageManager;  // Ссылка на объект MessageManager

	// Метод для изменения фона
	public void ChangeBackground(Sprite newBackground)
	{
		if (backgroundImage != null && newBackground != null)
		{
			backgroundImage.sprite = newBackground;  // Меняем фон
		}
		else
		{
			Debug.LogWarning("Не удалось изменить фон. Новый фон не указан или Image не найден.");
		}
	}

	void Awake()
	{
		// Проверяем, если уже есть экземпляр, то удаляем этот объект
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);  // Удаляем лишний экземпляр
		}
		else
		{
			Instance = this;  // Устанавливаем этот экземпляр как главный
			DontDestroyOnLoad(gameObject);  // Не уничтожаем объект при смене сцен
		}
	}
	// Метод для загрузки фона при старте игры
private void LoadSelectedBackground()
{
    string selectedBackgroundName = PlayerPrefs.GetString("SelectedBackground", "");
    if (!string.IsNullOrEmpty(selectedBackgroundName))
    {
        // Ищем предмет по имени, если оно найдено, меняем фон
        CaseItem selectedItem = caseItems.Find(item => item.itemName == selectedBackgroundName);
        if (selectedItem != null && selectedItem.backgroundSprite != null)
        {
            backgroundImage.sprite = selectedItem.backgroundSprite;
        }
    }
}

void Start()
{
	// Загружаем фоновое изображение
	LoadSelectedBackground();

	clicker = Object.FindFirstObjectByType<Clicker>(); // Получаем ссылку на Clicker
	if (clicker == null)
	{
		Debug.LogError("Clicker не найден на сцене!");
	}
	
	// Загрузка состояния активации для всех предметов
	foreach (var item in rouletteItems)
	{
		item.LoadActivationState(); // Загружаем состояние активации каждого предмета
	}

	// Назначаем обработчик нажатия кнопки на старте
	if (startButton != null)
	{
		startButton.onClick.AddListener(StartRoulette);
	}
	else
	{
		Debug.LogError("Кнопка для запуска рулетки не назначена!");
	}

	// Подписываемся на событие изменения кликов
	Clicker.OnClickAdded += HandleClickAdded;

	// Изначально размещаем стрелку под панелью
	if (arrowSprite != null)
	{
		arrowSprite.transform.position = new Vector3(roulettePanel.position.x, roulettePanel.position.y + arrowOffsetY, roulettePanel.position.z);
	}

	// Скрываем панель рулетки и кнопку закрытия на старте
	roulettePanelUI.SetActive(false);
	closeButton.gameObject.SetActive(false);

	// Назначаем обработчик для кнопки закрытия
	closeButton.onClick.AddListener(CloseRoulettePanel);

	// Загружаем состояние кнопок из PlayerPrefs
	LoadUnlockButtonsState();

	// Назначаем обработчик для кнопок активации
	foreach (var item in caseItems)
	{
		if (item.unlockButton != null)
		{
			item.unlockButton.onClick.AddListener(() => item.ActivateItem());  // Привязка к кнопке
		}
	}
}



	// Обработчик события изменения кликов
	private void HandleClickAdded(int amount)
	{
		Debug.Log($"Клики изменены на {amount}. Текущее количество: {Clicker.GetClickCount()}");
	}

	// Метод для создания новой панели рулетки
	private void CreateNewRoulettePanel()
	{
		foreach (var itemObject in itemObjects)
		{
			Destroy(itemObject);
		}
		itemObjects.Clear();
		rouletteItems.Clear();

		for (int i = 0; i < totalItemsCount; i++)
		{
			CaseItem randomItem = GetRandomItemBasedOnChance();
			rouletteItems.Add(randomItem);
		}

		for (int i = 0; i < totalItemsCount; i++)
		{
			GameObject itemObj = Instantiate(itemPrefab, roulettePanel);
			itemObj.transform.localPosition = new Vector3(i * itemSpacing, 0, 0);
			itemObj.GetComponent<Image>().sprite = rouletteItems[i].itemSprite;
			itemObjects.Add(itemObj);

			// Добавляем обработчик нажатия на кнопку активации каждого предмета
			if (rouletteItems[i].unlockButton != null)
			{
				int index = i; // Чтобы не потерять индекс в замыкании
				rouletteItems[i].unlockButton.onClick.AddListener(() => OnUnlockButtonClick(rouletteItems[index]));
			}
		}
	}

	// Метод для получения случайного предмета на основе шансов
	private CaseItem GetRandomItemBasedOnChance()
{
	if (caseItems == null || caseItems.Count == 0)
	{
		Debug.LogError("Список caseItems пуст!");
		return null;  // Вернуть null, чтобы избежать дальнейших ошибок
	}

	float totalChance = 0f;
	foreach (var item in caseItems)
	{
		if (item == null)
		{
			Debug.LogError("Найден пустой элемент в caseItems!");
			continue;  // Пропускаем пустой элемент, если он есть
		}
		totalChance += item.spawnChance;
	}

	float randomValue = Random.Range(0f, totalChance);

	float currentChance = 0f;
	foreach (var item in caseItems)
	{
		if (item == null)
		{
			continue;  // Пропускаем пустые элементы
		}

		currentChance += item.spawnChance;
		if (randomValue <= currentChance)
		{
			return item;
		}
	}

	// Если по какой-то причине не найден элемент (не должно быть такого), вернем первый
	return caseItems[0];
}


	// Метод для начала прокрутки рулетки с оплатой кликами
	public void StartRoulette()
	{
		if (Clicker.GetClickCount() >= clickCost)
		{
			Clicker.RemoveClicks(clickCost);
			ResetRoulettePanel();
			CreateNewRoulettePanel();
			StartCoroutine(SpinRoulette());

			// Обновляем UI
			Clicker.Instance.UpdateAllScoreTexts();

			roulettePanelUI.SetActive(true);
			startButton.gameObject.SetActive(false);
		}
		else
		{
			Debug.Log("Недостаточно кликов для запуска рулетки!");
		}
	}

	// Метод для сброса позиции панели рулетки
	private void ResetRoulettePanel()
	{
		roulettePanel.localPosition = Vector3.zero;
	}

	// Метод для прокрутки рулетки
	private IEnumerator SpinRoulette()
	{
		float spinDuration = Random.Range(2f, 4f);
		float elapsedTime = 0f;
		float startPosition = roulettePanel.localPosition.x;
		float targetPosition = startPosition - (totalItemsCount * itemSpacing);

		while (elapsedTime < spinDuration)
		{
			float t = elapsedTime / spinDuration;
			t = Mathf.SmoothStep(0, 1, t);
			float currentPosition = Mathf.Lerp(startPosition, targetPosition, t);
			roulettePanel.localPosition = new Vector3(currentPosition, 0, 0);

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		roulettePanel.localPosition = new Vector3(targetPosition, 0, 0);

		int winningIndex = GetClosestItemToArrow();
		HandleSpinResult(winningIndex);

		closeButton.gameObject.SetActive(true);
	}

	// Метод для определения предмета, ближайшего к центру экрана
	private int GetClosestItemToArrow()
	{
		int closestIndex = 0;
		float closestDistance = float.MaxValue;
		Vector3 arrowPosition = arrowSprite.transform.position;

		for (int i = 0; i < itemObjects.Count; i++)
		{
			Vector3 itemPosition = itemObjects[i].transform.position;
			float distance = Mathf.Abs(itemPosition.x - arrowPosition.x);

			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestIndex = i;
			}
		}

		return closestIndex;
	}

private void HandleSpinResult(int winningIndex)
{
	// Получаем предмет по индексу
	CaseItem resultItem = rouletteItems[winningIndex];

	// Сохраняем количество кликов до добавления бонусов
	int previousClickCount = Clicker.GetClickCount();

	// Выводим результат в консоль для отладки
	Debug.Log($"Поздравляем! Вы выиграли: {resultItem.itemName}");

	// Загружаем состояние активации для предмета (на случай, если игра не обновила)
	resultItem.LoadActivationState();

	// Проверяем, был ли предмет уже активирован
	if (resultItem.isActivated)
	{
		// Если предмет уже активирован, начисляем бонус
		// Добавляем бонус без учета множителя
		Clicker.AddClicks(resultItem.bonusClicks); // Добавляем фиксированный бонус
		Debug.Log($"Предмет {resultItem.itemName} уже активирован. Вам начислен бонус: {resultItem.bonusClicks} кликов.");

		// После добавления бонусов, вычисляем разницу
		int addedClicks = Clicker.GetClickCount() - previousClickCount;
		Debug.Log($"Бонус начислен: {addedClicks} кликов.");

		// Показываем сообщение через MessageManager
		if (messageManager != null)
		{
			messageManager.ShowMessage($"Предмет открыт, вы получили бонус: {addedClicks} кликов.");
		}
	}
	else
	{
		// Если предмет ещё не активирован, активируем его и даём бонус
		// Изменяем фон на указанный для этого предмета
		if (backgroundImage != null && resultItem.backgroundSprite != null)
		{
			backgroundImage.sprite = resultItem.backgroundSprite;  // Меняем фон
		}

		// Обновляем состояние кнопок
		EnableUnlockButton(resultItem);

		// Показываем сообщение через MessageManager
		if (messageManager != null)
		{
			messageManager.ShowMessage($"Поздравляем! Вы выиграли: {resultItem.itemName}");
		}

		// Если предмет активирован, начисляем бонус кликов
		// Бонус не зависит от множителя кликов
		Clicker.AddClicks(resultItem.bonusClicks); // Добавляем фиксированный бонус кликов
		Debug.Log($"Получено бонусных кликов: {resultItem.bonusClicks}");

		// После добавления бонусов, вычисляем разницу
		int addedClicks = Clicker.GetClickCount() - previousClickCount;
		Debug.Log($"Бонус начислен: {addedClicks} кликов.");

		// Устанавливаем, что предмет теперь активирован
		resultItem.isActivated = true;

		// Сохраняем состояние активации предмета
		resultItem.SaveActivationState();
	}
}










	// Метод для активации кнопок для выбранного предмета
	private void EnableUnlockButton(CaseItem resultItem)
	{
		if (resultItem.unlockButton != null)
		{
			resultItem.unlockButton.interactable = true;
			resultItem.unlockButton.gameObject.SetActive(true);
		}

		if (resultItem.lockedButton != null)
		{
			resultItem.lockedButton.interactable = false;
			resultItem.lockedButton.gameObject.SetActive(false);
		}

		PlayerPrefs.SetInt("UnlockState_" + resultItem.itemName, 1);
		PlayerPrefs.Save();
	}


	// Анимация для выделения выигравшего элемента
	private IEnumerator AnimateWinningItem(GameObject winningItem)
	{
		Vector3 originalScale = winningItem.transform.localScale;
		Vector3 enlargedScale = originalScale * 1.2f;
		float duration = 0.5f;
		float elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			winningItem.transform.localScale = Vector3.Lerp(originalScale, enlargedScale, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			winningItem.transform.localScale = Vector3.Lerp(enlargedScale, originalScale, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		winningItem.transform.localScale = originalScale;
	}

	// Метод для закрытия панели рулетки
	private void CloseRoulettePanel()
	{
		roulettePanelUI.SetActive(false);
		startButton.gameObject.SetActive(true);
		closeButton.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		Clicker.OnClickAdded -= HandleClickAdded;
	}

	private void LoadUnlockButtonsState()
	{
		foreach (var item in caseItems)
		{
			// Получаем состояние кнопки для каждого предмета из PlayerPrefs
			int state = PlayerPrefs.GetInt("UnlockState_" + item.itemName, 0);

			if (state == 1)
			{
				// Если предмет был разблокирован (состояние 1), то кнопка активации должна быть активна
				if (item.unlockButton != null)
				{
					item.unlockButton.interactable = true;  // Сделать кнопку активной
					item.unlockButton.gameObject.SetActive(true);  // Сделать кнопку видимой
				}

				// Блокируем кнопку заглушки
				if (item.lockedButton != null)
				{
					item.lockedButton.interactable = false;  // Блокируем кнопку
					item.lockedButton.gameObject.SetActive(false);  // Скрываем кнопку
				}
			}
			else
			{
				// Если предмет не был разблокирован (состояние 0), показываем кнопку блокировки
				if (item.unlockButton != null)
				{
					item.unlockButton.interactable = false;  // Делаем кнопку неактивной
					item.unlockButton.gameObject.SetActive(false);  // Скрываем кнопку
				}

				// Показываем кнопку блокировки
				if (item.lockedButton != null)
				{
					item.lockedButton.interactable = true;  // Разрешаем её использование
					item.lockedButton.gameObject.SetActive(true);  // Показываем кнопку
				}
			}
		}
	}


	// Обработчик нажатия на кнопку активации
	private void OnUnlockButtonClick(CaseItem resultItem)
	{
		if (backgroundImage != null && resultItem.backgroundSprite != null)
		{
			backgroundImage.sprite = resultItem.backgroundSprite;
		}
	}
}




// В структуре CaseItem добавим метод для обработки нажатия кнопки

[System.Serializable]
public class CaseItem
{
	public string itemName;              // Имя предмета
	public Sprite itemSprite;            // Изображение предмета
	[Range(0, 1)] public float spawnChance; // Шанс появления предмета

	public Button lockedButton;           // Кнопка заглушки
	public Button unlockButton;           // Кнопка выбора фона

	public Sprite backgroundSprite;      // Фон, который будет отображаться при выборе предмета
	public int bonusClicks;              // Количество бонусных кликов
	public bool isActivated = false;     // Параметр, указывающий активирован ли предмет

	// Метод для загрузки состояния активации
	public void LoadActivationState()
	{
		string key = "item_" + itemName;
		isActivated = PlayerPrefs.GetInt(key, 0) == 1; 
	}

	// Метод для сохранения состояния активации
	public void SaveActivationState()
	{
		string key = "item_" + itemName;
		PlayerPrefs.SetInt(key, isActivated ? 1 : 0); 
		PlayerPrefs.Save();
	}

// Метод для активации фона
public void ActivateItem()
{
    if (backgroundSprite != null)
    {
        // Меняем фон
        RouletteManager.Instance.ChangeBackground(backgroundSprite);
        
        // Сохраняем выбранный фон
        PlayerPrefs.SetString("SelectedBackground", itemName);
        PlayerPrefs.Save();
    }
    else
    {
        Debug.LogWarning("backgroundSprite не задан для предмета!");
    }
}

}


