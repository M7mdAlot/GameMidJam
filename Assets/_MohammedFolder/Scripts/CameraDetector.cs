using UnityEngine;

public class CameraDetector : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";


        private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            GameManager.Instance.AlertGuards();
        }
    }
    }


