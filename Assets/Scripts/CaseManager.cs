using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BackgroundItem
{
    public string name; // Название фона
    public Sprite backgroundSprite; // Спрайт фона
    public int probability; // Вероятность выпадения (в процентах)
    public Vector2 size = new Vector2(100, 100); // Размер элемента (ширина и высота)

    // Кнопка заблокированного состояния
    public Button lockedButton;

    // Кнопка разблокированного состояния
    public Button unlockedButton;

    // Флаг, указывающий, разблокирован ли фон
    public bool isUnlocked = false;
}

public class CaseManager : MonoBehaviour
{
    // Ссылка на Clicker для изменения количества кликов
    public Clicker clicker;

    // Ссылка на MessageManager для вывода сообщений
    public MessageManager messageManager;

    // Массив доступных фонов с их вероятностями
    public BackgroundItem[] backgrounds;

    // Цена открытия кейса
    public int casePrice = 1000;

    // Ссылка на текущий фон (например, Image компонент)
    public Image currentBackground;

    // Кнопка для открытия кейса
    public Button openCaseButton;

    // Текстовое поле для отображения результата открытия
    public Text resultText;

    // Панель для рулетки (содержит все спрайты фонов)
    public Transform roulettePanel;

    // Минимальное и максимальное время прокрутки (в секундах)
    public float minSpinDuration = 5f;
    public float maxSpinDuration = 8f;

    // Флаг для блокировки кнопки во время анимации
    private bool isSpinning = false;

    // Список для хранения элементов рулетки
    private List<Image> rouletteItems = new List<Image>();

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Проверяем, что все ссылки назначены
        if (openCaseButton == null || currentBackground == null || resultText == null || roulettePanel == null)
        {
            Debug.LogError("Не все ссылки назначены в инспекторе!");
            return;
        }

        // Создаём элементы рулетки один раз
        CreateRouletteItems();

        // Подсвечиваем все элементы перед началом игры
        HighlightAllItems(true);

        // Загружаем состояние фонов
        foreach (var background in backgrounds)
        {
            LoadBackgroundState(background);
        }

        // Инициализируем кнопки инвентаря
        InitializeInventoryButtons();

        // Назначаем обработчик события для кнопки открытия кейса
        openCaseButton.onClick.AddListener(OpenCase);

