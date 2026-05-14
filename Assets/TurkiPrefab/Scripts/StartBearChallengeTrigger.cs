using UnityEngine;

public class StartBearChallengeTrigger : MonoBehaviour
{
    public BearColorChallengeManager challengeManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            challengeManager.StartChallenge();
        }
    }
}