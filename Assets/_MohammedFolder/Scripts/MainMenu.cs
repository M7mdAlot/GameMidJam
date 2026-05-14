using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void SceneNext()
    {
        SceneManager.LoadScene(1);
        Debug.Log("cell");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
