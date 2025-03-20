using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasAutoScaler : MonoBehaviour
{
    [Header("Scaler Settings")]
    [Tooltip("Target resolution for scaling the UI")]
    [SerializeField] private Vector2 targetResolution = new Vector2(1920, 1080);

    [Tooltip("Ratio for matching width or height (0 for width, 1 for height)")]
    [Range(0f, 1f)]
    [SerializeField] private float widthHeightRatio = 0.5f;

    private CanvasScaler canvasScaler;

    private void Awake()
    {
        // Initialize the CanvasScaler component
        canvasScaler = GetComponent<CanvasScaler>();

        if (canvasScaler != null)
        {
            ConfigureCanvasScaler();
        }
        else
        {
            Debug.LogError("CanvasScaler component is missing!");
        }
    }

    private void ConfigureCanvasScaler()
    {
        // Set the UI scale mode to scale with screen size
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Set the reference resolution and scaling settings
        canvasScaler.referenceResolution = targetResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = widthHeightRatio;

        Debug.Log($"CanvasScaler configured: Resolution={targetResolution}, Ratio={widthHeightRatio}");
    }
}
