using UnityEngine;

public class CameraDetector : MonoBehaviour
{
    private string playerTag = "Player";
    public float detectionRadius = 0.5f;
    public float detectionRange = 15f;
    public Transform cameraEye;

    private void Update()
    {
        RaycastHit hit;
        if (Physics.SphereCast(cameraEye.position, detectionRadius, cameraEye.forward, out hit, detectionRange))
        {
            if (hit.collider.CompareTag(playerTag))
            {
                GameManager.Instance.AlertGuards();
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