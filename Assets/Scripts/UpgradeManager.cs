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
}

public class UpgradeManager : MonoBehaviour
{
    // Ссылка на скрипт кликера для доступа к счётчику кликов
    public Clicker clicker;

    // Ссылка на MessageManager для вывода сообщений
    public MessageManager messageManager;

    // Массив улучшений
    public Upgrade[] upgrades;

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Инициализация всех улучшений
        foreach (var upgrade in upgrades)
        {
            if (upgrade.upgradeButton != null && upgrade.priceText != null)
            {
                // Обновляем текст цены
                UpdatePriceText(upgrade);

                // Назначаем обработчик события для кнопки
                upgrade.upgradeButton.onClick.AddListener(() => TryPurchaseUpgrade(upgrade));
            }
        }
    }

    // Метод для попытки покупки улучшения
    private void TryPurchaseUpgrade(Upgrade upgrade)
    {
        int currentClicks = Clicker.GetClickCount();

        // Проверяем, достаточно ли кликов для покупки
        if (currentClicks >= upgrade.price)
        {
            // Вычитаем стоимость улучшения из счётчика кликов
            Clicker.SetClickCount(currentClicks - upgrade.price);
            clicker.UpdateAllScoreTexts(); // Обновляем текстовые поля счётчика

            // Применяем бонус (множитель)
            ApplyBonus(upgrade.bonusMultiplier);

            // Создаём префаб кнопки над кнопкой
            if (upgrade.upgradePrefab != null)
            {
                Button instantiatedButton = Instantiate(upgrade.upgradePrefab, upgrade.upgradeButton.transform);
                instantiatedButton.transform.localPosition = Vector3.zero; // Центрируем кнопку
                instantiatedButton.interactable = false; // Делаем кнопку неактивной (если нужно)
            }

            // Отключаем кнопку после покупки
            upgrade.upgradeButton.interactable = false;

            // Очищаем текст цены
            if (upgrade.priceText != null)
            {
                upgrade.priceText.text = "Куплено!";
            }

            // Показываем сообщение о успешной покупке
            if (messageManager != null)
            {
                string message = $"Поздравляем с улучшением! Клики теперь умножаются в {upgrade.bonusMultiplier:F1} раз";
                messageManager.ShowMessage(message);
            }
        }
        else
        {
            // Показываем сообщение о нехватке средств
            if (messageManager != null)
            {
                messageManager.ShowMessage("Недостаточно средств");
            }
        }
    }

    // Метод для применения бонуса (множителя)
    private void ApplyBonus(float bonusMultiplier)
    {
        // Увеличиваем множитель кликов
        int currentClicks = Clicker.GetClickCount();
        int newClicks = Mathf.RoundToInt(currentClicks * bonusMultiplier);
        Clicker.SetClickCount(newClicks);
        clicker.UpdateAllScoreTexts(); // Обновляем текстовые поля счётчика
    }

    // Метод для обновления текста цены
    private void UpdatePriceText(Upgrade upgrade)
    {
        if (upgrade.priceText != null)
        {
            upgrade.priceText.text = "Цена: " + upgrade.price;
        }
    }
}