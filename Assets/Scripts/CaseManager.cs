using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Добавлено для LINQ (Sum)

public class CaseManager : MonoBehaviour
{
    [Header("Case Settings")]
    [SerializeField] private List<CaseItem> caseItems = new List<CaseItem>();
    [SerializeField] private int totalItemsCount = 1000;
    [SerializeField] private int clickCost = 10;
    [SerializeField] private float itemSpacing = 100f;

    [Header("UI Elements")]
    [SerializeField] private RectTransform roulettePanel;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject roulettePanelUI;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject arrowSprite;
    [SerializeField] private float arrowOffsetY = -50f;

    [Header("Dependencies")]
    [SerializeField] private MessageManager messageManager;
    [SerializeField] private AchievementManager achievementManager;

    private static CaseManager instance;
    private Clicker clicker;
    private List<GameObject> itemObjects = new List<GameObject>();
    private List<CaseItem> rouletteItems = new List<CaseItem>();

    public static CaseManager Instance => instance;

    #region Unity Methods
    private void Awake()
    {
        SetupSingleton();
    }

    private void Start()
    {
        InitializeCaseManager();
    }

    private void OnDestroy()
    {
        Clicker.OnClickAdded -= HandleClickAdded;
    }
    #endregion

    #region Initialization
    private void SetupSingleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeCaseManager()
    {
        if (!ValidateComponents()) return;

        LoadSelectedBackground();
        clicker = FindFirstObjectByType<Clicker>();
        Clicker.OnClickAdded += HandleClickAdded;

        startButton.onClick.AddListener(StartRoulette);
        closeButton.onClick.AddListener(CloseRoulettePanel);
        roulettePanelUI.SetActive(false);
        closeButton.gameObject.SetActive(false);

        PositionArrow();
        LoadUnlockButtonsState();
        SetupItemActivationListeners();
    }

    private bool ValidateComponents()
    {
        if (roulettePanel == null) { Debug.LogError("RoulettePanel не назначен!"); enabled = false; return false; }
        if (itemPrefab == null) { Debug.LogError("ItemPrefab не назначен!"); enabled = false; return false; }
        if (backgroundImage == null) { Debug.LogError("BackgroundImage не назначен!"); enabled = false; return false; }
        if (startButton == null) { Debug.LogError("StartButton не назначен!"); enabled = false; return false; }
        if (roulettePanelUI == null) { Debug.LogError("RoulettePanelUI не назначен!"); enabled = false; return false; }
        if (closeButton == null) { Debug.LogError("CloseButton не назначен!"); enabled = false; return false; }
        if (arrowSprite == null) { Debug.LogError("ArrowSprite не назначен!"); enabled = false; return false; }
        if (messageManager == null && !TryGetComponent(out messageManager)) { Debug.LogError("MessageManager не назначен!"); enabled = false; return false; }
        if (achievementManager == null) achievementManager = FindFirstObjectByType<AchievementManager>();
        return true;
    }

    private void LoadSelectedBackground()
    {
        string backgroundName = SecurePlayerPrefs.GetString("SelectedBackground", string.Empty);
        if (!string.IsNullOrEmpty(backgroundName))
        {
            CaseItem selectedItem = caseItems.Find(item => item.ItemName == backgroundName);
            if (selectedItem != null)
                ChangeBackground(selectedItem.BackgroundSprite);
        }
    }

    private void PositionArrow()
    {
        arrowSprite.transform.position = new Vector3(roulettePanel.position.x, roulettePanel.position.y + arrowOffsetY, roulettePanel.position.z);
    }
    #endregion

    #region Roulette Logic
    public void StartRoulette()
    {
        if (Clicker.GetClickCount() < clickCost)
        {
            messageManager.ShowMessage("Недостаточно кликов для запуска рулетки!");
            return;
        }

        Clicker.RemoveClicks(clickCost);
        ResetRoulettePanel();
        CreateNewRoulettePanel();
        StartCoroutine(SpinRoulette());
        ToggleUI(true);
    }

    private void ResetRoulettePanel()
    {
        roulettePanel.localPosition = Vector3.zero;
    }

    private void CreateNewRoulettePanel()
    {
        ClearRouletteItems();
        PopulateRouletteItems();
        SpawnRouletteItems();
    }

    private void ClearRouletteItems()
    {
        foreach (var itemObject in itemObjects)
            Destroy(itemObject);
        itemObjects.Clear();
        rouletteItems.Clear();
    }

    private void PopulateRouletteItems()
    {
        for (int i = 0; i < totalItemsCount; i++)
            rouletteItems.Add(GetRandomItemBasedOnChance());
    }

    private void SpawnRouletteItems()
    {
        for (int i = 0; i < totalItemsCount; i++)
        {
            GameObject itemObj = Instantiate(itemPrefab, roulettePanel);
            itemObj.transform.localPosition = new Vector3(i * itemSpacing, 0, 0);
            itemObj.GetComponent<Image>().sprite = rouletteItems[i].ItemSprite;
            itemObjects.Add(itemObj);
        }
    }

    private CaseItem GetRandomItemBasedOnChance()
    {
        if (caseItems.Count == 0)
        {
            Debug.LogError("Список caseItems пуст!");
            return null;
        }

        float totalChance = caseItems.Sum(item => item?.SpawnChance ?? 0); // Исправлено с System.Linq
        float randomValue = Random.Range(0f, totalChance);
        float currentChance = 0f;

        foreach (var item in caseItems)
        {
            if (item == null) continue;
            currentChance += item.SpawnChance;
            if (randomValue <= currentChance)
                return item;
        }
        return caseItems[0]; // Fallback
    }

