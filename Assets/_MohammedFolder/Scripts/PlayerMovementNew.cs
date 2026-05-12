using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovementNew : MonoBehaviour
{
    public float Speed = 5f;
    public float dashSpeed = 20f;
    public float baseSpeed;
    private bool jumpPressed = false;
    public Transform CameraTarget;
    public float rotationSpeed = 100f;
    private float verticalRotation = 0f;
    public Animator animator;
    public float Rotation = 720;
    public int JumpLimit = 2;
    public int JumpCount = 0;
    public float JumpForce = 10;
    public Vector3 velocity;
    public float Gravity = -9.8f;
    public bool IsGrounded;
    public CharacterController characterController;
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool IsDashing = false;
    private Vector3 LastMoveDirection;
    int AirDash = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        baseSpeed = Speed;
    }

    void Update()
    {
        IsGrounded = characterController.isGrounded;

        CameraTarget.position = transform.position;

        CameraTarget.Rotate(Vector3.up * LookInput.x * rotationSpeed * Time.deltaTime);
        verticalRotation -= LookInput.y * rotationSpeed * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -20f, 35f);
        CameraTarget.localEulerAngles = new Vector3(verticalRotation, CameraTarget.localEulerAngles.y, 0f);

        float Horizontal = MoveInput.x;
        float Vertical = MoveInput.y;
        Vector3 camForward = CameraTarget.forward;
        Vector3 camRight = CameraTarget.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 CharaMove = camRight * Horizontal + camForward * Vertical;
        if (CharaMove != Vector3.zero)
        {
            LastMoveDirection = CharaMove.normalized;
        }

        characterController.Move(CharaMove * Time.deltaTime * Speed);

        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (IsGrounded && !jumpPressed)
        {
            JumpCount = 0;
            AirDash = 0;
        }

        velocity.y += Gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        if (CharaMove != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(CharaMove, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, Rotation * Time.deltaTime);
        }

        float currentSpeed = new Vector2(Horizontal, Vertical).magnitude;
        if (Speed > baseSpeed)
            animator.SetFloat("speed", currentSpeed);
        else
            animator.SetFloat("speed", currentSpeed * 0.5f);

        if (IsGrounded && velocity.y <= 0)
        {
            animator.SetBool("jump", false);
            animator.SetBool("doublejump", false);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        GetComponent<PlayerSounds>().PlayJumpVoice();
        if (context.started)
        {
            jumpPressed = true;
            if (JumpCount < JumpLimit)
            {
                if (JumpCount == 0)
                {
                    animator.SetBool("jump", true);
                    animator.SetBool("doublejump", false);
                }
                else if (JumpCount == 1 && GameManager.Instance.CanDoubleJump)
                {
                    animator.SetBool("jump", false);
                    animator.SetBool("doublejump", true);
                }
                else return;

                velocity.y = Mathf.Sqrt(JumpForce * -2f * Gravity);
                JumpCount++;
            }
        }
        if (context.canceled)
        {
            jumpPressed = false;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && !IsDashing && GameManager.Instance.CanDash)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        if (IsDashing) yield break;
        IsDashing = true;
        float dashDuration = 0.2f;
        float elapsed = 0f;
        int AirDashLimit = 1;
        float DashCoolDown = 0.8f;

        if (!characterController.isGrounded && AirDash < AirDashLimit)
        {
            while (elapsed < dashDuration)
            {
                characterController.Move(LastMoveDirection * dashSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            AirDash++;
        }

        if (characterController.isGrounded)
        {
            while (elapsed < dashDuration)
            {
                characterController.Move(LastMoveDirection * dashSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(DashCoolDown);
        IsDashing = false;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started && GameManager.Instance.CanSprint)
            Speed = baseSpeed * 2;
        if (context.canceled)
            Speed = baseSpeed;
    }
}