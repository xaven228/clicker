using UnityEngine;
using UnityEngine.UI;

public class FireEffect : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button clickButton;
    [SerializeField] private Sprite fireSprite;
    [SerializeField] private RectTransform targetPanel;

    [Header("Effect Settings")]
    [SerializeField] private int fireSpriteCount = 10;
    [SerializeField] private float explosionForce = 10f; // Увеличена сила разброса
    [SerializeField] private float fadeOutTime = 1f;

    private void Start()
    {
        if (clickButton != null)
        {
            clickButton.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        if (fireSprite != null && targetPanel != null)
        {
            Vector2 buttonPosition = clickButton.transform.position;

            for (int i = 0; i < fireSpriteCount; i++)
            {
                GameObject fireObject = new GameObject("FireSprite");
                fireObject.transform.SetParent(targetPanel.transform, true);
                fireObject.transform.position = buttonPosition;

                fireObject.transform.localScale = Vector3.one * 2f;

                Image image = fireObject.AddComponent<Image>();
                image.sprite = fireSprite;

                Rigidbody2D rb = fireObject.AddComponent<Rigidbody2D>();
                Vector2 randomDirection = Random.insideUnitCircle.normalized * Random.Range(0.5f, 1f); // Более широкий диапазон
                rb.AddForce(randomDirection * explosionForce, ForceMode2D.Impulse);

                // Добавляем случайное вращение
                rb.angularVelocity = Random.Range(-360f, 360f);

                StartCoroutine(FadeOut(fireObject));
            }
        }
        else
        {
            Debug.LogError("Спрайт огня или панель не назначены!");
        }
    }

    private System.Collections.IEnumerator FadeOut(GameObject fireObject)
    {
        CanvasRenderer cr = fireObject.GetComponent<CanvasRenderer>();
        if (cr != null)
        {
            float timer = 0f;
            while (timer < fadeOutTime)
            {
                timer += Time.deltaTime;
                cr.SetAlpha(1f - (timer / fadeOutTime));
                yield return null;
            }
        }
        Destroy(fireObject);
    }
}