    private IEnumerator SpinRoulette()
    {
        float spinDuration = Random.Range(2f, 4f);
        float elapsedTime = 0f;
        float startPosition = roulettePanel.localPosition.x;
        float targetPosition = startPosition - (totalItemsCount * itemSpacing);

        while (elapsedTime < spinDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / spinDuration);
            roulettePanel.localPosition = new Vector3(Mathf.Lerp(startPosition, targetPosition, t), 0, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        roulettePanel.localPosition = new Vector3(targetPosition, 0, 0);
        int winningIndex = GetClosestItemToArrow();
        HandleSpinResult(winningIndex);
        StartCoroutine(AnimateWinningItem(itemObjects[winningIndex]));
        closeButton.gameObject.SetActive(true);
    }

    private int GetClosestItemToArrow()
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;
        Vector3 arrowPosition = arrowSprite.transform.position;

        for (int i = 0; i < itemObjects.Count; i++)
        {
            float distance = Mathf.Abs(itemObjects[i].transform.position.x - arrowPosition.x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private void HandleSpinResult(int winningIndex)
    {
        CaseItem resultItem = rouletteItems[winningIndex];
        resultItem.LoadActivationState();

        if (resultItem.IsActivated)
        {
            int previousClicks = Clicker.GetClickCount();
            Clicker.AddClicks(resultItem.BonusClicks);
            int addedClicks = Clicker.GetClickCount() - previousClicks;
            messageManager.ShowMessage($"Предмет открыт, вы получили бонус: {addedClicks} кликов.");
            achievementManager?.CheckAchievements(addedClicks);
        }
        else
        {
            resultItem.IsActivated = true;
            resultItem.SaveActivationState();
            ChangeBackground(resultItem.BackgroundSprite);
            EnableUnlockButton(resultItem);
            messageManager.ShowMessage($"Поздравляем! Вы выиграли: {resultItem.ItemName}");
        }
        clicker.UpdateAllScoreTexts();
    }
    #endregion

    #region UI Management
    private void ToggleUI(bool isSpinning)
    {
        roulettePanelUI.SetActive(isSpinning);
        startButton.gameObject.SetActive(!isSpinning);
    }

    private void CloseRoulettePanel()
    {
        ToggleUI(false);
        closeButton.gameObject.SetActive(false);
    }

    private void EnableUnlockButton(CaseItem item)
    {
        ToggleButton(item.UnlockButton, true);
        ToggleButton(item.LockedButton, false);
        SecurePlayerPrefs.SetInt($"UnlockState_{item.ItemName}", 1);
    }

    private void LoadUnlockButtonsState()
    {
        foreach (var item in caseItems)
        {
            bool isUnlocked = SecurePlayerPrefs.GetInt($"UnlockState_{item.ItemName}", 0) == 1;
            ToggleButton(item.UnlockButton, isUnlocked);
            ToggleButton(item.LockedButton, !isUnlocked);
        }
    }

    private void ToggleButton(Button button, bool isActive)
    {
        if (button != null)
        {
            button.interactable = isActive;
            button.gameObject.SetActive(isActive);
        }
    }

    private void SetupItemActivationListeners()
    {
        foreach (var item in caseItems)
        {
            if (item.UnlockButton != null)
                item.UnlockButton.onClick.AddListener(() => item.ActivateItem());
        }
    }
    #endregion

    #region Animation
    private IEnumerator AnimateWinningItem(GameObject winningItem)
    {
        Vector3 originalScale = winningItem.transform.localScale;
        Vector3 enlargedScale = originalScale * 1.2f;
        float duration = 0.5f;

        yield return ScaleObject(winningItem, originalScale, enlargedScale, duration);
        yield return ScaleObject(winningItem, enlargedScale, originalScale, duration);
    }

    private IEnumerator ScaleObject(GameObject obj, Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        obj.transform.localScale = endScale;
    }
    #endregion

    #region Background Management
    public void ChangeBackground(Sprite newBackground)
    {
        if (backgroundImage != null && newBackground != null)
            backgroundImage.sprite = newBackground;
        else
            Debug.LogWarning("Не удалось изменить фон: BackgroundImage или новый фон не назначены.");
    }
    #endregion

    #region Event Handlers
    private void HandleClickAdded(int amount)
    {
        Debug.Log($"Клики изменены на {amount}. Текущее количество: {Clicker.GetClickCount()}");
    }
    #endregion
}

[System.Serializable]
public class CaseItem
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemSprite;
    [SerializeField, Range(0f, 1f)] private float spawnChance;
    [SerializeField] public Button lockedButton;
    [SerializeField] public Button unlockButton;
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private int bonusClicks;
    [SerializeField] public bool isActivated;

    public string ItemName => itemName;
    public Sprite ItemSprite => itemSprite;
    public float SpawnChance => spawnChance;
    public Button LockedButton => lockedButton;
    public Button UnlockButton => unlockButton;
    public Sprite BackgroundSprite => backgroundSprite;
    public int BonusClicks => bonusClicks;
    public bool IsActivated { get => isActivated; set => isActivated = value; }

    public void LoadActivationState()
    {
        IsActivated = SecurePlayerPrefs.GetInt($"item_{ItemName}", 0) == 1;
    }

    public void SaveActivationState()
    {
        SecurePlayerPrefs.SetInt($"item_{ItemName}", IsActivated ? 1 : 0);
    }

    public void ActivateItem()
    {
        if (BackgroundSprite != null)
        {
            CaseManager.Instance.ChangeBackground(BackgroundSprite);
            SecurePlayerPrefs.SetString("SelectedBackground", ItemName);
        }
        else
        {
            Debug.LogWarning($"BackgroundSprite не задан для предмета '{ItemName}'!");
        }
    }
}