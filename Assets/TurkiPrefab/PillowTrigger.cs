using UnityEngine;

public class PillowTrigger : MonoBehaviour
{
    public BearColorChallengeManager challengeManager;
    public ColorPillow colorPillow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            challengeManager.CheckPillow(colorPillow.pillowColor);
        }
    }
}