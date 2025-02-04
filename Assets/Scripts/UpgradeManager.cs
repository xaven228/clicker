using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Upgrade
{
    public string upgradeName;
    public int price;
    public float bonusMultiplier;
    public Button upgradePrefab;
    public Button upgradeButton;
    public Text priceText;

    public bool isPurchased = false;
}

public class UpgradeManager : MonoBehaviour
{
    public Clicker clicker;
    public MessageManager messageManager;
    public Upgrade[] upgrades;

    private const string PURCHASED_KEY_PREFIX = "UpgradePurchased_";

    void Start()
    {
        InitializeUpgrades();
    }

    private void InitializeUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade.upgradeButton != null && upgrade.priceText != null)
            {
                LoadUpgradeState(upgrade);

                if (!upgrade.isPurchased)
                {
                    upgrade.upgradeButton.onClick.AddListener(() => TryPurchaseUpgrade(upgrade));
                }
            }
        }
    }
    public void RefreshUI()
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade.upgradeButton != null)
                upgrade.upgradeButton.interactable = !upgrade.isPurchased;

            if (upgrade.priceText != null)
                upgrade.priceText.text = upgrade.isPurchased ? "Куплено!" : $"Цена: {upgrade.price}";
        }
    }
    public void LoadUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            LoadUpgradeState(upgrade);
        }
    }

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

    private void TryPurchaseUpgrade(Upgrade upgrade)
    {
        int currentClicks = Clicker.GetClickCount();

        if (currentClicks >= upgrade.price && !upgrade.isPurchased)
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
        Clicker.SetClickCount(currentClicks - upgrade.price);
        clicker.UpdateAllScoreTexts();

        // Изменённый метод: теперь множитель умножается
        Clicker.MultiplyGlobalMultiplier(upgrade.bonusMultiplier);

        InstantiateUpgradeButton(upgrade);

        upgrade.upgradeButton.interactable = false;
        upgrade.priceText.text = "Куплено!";

        upgrade.isPurchased = true;
        string key = PURCHASED_KEY_PREFIX + upgrade.upgradeName;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        Debug.Log($"Сохранено улучшение: {key}");

        ShowUpgradeMessage();
    }

    private void InstantiateUpgradeButton(Upgrade upgrade)
    {
        if (upgrade.upgradePrefab != null)
        {
            Button instantiatedButton = Instantiate(upgrade.upgradePrefab, upgrade.upgradeButton.transform);
            instantiatedButton.transform.localPosition = Vector3.zero;
            instantiatedButton.interactable = false;
        }
    }

    private void UpdatePriceText(Upgrade upgrade)
    {
        if (upgrade.priceText != null)
        {
            upgrade.priceText.text = $"Цена: {upgrade.price}";
        }
    }

    private void MarkAsPurchased(Upgrade upgrade)
    {
        upgrade.upgradeButton.interactable = false;
        upgrade.priceText.text = "Куплено!";
        InstantiateUpgradeButton(upgrade);
    }

    private void ShowInsufficientFundsMessage()
    {
        if (messageManager != null)
        {
            messageManager.ShowMessage("Недостаточно средств");
        }
    }

    private void ShowUpgradeMessage()
    {
        if (messageManager != null)
        {
            string message = $"Поздравляем с улучшением! Клики теперь умножаются в {Clicker.GetGlobalMultiplier():F1} раз";
            messageManager.ShowMessage(message);
        }
    }
}
