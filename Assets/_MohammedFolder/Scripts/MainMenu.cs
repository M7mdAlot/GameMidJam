using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
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
