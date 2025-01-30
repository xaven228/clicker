using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonPanelPair
{
    public string pairName; // Название пары (отображается в инспекторе)
    public Button button; // Кнопка
    public GameObject[] panelsToOpen; // Панели для открытия
    public GameObject[] panelsToClose; // Панели для закрытия
}

public class PanelManager : MonoBehaviour
{
    // Массив пар "кнопка - панель"
    public ButtonPanelPair[] buttonPanelPairs;

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Проходим по всем парам кнопок и панелей
        foreach (var pair in buttonPanelPairs)
        {
            // Назначаем обработчик события для каждой кнопки
            pair.button.onClick.AddListener(() => HandleButtonClick(pair));
        }
    }

    // Метод для обработки нажатия на кнопку
    private void HandleButtonClick(ButtonPanelPair pair)
    {
        // Логируем имя пары для отладки
        Debug.Log($"Нажата кнопка из пары: {pair.pairName}");

        // Открываем все панели из массива panelsToOpen
        if (pair.panelsToOpen != null)
        {
            foreach (var panel in pair.panelsToOpen)
            {
                if (panel != null) panel.SetActive(true);
            }
        }

        // Закрываем все панели из массива panelsToClose
        if (pair.panelsToClose != null)
        {
            foreach (var panel in pair.panelsToClose)
            {
                if (panel != null) panel.SetActive(false);
            }
        }
    }
}