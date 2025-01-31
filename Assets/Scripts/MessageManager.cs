using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    // Ссылка на текстовое поле для отображения сообщений
    public Text messageText;

    // Время, через которое сообщение исчезнет (в секундах)
    public float messageDuration = 3f;

    // Метод для отображения сообщения
    public void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message; // Устанавливаем текст
            Debug.Log($"Сообщение показано: {message}"); // Логируем для отладки

            // Запускаем корутину для очистки сообщения
            StartCoroutine(ClearMessageAfterDelay());
        }
        else
        {
            Debug.LogError("Ссылка на messageText не назначена!");
        }
    }

    // Корутина для очистки сообщения через заданное время
    private IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration); // Ждём указанное время
        if (messageText != null)
        {
            messageText.text = ""; // Очищаем текст
        }
    }
}