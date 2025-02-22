using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonPanelPair
{
    [Header("Pair Configuration")]
    [SerializeField] private string pairName = "Unnamed Pair";
    [SerializeField] private Button button;
    [SerializeField] private GameObject[] panelsToOpen = new GameObject[0];
    [SerializeField] private GameObject[] panelsToClose = new GameObject[0];

    public string PairName => pairName;
    public Button Button => button;
    public GameObject[] PanelsToOpen => panelsToOpen;
    public GameObject[] PanelsToClose => panelsToClose;
}

public class PanelManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ButtonPanelPair[] buttonPanelPairs = new ButtonPanelPair[0];

    [Header("Default Settings")]
    [SerializeField] private GameObject defaultPanel;

    #region Unity Methods
    private void Start()
    {
        InitializePanelManager();
    }
    #endregion

    #region Initialization
    private void InitializePanelManager()
    {
        if (!ValidatePairs())
            return;

        OpenDefaultPanel();
        AssignButtonListeners();
    }

    private bool ValidatePairs()
    {
        if (buttonPanelPairs.Length == 0)
        {
            Debug.LogError("Массив ButtonPanelPairs пуст! Настройте пары в инспекторе.");
            return false;
        }
        return true;
    }

    private void OpenDefaultPanel()
    {
        if (!ValidateDefaultPanel())
            return;

        SetAllPanelsActive(false);
        defaultPanel.SetActive(true);
    }

    private bool ValidateDefaultPanel()
    {
        if (defaultPanel == null)
        {
            Debug.LogError("Дефолтная панель не назначена в инспекторе!");
            return false;
        }
        return true;
    }
    #endregion

    #region Panel Management
    private void SetAllPanelsActive(bool state)
    {
        foreach (var pair in buttonPanelPairs)
        {
            TogglePanels(pair.PanelsToOpen, state);
            TogglePanels(pair.PanelsToClose, state);
        }
    }

    private void TogglePanels(GameObject[] panels, bool state)
    {
        if (panels == null) return;

        foreach (var panel in panels)
        {
            if (panel != null && panel.activeSelf != state)
            {
                panel.SetActive(state);
            }
        }
    }

    private void HandleButtonClick(ButtonPanelPair pair)
    {
        Debug.Log($"Активирована пара: {pair.PairName}");
        TogglePanels(pair.PanelsToOpen, true);
        TogglePanels(pair.PanelsToClose, false);
    }
    #endregion

    #region Button Setup
    private void AssignButtonListeners()
    {
        foreach (var pair in buttonPanelPairs)
        {
            if (!ValidateButton(pair))
                continue;

            pair.Button.onClick.RemoveAllListeners(); // Очищаем старые слушатели
            pair.Button.onClick.AddListener(() => HandleButtonClick(pair));
        }
    }

    private bool ValidateButton(ButtonPanelPair pair)
    {
        if (pair.Button == null)
        {
            Debug.LogError($"Кнопка не назначена для пары: {pair.PairName}");
            return false;
        }
        return true;
    }
    #endregion
}