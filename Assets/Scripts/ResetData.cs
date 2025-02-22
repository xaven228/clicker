using UnityEngine;
using UnityEngine.UI; // Добавлено пространство имён для Button

public class ResetData : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Clicker clicker;
    [SerializeField] private AchievementManager achievementManager;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private SpriteRenderer background;

    [Header("Inventory Items")]
    [SerializeField] private CaseItem[] caseItems;

    [Header("Default Settings")]
    [SerializeField] private Sprite defaultBackground;

    #region Public Methods
    /// <summary>
    /// Выполняет полный сброс всех данных игры.
    /// </summary>
    public void ResetAllData()
    {
        ResetClickerData();
        ResetAchievementData();
        ResetInventoryData();
        ResetBackgroundData();
        SecurePlayerPrefs.ClearPlayerPrefs();

        Debug.Log("Все данные успешно сброшены!");
        UpdateScoreUI();
    }
    #endregion

    #region Reset Methods
    private void ResetClickerData()
    {
        if (!ValidateReference(clicker, nameof(clicker))) return;

        Clicker.ResetProgress();
        Clicker.SetGlobalMultiplier(1f);
        Clicker.RemoveClicks(Clicker.GetClickCount());
    }

    private void ResetAchievementData()
    {
        if (!ValidateReference(achievementManager, nameof(achievementManager))) return;

        foreach (var achievement in achievementManager.Achievements)
        {
            if (achievement != null)
            {
                achievement.IsUnlocked = false;
                SecurePlayerPrefs.SetInt($"AchievementUnlocked_{achievement.AchievementName}", 0);
            }
        }
        achievementManager.UpdateAchievementUI();
    }

    private void ResetInventoryData()
    {
        ResetUpgrades();
        ResetCaseItems();
    }

    private void ResetUpgrades()
    {
        if (!ValidateReference(upgradeManager, nameof(upgradeManager))) return;

        foreach (var upgrade in upgradeManager.Upgrades)
        {
            if (upgrade != null)
            {
                upgrade.SetPurchased(false);
                SecurePlayerPrefs.SetInt($"UpgradePurchased_{upgrade.UpgradeName}", 0);

                if (upgrade.UpgradeButton != null)
                    upgrade.UpgradeButton.interactable = true;

                if (upgrade.PriceText != null)
                    upgrade.PriceText.text = $"Цена: {upgrade.Price}";
            }
        }
        upgradeManager.RefreshUI();
    }

    private void ResetCaseItems()
    {
        if (caseItems == null || caseItems.Length == 0)
        {
            Debug.LogWarning("Массив CaseItems не назначен или пуст.");
            return;
        }

        foreach (var item in caseItems)
        {
            if (item != null)
            {
                item.isActivated = false;
                item.SaveActivationState();

                ToggleButtonVisibility(item.lockedButton, true);
                ToggleButtonVisibility(item.unlockButton, false);
            }
        }
    }

    private void ResetBackgroundData()
    {
        if (!ValidateReference(background, nameof(background)) || !ValidateReference(defaultBackground, nameof(defaultBackground))) return;

        background.sprite = defaultBackground;
        SecurePlayerPrefs.SetString("SelectedBackground", string.Empty);
        Debug.Log("Фон сброшен до значения по умолчанию.");
    }
    #endregion

    #region UI Updates
    private void UpdateScoreUI()
    {
        if (!ValidateReference(clicker, nameof(clicker))) return;
        clicker.UpdateAllScoreTexts();
    }
    #endregion

    #region Helper Methods
    private bool ValidateReference(Object obj, string fieldName)
    {
        if (obj == null)
        {
            Debug.LogWarning($"{fieldName} не назначен в инспекторе.");
            return false;
        }
        return true;
    }

    private void ToggleButtonVisibility(Button button, bool isActive)
    {
        if (button != null)
            button.gameObject.SetActive(isActive);
    }
    #endregion
}