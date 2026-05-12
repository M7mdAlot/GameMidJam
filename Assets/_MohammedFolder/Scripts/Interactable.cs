using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRadius = 0.25f;
    public Transform player;

    [Header("Events")]
    public UnityEvent onInteract;
    public UnityEvent onUninteract;

    private InputAction interactAction;
    private InputAction uninteractAction;
    private bool isInteracting = false;

    void Start()
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
        uninteractAction = playerInput.actions["Uninteract"];
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);


        if (distance <= interactRadius && interactAction.WasPressedThisFrame() && !isInteracting)
        {
            Debug.Log("Interact");
            isInteracting = true;
            onInteract.Invoke();
        }

        if (uninteractAction.WasPressedThisFrame() && isInteracting)
        {
            Debug.Log("Uninteract");
            isInteracting = false;
            onUninteract.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}