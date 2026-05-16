using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event System.Action OnPlayerSpotted;
    public event System.Action<Vector3> OnCameraSpotted;

    public GameObject Player;
    public PlayerMovementNew PlayerMovement;
    public float CaughtFreezeTime = 1.5f;
    public bool CanDoubleJump = false;
    public bool CanDash = false;
    public bool CanSprint = false;

    private Vector3 checkpointPosition;
    private bool isCaught = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("CheckpointX"))
        {
            checkpointPosition = new Vector3(
                PlayerPrefs.GetFloat("CheckpointX"),
                PlayerPrefs.GetFloat("CheckpointY"),
                PlayerPrefs.GetFloat("CheckpointZ")
            );
            Player.transform.position = checkpointPosition;
        }
        else
        {
            checkpointPosition = Player.transform.position;
        }
    }

    public void AlertGuards()
    {
        OnPlayerSpotted?.Invoke();
    }

    public void AlertGuardsCamera(Vector3 spottedPosition)
    {
        OnCameraSpotted?.Invoke(spottedPosition);
    }

    public void PlayerCaught()
    {
        if (!isCaught)
            StartCoroutine(CaughtSequence());
    }

    public void SetCheckpoint()
    {
        checkpointPosition = Player.transform.position;
        PlayerPrefs.SetFloat("CheckpointX", checkpointPosition.x);
        PlayerPrefs.SetFloat("CheckpointY", checkpointPosition.y);
        PlayerPrefs.SetFloat("CheckpointZ", checkpointPosition.z);
        PlayerPrefs.Save();
    }

  private IEnumerator CaughtSequence()
    {
        isCaught = true;
        
        Debug.Log("Player Caught! Loading Scene 2...");
        
        // This instantly loads the new scene
        SceneManager.LoadScene(2);
        
        isCaught = false;
        
        yield return null; 
    }
}