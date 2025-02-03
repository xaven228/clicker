using UnityEngine;

public class AutoScaler : MonoBehaviour
{
    private Vector2 referenceResolution = new Vector2(1920, 1080); // Базовое разрешение для масштабирования
    
    void Start()
    {
        ScaleObjects();
    }

    void ScaleObjects()
    {
        float scaleFactorX = Screen.width / referenceResolution.x;
        float scaleFactorY = Screen.height / referenceResolution.y;
        float scaleFactor = Mathf.Min(scaleFactorX, scaleFactorY);

        // Reverting to the old method due to the lack of FindObjectSortMode in older Unity versions
        foreach (GameObject obj in FindObjectsOfType<GameObject>()) // Use the non-deprecated method
        {
            if (obj.activeInHierarchy)
            {
                obj.transform.localScale *= scaleFactor;
            }
        }
    }
}
