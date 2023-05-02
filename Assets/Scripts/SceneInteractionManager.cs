using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInteractionManager : MonoBehaviour
{
    private static int _currentSceneBuildIndex;

    private void Start()
    {
        _currentSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
    }

    /// <summary>
    /// Loads the first stage of the game.
    /// </summary>
    public static void StartGame()
    {
        ResetDefaults();
        SceneManager.LoadScene(_currentSceneBuildIndex + 1);
    }

    /// <summary>
    /// Reloads the current stage the player is in.
    /// </summary>
    public static void ReloadCurrentLevel()
    {
        ResetDefaults();
        SceneManager.LoadScene(_currentSceneBuildIndex);
    }

    /// <summary>
    /// Reloads the main menu from any scene.
    /// </summary>
    public static void LoadMainMenu()
    {
        ResetDefaults();
        //The first scene we build is the Main Menu which is why we load 0.
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Closes the application.
    /// </summary>
    public static void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Reset static values that are true at the beginning of each scene.
    /// </summary>
    private static void ResetDefaults()
    {
        Time.timeScale = 1.0f;
    }
}
