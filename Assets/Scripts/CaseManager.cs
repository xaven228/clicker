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

	public RectTransform roulettePanel; // Панель рулетки
	public GameObject itemPrefab; // Префаб элемента рулетки
	public float itemSpacing = 100f; // Расстояние между элементами

	private List<GameObject> itemObjects = new List<GameObject>(); // Список объектов на панели
	private List<CaseItem> rouletteItems = new List<CaseItem>(); // Список предметов на рулетке
	private Clicker clicker; // Ссылка на скрипт Clicker

	[Header("UI")]
	public Button startButton;  // Ссылка на кнопку запуска рулетки
	public GameObject roulettePanelUI;  // Ссылка на панель UI с рулеткой
	public Button closeButton;  // Кнопка закрытия панели рулетки

	// Добавляем ссылку на объект стрелки
	public GameObject arrowSprite;  // Стрелка, указывающая на центр
	public float arrowOffsetY = -50f; // Смещение стрелки по оси Y (для того, чтобы она была под панелью)

	// Ссылка на MessageManager для отображения сообщений
	public MessageManager messageManager;  // Ссылка на объект MessageManager

	void Start()
	{
		clicker = Object.FindFirstObjectByType<Clicker>(); // Получаем ссылку на Clicker
		if (clicker == null)
		{
			Debug.LogError("Clicker не найден на сцене!");
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
	}

	// Обработчик события изменения кликов
	private void HandleClickAdded(int amount)
	{
		// Здесь можно добавить дополнительную логику, если нужно
		Debug.Log($"Клики изменены на {amount}. Текущее количество: {Clicker.GetClickCount()}");
	}

	// Метод для создания новой панели рулетки
	private void CreateNewRoulettePanel()
	{
		// Очищаем текущие объекты на панели
		foreach (var itemObject in itemObjects)
		{
			Destroy(itemObject);
		}
		itemObjects.Clear();
		rouletteItems.Clear();

		// Генерируем предметы для рулетки с учетом их шансов
		for (int i = 0; i < totalItemsCount; i++)
		{
			CaseItem randomItem = GetRandomItemBasedOnChance();  // Получаем случайный предмет с учетом шансов
			rouletteItems.Add(randomItem);
		}

		// Создаем все предметы на рулетке с новыми случайными позициями
		for (int i = 0; i < totalItemsCount; i++)
		{
			GameObject itemObj = Instantiate(itemPrefab, roulettePanel);
			itemObj.transform.localPosition = new Vector3(i * itemSpacing, 0, 0);  // Расставляем элементы по оси X
			itemObj.GetComponent<Image>().sprite = rouletteItems[i].itemSprite;
			itemObjects.Add(itemObj);
		}
	}

	// Метод для получения случайного предмета на основе шансов
	private CaseItem GetRandomItemBasedOnChance()
	{
		float totalChance = 0f;
		foreach (var item in caseItems)
		{
			totalChance += item.spawnChance;
		}

		float randomValue = Random.Range(0f, totalChance);

		float currentChance = 0f;
		foreach (var item in caseItems)
		{
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
		if (Clicker.GetClickCount() >= clickCost)  // Проверяем количество кликов
		{
			Clicker.RemoveClicks(clickCost);  // Убираем клики для оплаты
			ResetRoulettePanel();  // Сбрасываем позицию панели рулетки
			CreateNewRoulettePanel();  // Генерируем новые элементы на панели
			StartCoroutine(SpinRoulette());  // Запускаем прокрутку

			// Обновляем UI сразу после начала рулетки
			Clicker.Instance.UpdateAllScoreTexts();

			// Показываем панель рулетки и скрываем кнопку старта
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
		roulettePanel.localPosition = Vector3.zero;  // Сбрасываем позицию панели в начальную
	}

	// Метод для прокрутки рулетки
	private IEnumerator SpinRoulette()
{
    // Случайное время прокрутки
    float spinDuration = Random.Range(2f, 4f);
    float elapsedTime = 0f;

    // Прокрутка рулетки по оси X (справа налево)
    float startPosition = roulettePanel.localPosition.x;
    float targetPosition = startPosition - (totalItemsCount * itemSpacing);  // Расстояние для полной прокрутки

    while (elapsedTime < spinDuration)
    {
        float t = elapsedTime / spinDuration;
        t = Mathf.SmoothStep(0, 1, t); // Плавное замедление
        float currentPosition = Mathf.Lerp(startPosition, targetPosition, t);
        roulettePanel.localPosition = new Vector3(currentPosition, 0, 0);  // Обновляем позицию панели с рулетки

        elapsedTime += Time.deltaTime;
        yield return null;
    }

    // После завершения прокрутки, обновляем позицию
    roulettePanel.localPosition = new Vector3(targetPosition, 0, 0);

    // Определяем предмет, который находится над стрелкой
    int winningIndex = GetClosestItemToArrow();
    HandleSpinResult(winningIndex);

    // Показываем кнопку закрытия после завершения прокрутки
    closeButton.gameObject.SetActive(true);
}


	// Метод для определения предмета, ближайшего к центру экрана
		private int GetClosestItemToArrow()
	{
		int closestIndex = 0;
		float closestDistance = float.MaxValue;

		// Получаем позицию стрелки в мировых координатах
		Vector3 arrowPosition = arrowSprite.transform.position;

		// Перебираем все объекты на рулетке и находим тот, который ближе всего к стрелке
		for (int i = 0; i < itemObjects.Count; i++)
		{
			// Получаем позицию предмета на рулетке в мировых координатах
			Vector3 itemPosition = itemObjects[i].transform.position;

			// Рассчитываем расстояние между стрелкой и объектом по оси X
			float distance = Mathf.Abs(itemPosition.x - arrowPosition.x);

			// Если это ближайший объект, обновляем информацию
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestIndex = i;
			}
		}

		return closestIndex;
	}



	// Метод для обработки результата спина
	private void HandleSpinResult(int winningIndex)
	{
		// Получаем предмет по индексу
		CaseItem resultItem = rouletteItems[winningIndex];  // Берем элемент по индексу

		// Выводим результат
		Debug.Log($"Поздравляем! Вы выиграли: {resultItem.name}");

		// Выделяем выигравший элемент (для визуализации, например, через изменение цвета или эффекта)
		GameObject winningItem = itemObjects[winningIndex];
		Image winningImage = winningItem.GetComponent<Image>();
		winningImage.color = Color.green; // Подсвечиваем выигравший элемент

		// Можно добавить анимацию (например, увеличение или мигание)
		StartCoroutine(AnimateWinningItem(winningItem));

		// Показываем сообщение через MessageManager
		if (messageManager != null)
		{
			messageManager.ShowMessage($"Поздравляем! Вы выиграли: {resultItem.name}");
		}

		// Можно добавить логику для добавления предмета в инвентарь или другие действия
	}

	// Анимация для выделения выигравшего элемента
	private IEnumerator AnimateWinningItem(GameObject winningItem)
	{
		Vector3 originalScale = winningItem.transform.localScale;
		Vector3 enlargedScale = originalScale * 1.2f;  // Увеличиваем элемент на 20%

		float duration = 0.5f; // Длительность анимации
		float elapsedTime = 0f;

		// Увеличиваем размер
		while (elapsedTime < duration)
		{
			winningItem.transform.localScale = Vector3.Lerp(originalScale, enlargedScale, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// Возвращаем в исходный размер
		elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			winningItem.transform.localScale = Vector3.Lerp(enlargedScale, originalScale, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// Убедитесь, что элемент вернулся в исходный размер
		winningItem.transform.localScale = originalScale;
	}

	// Метод для закрытия панели рулетки
	private void CloseRoulettePanel()
	{
		// Скрываем панель рулетки и показываем кнопку старта
		roulettePanelUI.SetActive(false);
		startButton.gameObject.SetActive(true);

		// Скрываем кнопку закрытия
		closeButton.gameObject.SetActive(false);
	}

	// Отписываемся от события при уничтожении объекта
	private void OnDestroy()
	{
		Clicker.OnClickAdded -= HandleClickAdded;
	}
}


// Структура для предмета рулетки
[System.Serializable]
public class CaseItem
{
	public string name;                // Имя предмета
	public Sprite itemSprite;          // Изображение предмета
	[Range(0, 1)] public float spawnChance; // Шанс появления предмета

	// Ссылки на кнопки для заглушки и выбора фона
	public Button lockedButton;        // Кнопка заглушки
	public Button unlockButton;        // Кнопка выбора фона
}
