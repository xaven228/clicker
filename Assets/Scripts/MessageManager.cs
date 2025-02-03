using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text messageText;

    [Header("Settings")]
    [SerializeField] private float messageDuration = 3f;

    private Queue<string> messageQueue = new Queue<string>(); // Очередь сообщений
    private bool isMessageShowing = false; // Флаг, показывается ли сообщение

    public void ShowMessage(string message)
    {
        if (messageText == null)
        {
            Debug.LogError("Ссылка на messageText не назначена!");
            return;
        }

        messageQueue.Enqueue(message); // Добавляем сообщение в очередь

        if (!isMessageShowing)
        {
            StartCoroutine(DisplayMessages()); // Запускаем корутину
        }
    }

    private IEnumerator DisplayMessages()
    {
        isMessageShowing = true;

        while (messageQueue.Count > 0)
        {
            string currentMessage = messageQueue.Dequeue();
            messageText.text = currentMessage;
            messageText.gameObject.SetActive(true);
            Debug.Log($"Сообщение показано: {currentMessage}");

            yield return new WaitForSeconds(messageDuration); // Ждём, пока сообщение будет показано

            messageText.text = ""; // Очищаем текст
            messageText.gameObject.SetActive(false);
            Debug.Log("Сообщение очищено");

            yield return new WaitForSeconds(0.5f); // Короткая пауза перед следующим сообщением
        }

        isMessageShowing = false;
    }

    public void ClearMessageImmediately()
    {
        StopAllCoroutines();
        messageQueue.Clear();
        messageText.text = "";
        messageText.gameObject.SetActive(false);
        isMessageShowing = false;
        Debug.Log("Очередь сообщений очищена немедленно");
    }
}
