using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    // Ссылка на текстовое поле для отображения сообщений
    public Text messageText;

    // Время, в течение которого сообщение будет видно (в секундах)
    public float displayTime = 1f;

    // Метод для показа сообщения
    public void ShowMessage(string message)
    {
        if (messageText != null)
        {
            // Устанавливаем текст
            messageText.text = message;

            // Делаем текст видимым
            messageText.gameObject.SetActive(true);

            // Запускаем корутину для скрытия текста через displayTime секунд
            StartCoroutine(HideMessageAfterDelay());
        }
    }

    // Корутина для скрытия текста
    private System.Collections.IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);

        // Скрываем текст
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
}