using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Upgrade
{
    public string upgradeName; // Название улучшения
    public int price; // Цена улучшения
    public float bonusMultiplier; // Бонус (множитель)
    public Button upgradePrefab; // Префаб кнопки, который появляется над кнопкой после покупки
    public Button upgradeButton; // Кнопка для покупки улучшения
    public Text priceText; // Текстовое поле для отображения цены

    // Флаг, указывающий, куплено ли улучшение
    public bool isPurchased = false;
}

public class UpgradeManager : MonoBehaviour
{
    public Clicker clicker; // Ссылка на скрипт кликера для доступа к счётчику кликов
    public MessageManager messageManager; // Ссылка на MessageManager для вывода сообщений
    public Upgrade[] upgrades; // Массив улучшений

    private const string PURCHASED_KEY_PREFIX = "UpgradePurchased_"; // Префикс для ключей PlayerPrefs

    // Метод, вызываемый при старте игры
    void Start()
    {
        InitializeUpgrades();
    }

    // Инициализация всех улучшений
    private void InitializeUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade.upgradeButton != null && upgrade.priceText != null)
            {
                LoadUpgradeState(upgrade);

                // Назначаем обработчик события для кнопки
                if (!upgrade.isPurchased)
                {
                    upgrade.upgradeButton.onClick.AddListener(() => TryPurchaseUpgrade(upgrade));
                }
            }
        }
    }

    // Метод для загрузки состояния улучшения из PlayerPrefs
    private void LoadUpgradeState(Upgrade upgrade)
    {
        string key = PURCHASED_KEY_PREFIX + upgrade.upgradeName;
        bool isPurchased = PlayerPrefs.GetInt(key, 0) == 1;
        Debug.Log($"Загружено улучшение: {key}, куплено: {isPurchased}");

        if (isPurchased)
        {
            MarkAsPurchased(upgrade);
        }
        else
        {
            UpdatePriceText(upgrade);
        }
    }

    // Метод для попытки покупки улучшения
    private void TryPurchaseUpgrade(Upgrade upgrade)
    {
        int currentClicks = Clicker.GetClickCount();

        // Проверяем, достаточно ли кликов для покупки
        if (currentClicks >= upgrade.price && !upgrade.isPurchased)
        {
            PurchaseUpgrade(upgrade, currentClicks);
        }
        else
        {
            ShowInsufficientFundsMessage();
        }
    }

    // Покупка улучшения
    private void PurchaseUpgrade(Upgrade upgrade, int currentClicks)
    {
        // Вычитаем стоимость улучшения из счётчика кликов
        Clicker.SetClickCount(currentClicks - upgrade.price);
        clicker.UpdateAllScoreTexts(); // Обновляем текстовые поля счётчика

        // Применяем бонус (множитель)
        ApplyBonus(upgrade.bonusMultiplier);

        // Создаём префаб кнопки над кнопкой
        InstantiateUpgradeButton(upgrade);

        // Обновляем состояние кнопки
        upgrade.upgradeButton.interactable = false;
        upgrade.priceText.text = "Куплено!";

        // Сохраняем состояние улучшения
        upgrade.isPurchased = true;
        string key = PURCHASED_KEY_PREFIX + upgrade.upgradeName;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        Debug.Log($"Сохранено улучшение: {key}");

        // Показываем сообщение о успешной покупке
        ShowUpgradeMessage();
    }

    // Метод для применения бонуса (множителя)
    private void ApplyBonus(float bonusMultiplier)
    {
        Clicker.IncreaseGlobalMultiplier(bonusMultiplier - 1); // Вычитаем 1, так как базовый множитель уже равен 1
        Debug.Log($"Глобальный множитель теперь: {Clicker.GetGlobalMultiplier()}");

        clicker.UpdateAllScoreTexts(); // Обновляем текстовые поля счётчика
    }

    // Метод для создания префаба кнопки
    private void InstantiateUpgradeButton(Upgrade upgrade)
    {
        if (upgrade.upgradePrefab != null)
        {
            Button instantiatedButton = Instantiate(upgrade.upgradePrefab, upgrade.upgradeButton.transform);
            instantiatedButton.transform.localPosition = Vector3.zero; // Центрируем кнопку
            instantiatedButton.interactable = false; // Делаем кнопку неактивной
        }
    }

    // Метод для обновления текста цены
    private void UpdatePriceText(Upgrade upgrade)
    {
        if (upgrade.priceText != null)
        {
            upgrade.priceText.text = $"Цена: {upgrade.price}";
        }
    }

    // Метод для отметки улучшения как купленного
    private void MarkAsPurchased(Upgrade upgrade)
    {
        upgrade.upgradeButton.interactable = false;
        upgrade.priceText.text = "Куплено!";
        InstantiateUpgradeButton(upgrade);
    }

    // Показываем сообщение о недостаточности средств
    private void ShowInsufficientFundsMessage()
    {
        if (messageManager != null)
        {
            messageManager.ShowMessage("Недостаточно средств");
        }
    }

    // Показываем сообщение о успешной покупке
    private void ShowUpgradeMessage()
    {
        if (messageManager != null)
        {
            string message = $"Поздравляем с улучшением! Клики теперь умножаются в {Clicker.GetGlobalMultiplier():F1} раз";
            messageManager.ShowMessage(message);
        }
    }
}
