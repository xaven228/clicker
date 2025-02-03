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

        // Используем FindObjectsByType вместо устаревшего метода
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None); // Без сортировки по InstanceID
        foreach (GameObject obj in allObjects)
        {
            if (obj.activeInHierarchy)
            {
                obj.transform.localScale *= scaleFactor;
            }
        }
    }
}
