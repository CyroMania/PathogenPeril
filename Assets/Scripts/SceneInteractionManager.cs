using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInteractionManager : MonoBehaviour
{
    public static void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
