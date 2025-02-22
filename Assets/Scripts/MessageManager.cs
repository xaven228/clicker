using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text messageText;

    [Header("Notification Settings")]
    [SerializeField] private float messageDuration = 3f;
    [SerializeField] private float delayBetweenMessages = 0.5f;

    [Header("Image Notification")]
    [SerializeField] private GameObject imageNotificationPrefab;
    [SerializeField] private Transform notificationParent;
    [SerializeField] private bool enableImageNotifications = false; // Переключатель для включения/отключения изображений

    private Queue<string> messageQueue = new Queue<string>();
    private bool isMessageShowing = false;
    private GameObject currentImageNotification;

    #region Unity Methods
    private void Start()
    {
        InitializeUI();
    }
    #endregion

    #region Initialization
    private void InitializeUI()
    {
        if (!ValidateTextComponent()) return;

        messageText.gameObject.SetActive(false);
        if (imageNotificationPrefab != null)
            imageNotificationPrefab.SetActive(false);
    }

    private bool ValidateTextComponent()
    {
        if (messageText == null)
        {
            Debug.LogError("Компонент messageText не назначен в инспекторе!");
            return false;
        }
        return true;
    }
    #endregion

    #region Message Display
    /// <summary>
    /// Добавляет сообщение в очередь для отображения.
    /// </summary>
    public void ShowMessage(string message)
    {
        if (!ValidateTextComponent()) return;

        messageQueue.Enqueue(message);
        if (!isMessageShowing)
            StartCoroutine(DisplayMessages());
    }

    private IEnumerator DisplayMessages()
    {
        isMessageShowing = true;

        while (messageQueue.Count > 0)
        {
            string currentMessage = messageQueue.Dequeue();
            DisplayCurrentMessage(currentMessage);

            yield return new WaitForSeconds(messageDuration);
            HideMessage();

            yield return new WaitForSeconds(delayBetweenMessages);
        }

        isMessageShowing = false;
    }

    private void DisplayCurrentMessage(string message)
    {
        if (enableImageNotifications && !message.StartsWith("<color="))
        {
            DisplayImageNotification(message);
        }
        else
        {
            DisplayTextMessage(message);
        }
        Debug.Log($"Сообщение отображено: {message}");
    }

    private void DisplayTextMessage(string message)
    {
        HideImageNotification();
        messageText.gameObject.SetActive(true);
        messageText.text = message;
    }

    private void DisplayImageNotification(string spriteName)
    {
        if (imageNotificationPrefab == null || notificationParent == null)
        {
            Debug.LogError("ImageNotificationPrefab или NotificationParent не назначены!");
            return;
        }

        messageText.gameObject.SetActive(false);
        HideImageNotification(); // Убираем предыдущее уведомление, если есть

        currentImageNotification = Instantiate(imageNotificationPrefab, notificationParent);
        currentImageNotification.SetActive(true);

        Image imageComponent = currentImageNotification.GetComponent<Image>();
        if (imageComponent != null)
        {
            Sprite sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");
            if (sprite != null)
                imageComponent.sprite = sprite;
            else
                Debug.LogError($"Спрайт '{spriteName}' не найден в Resources/Sprites!");
        }
    }

    private void HideMessage()
    {
        messageText.text = string.Empty;
        messageText.gameObject.SetActive(false);
        HideImageNotification();
        Debug.Log("Сообщение скрыто");
    }

    private void HideImageNotification()
    {
        if (currentImageNotification != null)
        {
            Destroy(currentImageNotification);
            currentImageNotification = null;
        }
    }
    #endregion

    #region Specialized Messages
    /// <summary>
    /// Отображает сообщение о победе.
    /// </summary>
    public void ShowWinMessage(string message)
    {
        ShowMessage($"<color=green>{message}</color>");
    }

    /// <summary>
    /// Отображает сообщение о поражении.
    /// </summary>
    public void ShowLoseMessage(string message)
    {
        ShowMessage($"<color=red>{message}</color>");
    }
    #endregion

    #region Utility
    /// <summary>
    /// Немедленно очищает очередь сообщений и скрывает текущее сообщение.
    /// </summary>
    public void ClearMessageImmediately()
    {
        StopAllCoroutines();
        messageQueue.Clear();
        HideMessage();
        isMessageShowing = false;
        Debug.Log("Очередь сообщений очищена");
    }
    #endregion
}