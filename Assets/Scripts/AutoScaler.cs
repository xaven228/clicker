using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class AutoScaler : MonoBehaviour
{
    [Header("Reference Resolution")]
    [Tooltip("Базовое разрешение для масштабирования")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);

    private CanvasScaler canvasScaler;

    void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();

        if (canvasScaler != null)
        {
            // Устанавливаем базовое разрешение
            canvasScaler.referenceResolution = referenceResolution;

            // Настраиваем режим масштабирования
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f; // 0 = только ширина, 1 = только высота, 0.5 = среднее значение

            Debug.Log("CanvasScaler настроен для автоматического масштабирования объектов UI.");
        }
        else
        {
            Debug.LogError("Компонент CanvasScaler не найден на этом объекте.");
        }
    }
}