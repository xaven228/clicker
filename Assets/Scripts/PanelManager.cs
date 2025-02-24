using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

[System.Serializable]
public class RightClickPanelPair
{
    [Header("Right Click Pair Configuration")]
    [SerializeField] private string pairName = "Unnamed Right Click Pair";
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
    [SerializeField] private RightClickPanelPair[] rightClickPanelPairs = new RightClickPanelPair[0];

    [Header("Default Settings")]
    [SerializeField] private GameObject defaultPanel;

    #region Unity Methods
    private void Start()
    {
        InitializePanelManager();
    }

    private void Update()
    {
        CheckRightMouseClick();
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
        if (buttonPanelPairs.Length == 0 && rightClickPanelPairs.Length == 0)
        {
            Debug.LogError("Массивы ButtonPanelPairs и RightClickPanelPairs пусты! Настройте пары в инспекторе.");
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
        foreach (var pair in rightClickPanelPairs)
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

    private void HandleRightClick(RightClickPanelPair pair)
    {
        Debug.Log($"Активирована пара ПКМ: {pair.PairName}");
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

            pair.Button.onClick.RemoveAllListeners();
            pair.Button.onClick.AddListener(() => HandleButtonClick(pair));
        }

        foreach (var pair in rightClickPanelPairs)
        {
            if (pair.Button == null)
            {
                Debug.LogError($"Кнопка не назначена для пары ПКМ: {pair.PairName}");
                continue;
            }
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

    #region Right Mouse Click Setup
    private void CheckRightMouseClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            foreach (var pair in rightClickPanelPairs)
            {
                if (pair.Button != null && IsPointerOverUIObject(pair.Button.gameObject))
                {
                    HandleRightClick(pair);
                }
            }
        }
    }

    private bool IsPointerOverUIObject(GameObject uiObject)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.mousePosition;
        System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Exists(result => result.gameObject == uiObject);
    }
    #endregion
}