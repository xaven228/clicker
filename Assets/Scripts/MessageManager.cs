using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    // Ссылка на текстовое поле для отображения сообщений
    [Header("UI Elements")]
    [SerializeField] private Text messageText;

    // Время, через которое сообщение исчезнет (в секундах)
    [Header("Settings")]
    [SerializeField] private float messageDuration = 3f;

    // Метод для отображения сообщения
    public void ShowMessage(string message)
    {
        if (messageText == null)
        {
            Debug.LogError("Ссылка на messageText не назначена!");
            return;
        }

        // Устанавливаем текст и логируем для отладки
        messageText.text = message;
        Debug.Log($"Сообщение показано: {message}");

        // Запускаем корутину для очистки сообщения
        StopAllCoroutines(); // Останавливаем любые текущие корутины очистки
        StartCoroutine(ClearMessageAfterDelay());
    }

    // Корутина для очистки сообщения через заданное время
    private IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration); // Ждем указанное время
        if (messageText != null)
        {
            messageText.text = ""; // Очищаем текст сообщения
            Debug.Log("Сообщение очищено");
        }
    }

    // (Опционально) Метод для немедленного очистки текста
    public void ClearMessageImmediately()
    {
        if (messageText != null)
        {
            messageText.text = ""; // Немедленно очищаем текст
            Debug.Log("Сообщение очищено немедленно");
        }
    }
}
