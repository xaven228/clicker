using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonPanelPair
{
    [Header("Pair Info")]
    public string pairName; // Название пары
    public Button button; // Кнопка, которая будет управлять панелями
    public GameObject[] panelsToOpen; // Панели, которые нужно открыть
    public GameObject[] panelsToClose; // Панели, которые нужно закрыть
}

public class PanelManager : MonoBehaviour
{
    [Header("Button-Panel Configuration")]
    [SerializeField] private ButtonPanelPair[] buttonPanelPairs; // Массив пар "кнопка - панель"

    [Header("Default Panel")]
    [SerializeField] private GameObject defaultPanel; // Ссылка на панель, которая должна быть открыта при старте

    // Метод, вызываемый при старте игры
    private void Start()
    {
        if (buttonPanelPairs == null || buttonPanelPairs.Length == 0)
        {
            Debug.LogError("Массив buttonPanelPairs пуст! Добавьте пары в инспекторе.");
            return;
        }

        // Открытие панели по умолчанию
        OpenDefaultPanel();

        // Назначение обработчиков кнопок
        AssignButtonListeners();
    }

    // Метод для открытия панели по умолчанию
    private void OpenDefaultPanel()
    {
        // Закрываем все панели перед открытием дефолтной
        SetPanelsActiveState(false);

        if (defaultPanel == null)
        {
            Debug.LogError("Дефолтная панель не назначена!");
            return;
        }

        // Открываем дефолтную панель
        defaultPanel.SetActive(true);
    }

    // Метод для обработки нажатия на кнопку
    private void HandleButtonClick(ButtonPanelPair pair)
    {
        Debug.Log($"Нажата кнопка из пары: {pair.pairName}");

        // Открываем все панели из массива panelsToOpen
        TogglePanelsState(pair.panelsToOpen, true);
        
        // Закрываем все панели из массива panelsToClose
        TogglePanelsState(pair.panelsToClose, false);
    }

    // Метод для установки состояний всех панелей
    private void SetPanelsActiveState(bool state)
    {
        foreach (var pair in buttonPanelPairs)
        {
            if (pair.panelsToOpen != null)
            {
                foreach (var panel in pair.panelsToOpen)
                {
                    if (panel != null) panel.SetActive(state);
                }
            }
        }
    }

    // Метод для переключения состояния панелей
    private void TogglePanelsState(GameObject[] panels, bool state)
    {
        if (panels == null) return;

        foreach (var panel in panels)
        {
            if (panel != null)
            {
                bool isCurrentlyActive = panel.activeSelf;
                if (isCurrentlyActive != state) // Проверяем, нужно ли изменять состояние
                {
                    panel.SetActive(state);
                }
            }
        }
    }

    // Метод для назначения обработчиков событий для кнопок
    private void AssignButtonListeners()
    {
        foreach (var pair in buttonPanelPairs)
        {
            if (pair.button == null)
            {
                Debug.LogError($"Кнопка не назначена для пары: {pair.pairName}");
                continue; // Пропускаем эту пару
            }

            pair.button.onClick.AddListener(() => HandleButtonClick(pair));
        }
    }
}
