using UnityEngine;

public class WinningFloorTrigger : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Drag your Winning Canvas Panel here")]
    public GameObject winningUIPanel;

    void Start()
    {
        // Automatically hide the UI when the game starts just in case you left it on
        if (winningUIPanel != null)
        {
            winningUIPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object stepping on the floor is the Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player stepped on the Winning Floor! Game Over/Win!");
            
            if (winningUIPanel != null)
            {
                // Turn on the UI Canvas
                winningUIPanel.SetActive(true);
                
                // Freeze the game time
                Time.timeScale = 0f;
                
                // Unlock and show the mouse cursor so they can click menu buttons
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}