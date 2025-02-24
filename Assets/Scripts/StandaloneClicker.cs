using UnityEngine;
using UnityEngine.UI;

public class StandaloneClicker : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button clickButton;

    private void Start()
    {
        if (clickButton != null)
        {
            clickButton.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        // Здесь вы можете добавить код, который будет выполняться при нажатии на кнопку
        Debug.Log("Кнопка нажата!");
    }
}   