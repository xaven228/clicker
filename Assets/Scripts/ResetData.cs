using UnityEngine;

public class ResetData : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Clicker clicker;
    [SerializeField] private AchievementManager achievementManager;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private CaseItem[] caseItems; // Добавление массива предметов

    [Header("Default Settings")]
    [SerializeField] private Sprite defaultBackground;

    // Метод для полного сброса данных
    public void ResetAllData()
    {
        // Сбрасываем данные по модулям
        ResetClickerData();
        ResetAchievementData();
        ResetInventoryData();
        ResetBackgroundData();
        SecurePlayerPrefs.ClearPlayerPrefs();

        Debug.Log("Все данные сброшены!");

        // Обновляем интерфейс
        UpdateScoreUI();
    }

    // Сброс данных для Clicker
    private void ResetClickerData()
    {
        if (clicker != null)
        {
            // Сбрасываем прогресс кликера
            Clicker.ResetProgress();

            // Сбрасываем все клики и множители до нуля
            Clicker.SetGlobalMultiplier(1f);
            Clicker.RemoveClicks(Clicker.GetClickCount());
        }
        else
        {
            Debug.LogWarning("Clicker не назначен в инспекторе.");
        }
    }

    // Сброс данных для AchievementManager
    private void ResetAchievementData()
    {
        if (achievementManager != null)
        {
            foreach (var achievement in achievementManager.achievements)
            {
                achievement.isUnlocked = false;

                // Сохраняем сброшенное достижение в SecurePlayerPrefs
                string unlockedKey = $"AchievementUnlocked_{achievement.achievementName}";
                SecurePlayerPrefs.SetInt(unlockedKey, 0); // 0 означает, что достижение не выполнено
            }

            // Обновляем интерфейс достижений
            achievementManager.UpdateAchievementUI();
        }
        else
        {
            Debug.LogWarning("AchievementManager не назначен в инспекторе.");
        }
    }

    // Сброс данных для инвентаря и улучшений
    private void ResetInventoryData()
    {
        if (upgradeManager != null)
        {
            foreach (var upgrade in upgradeManager.upgrades)
            {
                ResetUpgradeData(upgrade);
            }
        }
        else
        {
            Debug.LogWarning("UpgradeManager не назначен в инспекторе.");
        }

        // Сброс данных для предметов
        if (caseItems != null)
        {
            foreach (var item in caseItems)
            {
                item.isActivated = false;
                item.SaveActivationState();
                
                // Обновляем кнопки
                if (item.lockedButton != null)
                {
                    item.lockedButton.gameObject.SetActive(true);
                }
                if (item.unlockButton != null)
                {
                    item.unlockButton.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogWarning("CaseItems не назначены в инспекторе.");
        }
    }

    // Сброс состояния отдельного улучшения
    private void ResetUpgradeData(Upgrade upgrade)
    {
        if (upgrade != null)
        {
            upgrade.isPurchased = false;

            // Восстанавливаем интерфейс улучшений
            if (upgrade.upgradeButton != null)
            {
                upgrade.upgradeButton.interactable = true; // Делаем кнопку активной
            }

            if (upgrade.priceText != null)
            {
                upgrade.priceText.text = $"Цена: {upgrade.price}"; // Восстанавливаем текст цены
            }

            // Удаляем состояние покупки из SecurePlayerPrefs
            string key = "UpgradePurchased_" + upgrade.upgradeName;
            SecurePlayerPrefs.SetInt(key, 0);
        }
        else
        {
            Debug.LogWarning("Upgrade не назначен или отсутствует.");
        }
    }

    // Сброс фона
    private void ResetBackgroundData()
    {
        if (background != null && defaultBackground != null)
        {
            background.sprite = defaultBackground;
            Debug.Log("Фон сброшен.");
        }
        else
        {
            Debug.LogWarning("Background или DefaultBackground не назначены в инспекторе.");
        }

        // Сбрасываем сохраненный фон в SecurePlayerPrefs
        SecurePlayerPrefs.SetString("SelectedBackground", string.Empty);
    }

    // Обновление интерфейса с текстами счёта
    private void UpdateScoreUI()
    {
        if (clicker != null)
        {
            clicker.UpdateAllScoreTexts();
        }
        else
        {
            Debug.LogWarning("Clicker не назначен в инспекторе.");
        }
    }
}