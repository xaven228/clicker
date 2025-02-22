using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class AutoScaler : MonoBehaviour
{
    [Header("Scaler Settings")]
    [Tooltip("Базовое разрешение для масштабирования UI")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);

    [Tooltip("Баланс между шириной и высотой (0 - только ширина, 1 - только высота)")]
    [Range(0f, 1f)]
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    private CanvasScaler canvasScaler;

    #region Unity Methods
    private void Awake()
    {
        InitializeScaler();
    }
    #endregion

    #region Initialization
    private void InitializeScaler()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        if (!ValidateScaler()) return;

        ConfigureScaler();
        Debug.Log($"CanvasScaler настроен: ReferenceResolution={referenceResolution}, MatchWidthOrHeight={matchWidthOrHeight}");
    }

    private bool ValidateScaler()
    {
        if (canvasScaler == null)
        {
            Debug.LogError("Компонент CanvasScaler не найден на объекте! Убедитесь, что он добавлен.");
            enabled = false;
            return false;
        }
        return true;
    }

    private void ConfigureScaler()
    {
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
    }
    #endregion
}