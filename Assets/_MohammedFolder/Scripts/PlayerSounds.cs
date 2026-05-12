using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    public AudioSource voiceSource;
    public AudioSource footstepSource;
    private float highestPoint;
    public float minimumFallHeight = 2f;
    public AudioClip jumpVoice;
    public AudioClip[] landingVoices;

    public AudioClip walkFootstep;
    public AudioClip runFootstep;

    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.25f;

    private float footstepTimer = 0f;
    private bool wasGrounded = true;
    private PlayerMovementNew playerMovement;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovementNew>();
    }

    private void Update()
    {
        HandleLanding();
        HandleFootsteps();
    }

    private void HandleLanding()
    {
        if (!playerMovement.IsGrounded)
        {
            if (transform.position.y > highestPoint)
                highestPoint = transform.position.y;
        }

        if (!wasGrounded && playerMovement.IsGrounded)
        {
            float fallDistance = highestPoint - transform.position.y;
            if (fallDistance >= minimumFallHeight)
                PlayRandomLanding();

            highestPoint = transform.position.y;
        }

        wasGrounded = playerMovement.IsGrounded;
    }

    private void HandleFootsteps()
    {
        if (!playerMovement.IsGrounded) return;
        if (playerMovement.MoveInput == Vector2.zero) return;

        footstepTimer += Time.deltaTime;

        bool isSprinting = playerMovement.Speed > playerMovement.baseSpeed * 1.5f;

        if (isSprinting)
        {
            if (footstepTimer >= runStepInterval)
            {
                footstepSource.clip = runFootstep;
                footstepSource.Play();
                footstepTimer = 0f;
            }
        }
        else
        {
            if (footstepTimer >= walkStepInterval)
            {
                footstepSource.clip = walkFootstep;
                footstepSource.Play();
                footstepTimer = 0f;
            }
        }
    }

    public void PlayJumpVoice()
    {
        if (jumpVoice != null)
            voiceSource.PlayOneShot(jumpVoice);
    }

    private void PlayRandomLanding()
    {
        if (landingVoices.Length == 0) return;
        int randomIndex = Random.Range(0, landingVoices.Length);
        voiceSource.PlayOneShot(landingVoices[randomIndex]);
    }
}