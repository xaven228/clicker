using UnityEngine;
using UnityEngine.EventSystems;

public class ClearFocus : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private EventSystem eventSystem;

    #region Unity Methods
    private void Awake()
    {
        InitializeEventSystem();
    }

    private void Update()
    {
        CheckForInput();
    }
    #endregion

    #region Initialization
    private void InitializeEventSystem()
    {
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                Debug.LogError("EventSystem не найден в сцене! Убедитесь, что он присутствует.");
                enabled = false;
                return;
            }
        }
    }
    #endregion

    #region Focus Management
    private void CheckForInput()
    {
        if (Input.anyKeyDown && eventSystem != null)
        {
            RemoveFocus();
        }
    }

    /// <summary>
    /// Снимает фокус с текущего выбранного объекта в EventSystem.
    /// </summary>
    private void RemoveFocus()
    {
        eventSystem.SetSelectedGameObject(null);
        // Опционально: Debug.Log("Фокус снят с активного объекта.");
    }
    #endregion
}