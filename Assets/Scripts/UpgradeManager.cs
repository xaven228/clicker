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

    // Префикс для ключей PlayerPrefs
    private const string PURCHASED_KEY_PREFIX = "UpgradePurchased_";

    // Флаг для сброса улучшений
    private bool resetUpgrades = false;

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Проверяем, была ли нажата клавиша R для сброса улучшений
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllUpgrades();
            Debug.Log("Улучшения сброшены!");
        }

        // Инициализация всех улучшений
        foreach (var upgrade in upgrades)
        {
            if (upgrade.upgradeButton != null && upgrade.priceText != null)
            {
                // Загружаем состояние улучшения из PlayerPrefs
                string key = PURCHASED_KEY_PREFIX + upgrade.upgradeName;
                bool isPurchased = PlayerPrefs.GetInt(key, 0) == 1;

                Debug.Log($"Загружено улучшение: {key}, куплено: {isPurchased}");

                if (isPurchased)
                {
                    // Если улучшение уже куплено, обновляем интерфейс
                    MarkAsPurchased(upgrade);
                }
                else
                {
                    // Если улучшение не куплено, обновляем текст цены
                    UpdatePriceText(upgrade);

                    // Назначаем обработчик события для кнопки
                    upgrade.upgradeButton.onClick.AddListener(() => TryPurchaseUpgrade(upgrade));
                }
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

            // Сохраняем состояние улучшения
            string key = PURCHASED_KEY_PREFIX + upgrade.upgradeName;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();

            Debug.Log($"Сохранено улучшение: {key}"); // Отладочное сообщение

            // Показываем сообщение о успешной покупке
            if (messageManager != null)
            {
                string message = $"Поздравляем с улучшением! Клики теперь умножаются в {Clicker.GetGlobalMultiplier():F1} раз";
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
        // Увеличиваем глобальный множитель
        Clicker.IncreaseGlobalMultiplier(bonusMultiplier - 1); // Вычитаем 1, так как базовый множитель уже равен 1
        Debug.Log($"Глобальный множитель теперь: {Clicker.GetGlobalMultiplier()}");

        // Обновляем текстовые поля счётчика
        clicker.UpdateAllScoreTexts();
    }

    // Метод для обновления текста цены
    private void UpdatePriceText(Upgrade upgrade)
    {
        if (upgrade.priceText != null)
        {
            upgrade.priceText.text = "Цена: " + upgrade.price;
        }
    }

    // Метод для отметки улучшения как купленного
    private void MarkAsPurchased(Upgrade upgrade)
    {
        // Отключаем кнопку
        if (upgrade.upgradeButton != null)
        {
            upgrade.upgradeButton.interactable = false;
        }

        // Обновляем текст цены
        if (upgrade.priceText != null)
        {
            upgrade.priceText.text = "Куплено!";
        }

        // Создаём префаб кнопки над кнопкой
        if (upgrade.upgradePrefab != null)
        {
            Button instantiatedButton = Instantiate(upgrade.upgradePrefab, upgrade.upgradeButton.transform);
            instantiatedButton.transform.localPosition = Vector3.zero; // Центрируем кнопку
            instantiatedButton.interactable = false; // Делаем кнопку неактивной (если нужно)
        }
    }

    // Метод для сброса всех улучшений
    private void ResetAllUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            string key = PURCHASED_KEY_PREFIX + upgrade.upgradeName;

            // Удаляем данные о покупке из PlayerPrefs
            PlayerPrefs.DeleteKey(key);

            // Возвращаем кнопку в исходное состояние
            if (upgrade.upgradeButton != null)
            {
                upgrade.upgradeButton.interactable = true;
            }

            // Возвращаем текст цены
            if (upgrade.priceText != null)
            {
                upgrade.priceText.text = "Цена: " + upgrade.price;
            }

            // Удаляем префаб кнопки, если он был создан
            if (upgrade.upgradePrefab != null)
            {
                // Ищем дочерний объект с именем префаба
                Transform prefabInstance = upgrade.upgradeButton.transform.Find(upgrade.upgradePrefab.name);
                if (prefabInstance != null)
                {
                    Destroy(prefabInstance.gameObject); // Удаляем объект
                }
            }
        }

        // Сбрасываем глобальный множитель
        Clicker.SetGlobalMultiplier(1f);

        // Обновляем текстовые поля счётчика
        clicker.UpdateAllScoreTexts();

        Debug.Log("Улучшения сброшены!");
    }

    // Метод Update для проверки нажатия клавиши R
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllUpgrades();
            Debug.Log("Улучшения сброшены!");
        }
    }
}