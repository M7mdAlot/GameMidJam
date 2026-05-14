using UnityEngine;

public class CameraDetector : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    public float detectionRadius = 0.5f;
    public float detectionRange = 15f;
    public Transform cameraEye;
    public LayerMask playerLayer;

    private void Update()
    {
        RaycastHit hit;
        if (Physics.SphereCast(cameraEye.position, detectionRadius, cameraEye.forward, out hit, detectionRange, playerLayer))
        {
            if (hit.collider.CompareTag(playerTag))
            {
                GameManager.Instance.AlertGuardsCamera(hit.collider.transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (cameraEye == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cameraEye.position, cameraEye.position + cameraEye.forward * detectionRange);
        Gizmos.DrawWireSphere(cameraEye.position + cameraEye.forward * detectionRange, detectionRadius);
    }
}