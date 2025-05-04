using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene To Load")]
    [Tooltip("Name or index of the game scene to load")]
    public string gameSceneName = "Level0";

    public void StartGame()
    {
        Debug.Log($"Loading scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()
    {
        Debug.Log("Options button pressed (no logic implemented)");
    }

    public void ExitGame()
    {
        Debug.Log("Exit game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
