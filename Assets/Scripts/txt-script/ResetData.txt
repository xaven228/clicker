using UnityEngine;

public class ResetData : MonoBehaviour
{
    // Ссылка на Clicker для сброса кликов и множителя
    public Clicker clicker;

    // Ссылка на AchievementManager для сброса достижений
    public AchievementManager achievementManager;

    // Ссылка на UpgradeManager для сброса улучшений
    public UpgradeManager upgradeManager;

    // Ссылка на объект фона
    public SpriteRenderer background;

    // Начальный спрайт фона
    public Sprite defaultBackground;

    // Метод для полного сброса данных
    public void ResetAllData()
    {
        // Сбрасываем данные в Clicker
        if (clicker != null)
        {
            Clicker.ResetProgress();
        }

        // Сбрасываем данные в AchievementManager
        if (achievementManager != null)
        {
            foreach (var achievement in achievementManager.achievements)
            {
                achievement.isUnlocked = false;
                achievementManager.SaveAchievementProgress(achievement);
            }

            // Обновляем интерфейс достижений
            achievementManager.UpdateAchievementUI();
        }

        // Сбрасываем данные в UpgradeManager (инвентарь)
        ResetInventory();

        // Сбрасываем фон
        ResetBackground();

        // Очищаем PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Все данные сброшены!");

        // Обновляем интерфейс
        if (clicker != null)
        {
            clicker.UpdateAllScoreTexts();
        }
    }

    // Метод для сброса инвентаря (улучшений)
    private void ResetInventory()
    {
        if (upgradeManager != null)
        {
            foreach (var upgrade in upgradeManager.upgrades)
            {
                // Сбрасываем состояние улучшения
                upgrade.isPurchased = false;

                // Восстанавливаем интерфейс улучшений
                if (upgrade.upgradeButton != null)
                {
                    upgrade.upgradeButton.interactable = true; // Делаем кнопку активной
                }

                if (upgrade.priceText != null)
                {
                    upgrade.priceText.text = "Цена: " + upgrade.price; // Восстанавливаем текст цены
                }
            }

            // Сбрасываем глобальный множитель
            Clicker.SetGlobalMultiplier(1f);

            Debug.Log("Инвентарь сброшен.");
        }
    }

    // Метод для сброса фона
    private void ResetBackground()
    {
        if (background != null && defaultBackground != null)
        {
            background.sprite = defaultBackground;
            Debug.Log("Фон сброшен.");
        }
    }
}