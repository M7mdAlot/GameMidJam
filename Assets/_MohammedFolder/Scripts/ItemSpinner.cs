using UnityEngine;

public class ItemSpinner : MonoBehaviour
{
    [Tooltip("How fast the item spins. Positive numbers spin one way, negative spin the other.")]
    public float spinSpeed = 100f;

    void Update()
    {
        // Rotate the item smoothly around its Y (Up) axis every frame
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
    }
}