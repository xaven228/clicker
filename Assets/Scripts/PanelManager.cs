using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonPanelPair
{
    public string pairName; // Название пары
    public Button button; // Кнопка
    public GameObject[] panelsToOpen; // Панели для открытия
    public GameObject[] panelsToClose; // Панели для закрытия
}

public class PanelManager : MonoBehaviour
{
    // Массив пар "кнопка - панель"
    public ButtonPanelPair[] buttonPanelPairs;

    // Ссылка на панель, которая должна быть открыта при старте
    public GameObject defaultPanel;

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Проверяем, что массив пар не пуст
        if (buttonPanelPairs == null || buttonPanelPairs.Length == 0)
        {
            Debug.LogError("Массив buttonPanelPairs пуст! Добавьте пары в инспекторе.");
            return;
        }

        // Открываем только одну панель по умолчанию
        OpenDefaultPanel();

        // Проходим по всем парам кнопок и панелей
        foreach (var pair in buttonPanelPairs)
        {
            // Проверяем, что кнопка назначена
            if (pair.button == null)
            {
                Debug.LogError($"Кнопка не назначена для пары: {pair.pairName}");
                continue; // Пропускаем эту пару
            }

            // Назначаем обработчик события для каждой кнопки
            pair.button.onClick.AddListener(() => HandleButtonClick(pair));
        }
    }

    // Метод для открытия панели по умолчанию
    private void OpenDefaultPanel()
    {
        // Закрываем все панели
        foreach (var pair in buttonPanelPairs)
        {
            if (pair.panelsToOpen != null)
            {
                foreach (var panel in pair.panelsToOpen)
                {
                    if (panel != null) panel.SetActive(false);
                }
            }
        }

        // Проверяем, что дефолтная панель назначена
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