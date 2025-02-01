using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BackgroundItem
{
    public string name;               // Название фона
    public Sprite backgroundSprite;   // Спрайт фона
    public int probability;           // Вероятность выпадения (в процентах)
    public bool isUnlocked = false;   // Флаг, указывающий, разблокирован ли фон
}

[System.Serializable]
public class BonusItem
{
    public string name;           // Название бонуса
    public int bonusAmount;       // Количество бонусных кликов
    public Sprite bonusSprite;    // Спрайт бонуса
    public int probability;       // Вероятность выпадения бонуса
}

[System.Serializable]
public class RouletteItem
{
    public string name;          // Название предмета (фон или бонус)
    public Sprite itemSprite;    // Спрайт предмета
    public int probability;      // Шанс выпадения (в процентах)
    public bool isBonus;         // Это бонус (клики) или фон?
    public int bonusAmount;      // Количество кликов (если это бонус)
}

public class CaseManager : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    public Clicker clicker;
    public MessageManager messageManager;
    public Image currentBackground;  // UI элемент для отображения текущего фона
    public Button openCaseButton;
    public Text resultText;
    public Transform roulettePanel;

    [Header("Настройки кейса")]
    public BackgroundItem[] backgrounds; // Массив фонов
    public BonusItem[] bonuses;          // Массив бонусов
    public int casePrice = 1000;         // Цена кейса

    [Header("Настройки рулетки")]
    public float minSpinDuration = 5f;
    public float maxSpinDuration = 8f;

    private bool isSpinning = false;
    private List<Image> rouletteItems = new List<Image>();

    void Start()
    {
        if (CheckRequiredReferences())
        {
            InitializeCase();
        }
    }

    // Проверка наличия всех необходимых ссылок в инспекторе
    private bool CheckRequiredReferences()
    {
        if (openCaseButton == null || currentBackground == null || resultText == null || roulettePanel == null)
        {
            Debug.LogError("Не все ссылки назначены в инспекторе!");
            return false;
        }
        return true;
    }

    // Инициализация рулетки
    private void InitializeCase()
    {
        CreateRouletteItems();
        openCaseButton.onClick.AddListener(OpenCase);
    }

    // Создание элементов рулетки
    private void CreateRouletteItems()
    {
        // Очищаем старые элементы
        foreach (Transform child in roulettePanel)
        {
            Destroy(child.gameObject);
        }

        rouletteItems.Clear();

        // Создаем фоны
        foreach (var background in backgrounds)
        {
            if (background.backgroundSprite != null)
            {
                CreateImageItem(background.backgroundSprite, roulettePanel);
            }
        }

        // Создаем бонусы
        foreach (var bonus in bonuses)
        {
            if (bonus.bonusSprite != null)
            {
                CreateImageItem(bonus.bonusSprite, roulettePanel);
            }
        }
    }

    // Создание изображения для элемента рулетки
    private void CreateImageItem(Sprite sprite, Transform parent)
    {
        GameObject imageObject = new GameObject("RouletteItem", typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image imageComponent = imageObject.GetComponent<Image>();
        imageComponent.sprite = sprite;
        imageComponent.preserveAspect = true;

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 100); // Размер каждого элемента
    }

    // Открытие кейса
    private void OpenCase()
    {
        if (isSpinning) return;

        if (Clicker.GetClickCount() < casePrice)
        {
            ShowMessage("Недостаточно кликов для открытия кейса!");
            return;
        }

        Clicker.AddClicks(-casePrice);
        clicker.UpdateAllScoreTexts();

        StartCoroutine(SpinRoulette());
    }

    // Корутина для анимации рулетки
    private IEnumerator SpinRoulette()
    {
        isSpinning = true;
        float spinDuration = Random.Range(minSpinDuration, maxSpinDuration);
        float elapsedTime = 0f;

        // Крутить рулетку
        while (elapsedTime < spinDuration)
        {
            foreach (Transform child in roulettePanel)
            {
                child.GetComponent<Image>().color = Color.white;
            }

            yield return new WaitForSeconds(0.2f);
            elapsedTime += 0.2f;
        }

        // Когда рулетка останавливается, получаем результат
        SpinResult result = GetSpinResult();
        HandleSpinResult(result);

        isSpinning = false;
    }

    // Получить результат спина (фон или бонус)
    private SpinResult GetSpinResult()
    {
        int totalProbability = 0;

        // Рассчитываем общую вероятность
        foreach (var background in backgrounds)
        {
            totalProbability += background.probability;
        }
        foreach (var bonus in bonuses)
        {
            totalProbability += bonus.probability;
        }

        int randomValue = Random.Range(0, totalProbability);

        // Проверяем, что выпал фон
        int cumulativeProbability = 0;
        foreach (var background in backgrounds)
        {
            cumulativeProbability += background.probability;
            if (randomValue < cumulativeProbability)
            {
                return new SpinResult { isBackground = true, background = background };
            }
        }

        // Проверяем, что выпал бонус
        cumulativeProbability = 0;
        foreach (var bonus in bonuses)
        {
            cumulativeProbability += bonus.probability;
            if (randomValue < cumulativeProbability)
            {
                return new SpinResult { isBackground = false, bonus = bonus };
            }
        }

        return null;
    }

    // Обработка результата спина
    private void HandleSpinResult(SpinResult result)
    {
        if (result.isBackground)
        {
            UnlockBackground(result.background);
            ShowMessage($"Вы выиграли фон: {result.background.name}");
            SetBackground(result.background); // Устанавливаем фоновое изображение
        }
        else
        {
            AddBonusClicks(result.bonus);
            ShowMessage($"Вы выиграли бонус: {result.bonus.name}!");
        }
    }

    // Разблокировка фона
    private void UnlockBackground(BackgroundItem background)
    {
        if (!background.isUnlocked)
        {
            background.isUnlocked = true;
            SaveBackgroundState(background);
        }
    }

    // Добавление бонусных кликов
    private void AddBonusClicks(BonusItem bonus)
    {
        Clicker.AddClicks(bonus.bonusAmount);
        clicker.UpdateAllScoreTexts();
    }

    // Сохранение состояния фона
    private void SaveBackgroundState(BackgroundItem background)
    {
        string key = $"Background_{background.name}_IsUnlocked";
        PlayerPrefs.SetInt(key, background.isUnlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Метод для установки выбранного фона
    public void SetBackground(BackgroundItem background)
    {
        if (currentBackground != null && background.backgroundSprite != null)
        {
            currentBackground.sprite = background.backgroundSprite;
            SaveSelectedBackground(background);
        }
    }

    // Сохранение выбранного фона
    private void SaveSelectedBackground(BackgroundItem background)
    {
        PlayerPrefs.SetString("SelectedBackground", background.name);
        PlayerPrefs.Save();
    }

    // Показ сообщения
    private void ShowMessage(string message)
    {
        if (messageManager != null)
        {
            messageManager.ShowMessage(message);
        }
        else
        {
            Debug.Log(message);
        }
    }
}

public class SpinResult
{
    public bool isBackground;
    public BackgroundItem background;
    public BonusItem bonus;
}
