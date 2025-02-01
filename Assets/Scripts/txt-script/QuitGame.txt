using UnityEngine;

public class QuitGame : MonoBehaviour
{
    // Метод для закрытия игры
    public void ExitGame()
    {
        Debug.Log("Игра закрывается...");
        Application.Quit();

        // Если вы тестируете в редакторе Unity, используйте это:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}