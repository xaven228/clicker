using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Upgrade
{
    [SerializeField] private string upgradeName;
    [SerializeField] private int price;
    [SerializeField] private float bonusMultiplier;
    [SerializeField] private Button upgradePrefab;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Text priceText;

    public bool IsPurchased { get; private set; } = false;

    public string UpgradeName => upgradeName;
    public int Price => price;
    public float BonusMultiplier => bonusMultiplier;
    public Button UpgradeButton => upgradeButton;
    public Text PriceText => priceText;
    public Button UpgradePrefab => upgradePrefab;

    public void SetPurchased(bool value) => IsPurchased = value;
}

public class UpgradeManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Clicker clicker;
    [SerializeField] private MessageManager messageManager;

    [Header("Upgrades")]
    [SerializeField] private Upgrade[] upgrades;

    public Upgrade[] Upgrades => upgrades; // Добавлено публичное свойство

    private const string PURCHASED_KEY_PREFIX = "UpgradePurchased_";

    private void Start()
    {
        InitializeUpgrades();
    }

    #region Initialization
    private void InitializeUpgrades()
    {
        if (upgrades == null || upgrades.Length == 0)
        {
            Debug.LogWarning("Массив улучшений пуст!");
            return;
        }

        foreach (var upgrade in upgrades)
        {
            ValidateUpgradeComponents(upgrade);
            LoadUpgradeState(upgrade);
            SetupButtonListener(upgrade);
        }
        RefreshUI();
    }

    private void ValidateUpgradeComponents(Upgrade upgrade)
    {
        if (upgrade.UpgradeButton == null) Debug.LogWarning($"Кнопка не назначена для улучшения {upgrade.UpgradeName}");
        if (upgrade.PriceText == null) Debug.LogWarning($"Текст цены не назначен для улучшения {upgrade.UpgradeName}");
    }

    private void SetupButtonListener(Upgrade upgrade)
    {
        if (upgrade.UpgradeButton != null && !upgrade.IsPurchased)
        {
            upgrade.UpgradeButton.onClick.RemoveAllListeners();
            upgrade.UpgradeButton.onClick.AddListener(() => TryPurchaseUpgrade(upgrade));
        }
    }
    #endregion

    #region UI Management
    public void RefreshUI()
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade.UpgradeButton != null)
                upgrade.UpgradeButton.interactable = !upgrade.IsPurchased;

            if (upgrade.PriceText != null)
                upgrade.PriceText.text = upgrade.IsPurchased ? "Куплено!" : $"Цена: {upgrade.Price}";
        }
    }
    #endregion

    #region Save/Load
    public void LoadUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            LoadUpgradeState(upgrade);
        }
    }

    private void LoadUpgradeState(Upgrade upgrade)
    {
        string key = PURCHASED_KEY_PREFIX + upgrade.UpgradeName;
        bool isPurchased = SecurePlayerPrefs.GetInt(key, 0) == 1;

        if (isPurchased)
        {
            MarkAsPurchased(upgrade);
        }
        else
        {
            UpdatePriceText(upgrade);
        }

        Debug.Log($"Загружено улучшение: {key}, куплено: {isPurchased}");
    }
    #endregion

    #region Purchase Logic
    private void TryPurchaseUpgrade(Upgrade upgrade)
    {
        int currentClicks = Clicker.GetClickCount();

        if (currentClicks >= upgrade.Price && !upgrade.IsPurchased)
        {
            PurchaseUpgrade(upgrade, currentClicks);
        }
        else
        {
            ShowInsufficientFundsMessage();
        }
    }

    private void PurchaseUpgrade(Upgrade upgrade, int currentClicks)
    {
        // Обновление кликов и множителя
        Clicker.SetClickCount(currentClicks - upgrade.Price);
        clicker.UpdateAllScoreTexts();
        Clicker.MultiplyGlobalMultiplier(upgrade.BonusMultiplier);

        // Обновление UI и состояния
        upgrade.SetPurchased(true);
        upgrade.UpgradeButton.interactable = false;
        upgrade.PriceText.text = "Куплено!";
        InstantiateUpgradeButton(upgrade);

        // Сохранение
        string key = PURCHASED_KEY_PREFIX + upgrade.UpgradeName;
        SecurePlayerPrefs.SetInt(key, 1);
        Debug.Log($"Сохранено улучшение: {key}");

        ShowUpgradeMessage();
    }

    private void InstantiateUpgradeButton(Upgrade upgrade)
    {
        if (upgrade.UpgradePrefab != null)
        {
            Button instantiatedButton = Instantiate(upgrade.UpgradePrefab, upgrade.UpgradeButton.transform);
            instantiatedButton.transform.localPosition = Vector3.zero;
            instantiatedButton.interactable = false;
        }
    }

    private void UpdatePriceText(Upgrade upgrade)
    {
        if (upgrade.PriceText != null)
        {
            upgrade.PriceText.text = $"Цена: {upgrade.Price}";
        }
    }

    private void MarkAsPurchased(Upgrade upgrade)
    {
        upgrade.SetPurchased(true);
        upgrade.UpgradeButton.interactable = false;
        upgrade.PriceText.text = "Куплено!";
        InstantiateUpgradeButton(upgrade);
    }
    #endregion

    #region Messages
    private void ShowInsufficientFundsMessage()
    {
        messageManager?.ShowMessage("Недостаточно средств");
    }

    private void ShowUpgradeMessage()
    {
        if (messageManager != null)
        {
            string message = $"Поздравляем с улучшением! Клики теперь умножаются в {Clicker.GetGlobalMultiplier():F1} раз";
            messageManager.ShowMessage(message);
        }
    }
    #endregion
}