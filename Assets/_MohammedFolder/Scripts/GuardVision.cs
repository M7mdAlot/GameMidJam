using UnityEngine;

public class GuardVision : MonoBehaviour
{
    public float DetectRange = 10f;
    public float DetectAngle = 90f;
    public Transform EyePosition;
    public GameObject Player;

    private void Update()
    {
        float distance = Vector3.Distance(Player.transform.position, EyePosition.position);

        if (distance <= DetectRange)
        {
            Vector3 directionToPlayer = Player.transform.position - EyePosition.position;
            float angle = Vector3.Angle(EyePosition.forward, directionToPlayer);

            if (angle <= DetectAngle / 2)
            {
                RaycastHit hit;
                if (Physics.Raycast(EyePosition.position, directionToPlayer, out hit, DetectRange))
                {
                    if (hit.collider.gameObject == Player)
                    {
                        GameManager.Instance.AlertGuards();
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (EyePosition == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(EyePosition.position, DetectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(EyePosition.position, Quaternion.Euler(0, DetectAngle / 2, 0) * EyePosition.forward * DetectRange);
        Gizmos.DrawRay(EyePosition.position, Quaternion.Euler(0, -DetectAngle / 2, 0) * EyePosition.forward * DetectRange);
    }
}