        // Загружаем выбранный фон
        BackgroundItem loadedBackground = LoadSelectedBackground();
        if (loadedBackground != null)
        {
            SetBackground(loadedBackground);
        }
    }

    // Метод для создания элементов рулетки
    private void CreateRouletteItems()
    {
        // Очищаем предыдущие элементы в рулетке
        foreach (Transform child in roulettePanel)
        {
            Destroy(child.gameObject);
        }

        rouletteItems.Clear();

        // Создаём новые элементы для каждого фона
        foreach (var background in backgrounds)
        {
            if (background.backgroundSprite != null)
            {
                // Создаём новый объект Image
                GameObject imageObject = new GameObject(background.name, typeof(Image));
                imageObject.transform.SetParent(roulettePanel, false);

                // Настройка Image
                Image imageComponent = imageObject.GetComponent<Image>();
                imageComponent.sprite = background.backgroundSprite;
                imageComponent.preserveAspect = true; // Сохраняем пропорции спрайта

                // Устанавливаем размеры
                RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = background.size; // Используем размер из массива

                // Делаем элемент невидимым по умолчанию
                imageComponent.color = new Color(1, 1, 1, 0); // Прозрачный цвет

                // Добавляем элемент в список
                rouletteItems.Add(imageComponent);
            }
        }
    }

    // Метод для подсветки всех элементов
    private void HighlightAllItems(bool highlight)
    {
        foreach (Image item in rouletteItems)
        {
            if (item != null)
            {
                item.color = highlight ? Color.white : new Color(1, 1, 1, 0);
            }
        }
    }

    // Метод для инициализации кнопок инвентаря
    private void InitializeInventoryButtons()
    {
        foreach (var background in backgrounds)
        {
            if (background.lockedButton != null && background.unlockedButton != null)
            {
                // Заблокированная кнопка видима, если фон не разблокирован
                background.lockedButton.gameObject.SetActive(!background.isUnlocked);

                // Разблокированная кнопка видима, если фон разблокирован
                background.unlockedButton.gameObject.SetActive(background.isUnlocked);

                // Добавляем обработчик события для разблокированной кнопки
                background.unlockedButton.onClick.AddListener(() => SetBackground(background));

                Debug.Log($"Инициализированы кнопки для фона: {background.name}");
            }
            else
            {
                Debug.LogError($"Кнопки для фона {background.name} не назначены!");
            }
        }
    }

    // Метод для открытия кейса
    private void OpenCase()
    {
        // Проверяем, идёт ли уже анимация рулетки
        if (isSpinning)
        {
            return;
        }

        // Получаем текущее количество кликов
        int currentClicks = Clicker.GetClickCount();

        // Проверяем, достаточно ли кликов для открытия кейса
        if (currentClicks < casePrice)
        {
            if (messageManager != null)
            {
                messageManager.ShowMessage("Недостаточно кликов для открытия кейса!");
            }
            else
            {
                Debug.LogError("Ссылка на MessageManager не назначена!");
            }
            return;
        }

        // Вычитаем стоимость открытия кейса
        Clicker.AddClicks(-casePrice);
        clicker.UpdateAllScoreTexts();

        // Гасим все элементы перед началом анимации
        HighlightAllItems(false);

        // Запускаем анимацию рулетки
        StartCoroutine(SpinRoulette());
    }

    // Корутина для анимации рулетки
    private IEnumerator SpinRoulette()
    {
        isSpinning = true;

        // Генерируем случайное время прокрутки
        float spinDuration = Random.Range(minSpinDuration, maxSpinDuration);

        // Генерируем случайный результат
        int randomValue = Random.Range(0, 100); // Число от 0 до 99
        int cumulativeProbability = 0;
        BackgroundItem selectedBackground = null;

        foreach (var background in backgrounds)
        {
            cumulativeProbability += background.probability;

            if (randomValue < cumulativeProbability)
            {
                selectedBackground = background;
                break;
            }
        }

        // Анимация случайного подсвечивания
        float elapsedTime = 0f;
        int previousIndex = -1;

        while (elapsedTime < spinDuration)
        {
            // Выбираем случайный индекс
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, rouletteItems.Count);
            } while (randomIndex == previousIndex); // Убеждаемся, что индекс не повторяется

            // Делаем предыдущий элемент невидимым
            if (previousIndex != -1)
            {
                rouletteItems[previousIndex].color = new Color(1, 1, 1, 0);
            }

            // Делаем текущий элемент видимым
            rouletteItems[randomIndex].color = Color.white;

            // Запоминаем текущий индекс как предыдущий
            previousIndex = randomIndex;

            // Ждём 0.5 секунды
            yield return new WaitForSeconds(0.5f);

            // Увеличиваем прошедшее время
            elapsedTime += 0.5f;
        }

        // В конце анимации делаем выбранный элемент видимым
        if (selectedBackground.backgroundSprite != null)
        {
            // Находим индекс выбранного элемента
            int selectedIndex = -1;
            for (int i = 0; i < backgrounds.Length; i++)
            {
                if (backgrounds[i].backgroundSprite == selectedBackground.backgroundSprite)
                {
                    selectedIndex = i;
                    break;
                }
            }

            if (selectedIndex != -1)
            {
                // Гасим все элементы
                foreach (Image item in rouletteItems)
                {
                    item.color = new Color(1, 1, 1, 0);
                }

                // Делаем выбранный элемент видимым
                rouletteItems[selectedIndex].color = Color.white;

                // Разблокируем фон
                UnlockBackground(selectedBackground);

                if (resultText != null)
                {
                    resultText.text = $"Вы выиграли: {selectedBackground.name}";
                }

                if (messageManager != null)
                {
                    messageManager.ShowMessage($"Вы выиграли: {selectedBackground.name}");
                }
                else
                {
                    Debug.LogError("Ссылка на MessageManager не назначена!");
                }
            }
        }

        // Завершаем анимацию
        isSpinning = false;
    }

    // Метод для разблокировки фона
    private void UnlockBackground(BackgroundItem background)
    {
        if (!background.isUnlocked)
        {
            background.isUnlocked = true;

            // Скрываем заблокированную кнопку
            if (background.lockedButton != null)
            {
                background.lockedButton.gameObject.SetActive(false);
            }

            // Показываем разблокированную кнопку
            if (background.unlockedButton != null)
            {
                background.unlockedButton.gameObject.SetActive(true);
            }

            // Сохраняем состояние фона
            SaveBackgroundState(background);

            Debug.Log($"Фон '{background.name}' разблокирован!");
        }
    }

    // Метод для установки нового фона
    public void SetBackground(BackgroundItem background)
    {
        if (currentBackground != null && background.backgroundSprite != null)
        {
            currentBackground.sprite = background.backgroundSprite;
            Debug.Log($"Фон изменён на: {background.name}");

            // Сохраняем выбранный фон
            SaveSelectedBackground(background);
        }
    }

    // Метод для сохранения состояния фона
    private void SaveBackgroundState(BackgroundItem background)
    {
        string key = $"Background_{background.name}_IsUnlocked";
        PlayerPrefs.SetInt(key, background.isUnlocked ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Состояние фона '{background.name}' сохранено: {background.isUnlocked}");
    }

    // Метод для загрузки состояния фона
    private void LoadBackgroundState(BackgroundItem background)
    {
        string key = $"Background_{background.name}_IsUnlocked";
        background.isUnlocked = PlayerPrefs.GetInt(key, 0) == 1;
        Debug.Log($"Состояние фона '{background.name}' загружено: {background.isUnlocked}");
    }

    // Метод для сохранения выбранного фона
    private void SaveSelectedBackground(BackgroundItem background)
    {
        if (background != null)
        {
            PlayerPrefs.SetString("SelectedBackground", background.name);
            PlayerPrefs.Save();
            Debug.Log($"Выбранный фон '{background.name}' сохранён.");
        }
    }

    // Метод для загрузки выбранного фона
    private BackgroundItem LoadSelectedBackground()
    {
        string selectedBackgroundName = PlayerPrefs.GetString("SelectedBackground", "");
        foreach (var background in backgrounds)
        {
            if (background.name == selectedBackgroundName && background.isUnlocked)
            {
                Debug.Log($"Загружен выбранный фон: {background.name}");
                return background;
            }
        }
        Debug.Log("Выбранный фон не найден или заблокирован.");
        return null;
    }
}