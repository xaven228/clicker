using UnityEngine;
using UnityEngine.EventSystems;

public class ClearFocus : MonoBehaviour
{
    // Ссылка на EventSystem, используемая для снятия фокуса
    private EventSystem eventSystem;

    // Инициализация
    private void Awake()
    {
        // Инициализируем EventSystem
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("EventSystem не найден в сцене.");
        }
    }

    // Метод, вызываемый каждый кадр
    private void Update()
    {
        // Проверяем, если нажата любая клавиша или кнопка мыши
        if (Input.anyKeyDown && eventSystem != null)
        {
            RemoveFocus();
        }
    }

    // Снятие фокуса с активного объекта
    private void RemoveFocus()
    {
        eventSystem.SetSelectedGameObject(null);
    }
}
