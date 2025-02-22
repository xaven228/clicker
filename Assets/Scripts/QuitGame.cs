using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuitGame : MonoBehaviour
{
    #region Public Methods
    /// <summary>
    /// Закрывает приложение или останавливает воспроизведение в редакторе Unity.
    /// </summary>
    public void ExitGame()
    {
        LogExitMessage();
        PerformQuit();
    }
    #endregion

    #region Private Methods
    private void LogExitMessage()
    {
        Debug.Log("Закрытие игры инициировано...");
    }

    private void PerformQuit()
    {
        // Выход из приложения для билда
        Application.Quit();

        // Остановка воспроизведения в редакторе Unity
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
    #endregion
}