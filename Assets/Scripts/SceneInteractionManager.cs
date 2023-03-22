using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInteractionManager : MonoBehaviour
{
    private static int _currentSceneBuildIndex;

    private void Start()
    {
        _currentSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
    }

    public static void StartGame()
    {
        SceneManager.LoadScene(_currentSceneBuildIndex + 1);
    }

    //This is needed for the retry button functionalitys
    public static void ReloadCurrentLevel()
    {
        SceneManager.LoadScene(_currentSceneBuildIndex);
    }

    public static void LoadMainMenu()
    {
        //The first scene we build is the Main Menu which is why we load 0.
        SceneManager.LoadScene(0);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
