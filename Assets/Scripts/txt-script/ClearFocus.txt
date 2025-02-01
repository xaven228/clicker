using UnityEngine;
using UnityEngine.EventSystems;

public class ClearFocus : MonoBehaviour
{
    // Метод, вызываемый каждый кадр
    void Update()
    {
        // Если нажата любая клавиша или кнопка мыши, убираем фокус
        if (Input.anyKeyDown)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}