using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    [Header("Новая система уведомлений")]
    [SerializeField] private GameObject notificationContainerPrefab;
    [SerializeField] private GameObject notificationTextPrefab;
    [SerializeField] private float newNotificationDisplayTime = 3f;

    private void Start()
    {
        // Инициализация префабов
        if (notificationContainerPrefab != null)
        {
            notificationContainerPrefab.SetActive(false);
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationContainerPrefab == null || notificationTextPrefab == null)
        {
            Debug.LogError("Префабы уведомления не назначены!");
            return;
        }

        // Активируем контейнер
        notificationContainerPrefab.SetActive(true);

        // Обновляем текст
        Text messageText = notificationTextPrefab.GetComponent<Text>();
        if (messageText != null)
        {
            messageText.text = message;
        }

        // Запускаем корутину для скрытия уведомления
        StartCoroutine(HideNotification());
    }

    private IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(newNotificationDisplayTime);
        notificationContainerPrefab.SetActive(false);
    }

    public void ShowWinMessage(string message)
    {
        ShowNotification($"<color=green>{message}</color>");
    }

    public void ShowLoseMessage(string message)
    {
        ShowNotification($"<color=red>{message}</color>");
    }

    // Добавленный метод ShowMessage
    public void ShowMessage(string message)
    {
        ShowNotification(message);
    }
}