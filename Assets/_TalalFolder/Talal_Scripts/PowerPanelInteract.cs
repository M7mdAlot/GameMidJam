using UnityEngine;
using System.Collections; // <-- Required for Coroutines (timers)

public class PowerPanelInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRadius = 2.0f;
    public Transform player;

    [Header("Door Settings")]
    public Animator[] doorAnimators; 
    public string openAnimationTrigger = "OpenDoor"; 

    // <-- NEW: Camera Cutscene Settings -->
    [Header("Cutscene Settings")]
    [Tooltip("Drag your main player camera here")]
    public GameObject playerCamera;
    
    [Tooltip("Drag the camera that looks at the doors here")]
    public GameObject doorCamera;
    
    [Tooltip("How many seconds the camera stares at the doors before switching back")]
    public float cutsceneDuration = 3.0f;

    private bool areDoorsOpen = false;

    void Start()
    {
        // Make sure the door camera is turned off when the game starts
        if (doorCamera != null)
        {
            doorCamera.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactRadius && Input.GetKeyDown(KeyCode.E) && !areDoorsOpen)
        {
            // Instead of just calling a function, we start the Timer Coroutine
            StartCoroutine(DoorCutsceneRoutine());
        }
    }

    // <-- NEW: The Cutscene Sequence -->
    private IEnumerator DoorCutsceneRoutine()
    {
        areDoorsOpen = true; // Lock the panel so they can't press E again
        
        // 1. Switch the cameras! (Disable player, enable door camera)
        if (playerCamera != null) playerCamera.SetActive(false);
        if (doorCamera != null) doorCamera.SetActive(true);

        Debug.Log("Cutscene Started: Cameras Switched.");

        // 2. Play the door animations
        foreach (Animator door in doorAnimators)
        {
            if (door != null)
            {
                door.SetTrigger(openAnimationTrigger);
            }
        }

        // 3. Wait for however many seconds you set in the Inspector
        yield return new WaitForSeconds(cutsceneDuration);

        // 4. Switch the cameras back! (Disable door, enable player camera)
        if (doorCamera != null) doorCamera.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);

        Debug.Log("Cutscene Ended: Control returned to player.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}