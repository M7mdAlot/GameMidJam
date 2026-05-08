using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementNew : MonoBehaviour
{
    public float Speed = 5f;
    private float baseSpeed;
    private bool jumpPressed = false;
    public Transform CameraTarget;
    public float normalDistance = -3f;
    public float topDistance = -1f;
    public float zoomSpeed = 5f;
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

        if (Vertical != 0 || Horizontal != 0)
        {
            animator.SetBool("Forward", true);
        }

        if (Vertical == 0 && Horizontal == 0)
        {
            animator.SetBool("Forward", false);
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
        if (context.started)
        {
            jumpPressed = true;
            if (JumpCount < JumpLimit)
            {
                if (!IsGrounded && JumpCount == 0)
                {
                    JumpCount++;
                }
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
        if (context.started && !IsDashing)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        if (IsDashing) yield break;
        IsDashing = true;
        float dashSpeed = 20f;
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
        if (context.started)
            Speed = baseSpeed * 2;
        if (context.canceled)
            Speed = baseSpeed;
    }
}