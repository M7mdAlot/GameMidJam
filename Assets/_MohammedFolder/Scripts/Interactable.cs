using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("How close the player needs to be to interact")]
    public float interactRadius = 2.0f; // Increased to 2 meters so it's easier to hit!
    public Transform player;

    [Header("Events")]
    public UnityEvent onInteract;
    public UnityEvent onUninteract;

    private bool isInteracting = false;

    void Update()
    {
        // Safety check to prevent errors if you forget to assign the player
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Replaced New Input System with standard KeyCode.E
        if (distance <= interactRadius && Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            Debug.Log("Interact triggered!");
            isInteracting = true;
            onInteract.Invoke();
        }

        // Added KeyCode.Q to uninteract (you can change this to Esc or whatever you like)
        if (Input.GetKeyDown(KeyCode.Q) && isInteracting)
        {
            Debug.Log("Uninteract triggered!");
            isInteracting = false;
            onUninteract.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // This draws a yellow circle around the item in the editor so you can see the radius size
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}