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

        // Назначаем обработчик события для кнопки открытия кейса
        openCaseButton.onClick.AddListener(OpenCase);
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

                // Устанавливаем выбранный фон
                SetBackground(selectedBackground);

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

    // Метод для установки нового фона
    private void SetBackground(BackgroundItem background)
    {
        if (currentBackground != null && background.backgroundSprite != null)
        {
            currentBackground.sprite = background.backgroundSprite;
        }
    }

    // Метод OnValidate вызывается при изменении значений в инспекторе
    private void OnValidate()
    {
        // Гасим все элементы в редакторе
        foreach (Transform child in roulettePanel)
        {
            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(1, 1, 1, 0);
            }
        }
    }
}