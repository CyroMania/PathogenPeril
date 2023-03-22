using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInteractionManager : MonoBehaviour
{
    public static void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
