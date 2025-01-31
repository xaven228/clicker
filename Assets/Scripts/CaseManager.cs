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

    // Штраф за неудачное открытие кейса
    public int penaltyClicks = -500;

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

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Проверяем, что все ссылки назначены
        if (openCaseButton == null || currentBackground == null || resultText == null || roulettePanel == null)
        {
            Debug.LogError("Не все ссылки назначены в инспекторе!");
            return;
        }

        // Автоматически добавляем элементы в рулетку
        UpdateRouletteItems();

        // Назначаем обработчик события для кнопки открытия кейса
        openCaseButton.onClick.AddListener(OpenCase);
    }

    // Метод для обновления элементов рулетки
    private void UpdateRouletteItems()
    {
        // Очищаем предыдущие элементы в рулетке
        foreach (Transform child in roulettePanel)
        {
            Destroy(child.gameObject);
        }

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
            return;
        }

        // Вычитаем стоимость открытия кейса
        Clicker.AddClicks(-casePrice);
        clicker.UpdateAllScoreTexts();

        // Запускаем анимацию рулетки
        StartCoroutine(SpinRoulette());
    }

    // Корутина для анимации рулетки
    private IEnumerator SpinRoulette()
    {
        isSpinning = true;

        // Генерируем случайное время прокрутки
        float spinDuration = Random.Range(minSpinDuration, maxSpinDuration);

        // Получаем все дочерние объекты рулетки (спрайты фонов)
        List<Image> rouletteItems = new List<Image>();
        foreach (Transform child in roulettePanel)
        {
            Image image = child.GetComponent<Image>();
            if (image != null && image.sprite != null) // Убедитесь, что спрайт назначен
            {
                rouletteItems.Add(image);
            }
        }

        // Если нет элементов в рулетке, завершаем корутину
        if (rouletteItems.Count == 0)
        {
            Debug.LogError("В рулетке нет элементов или спрайты не назначены!");
            isSpinning = false;
            yield break;
        }

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

        // Если ничего не выпало, выбираем штраф
        if (selectedBackground == null)
        {
            selectedBackground = new BackgroundItem { name = "Штраф", backgroundSprite = null, probability = 0 };
        }

        // Анимация мерцания
        float elapsedTime = 0f;
        int currentIndex = 0;

        while (elapsedTime < spinDuration)
        {
            // Делаем текущий элемент видимым
            rouletteItems[currentIndex].color = Color.white;

            // Ждём 0.5 секунды
            yield return new WaitForSeconds(0.5f);

            // Делаем текущий элемент невидимым
            rouletteItems[currentIndex].color = new Color(1, 1, 1, 0);

            // Переходим к следующему элементу
            currentIndex = (currentIndex + 1) % rouletteItems.Count;

            // Увеличиваем прошедшее время
            elapsedTime += 1f; // Интервал между мерцаниями — 1 секунда
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
            }
        }
        else
        {
            // Применяем штраф
            Clicker.AddClicks(penaltyClicks);
            clicker.UpdateAllScoreTexts();

            if (resultText != null)
            {
                resultText.text = "Вы проиграли! Штраф: -500 кликов";
            }

            if (messageManager != null)
            {
                messageManager.ShowMessage("Вы проиграли! Штраф: -500 кликов");
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
        // Автоматически обновляем элементы рулетки при изменении массива backgrounds
        UpdateRouletteItems();
    }
}