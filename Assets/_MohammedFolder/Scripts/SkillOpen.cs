using UnityEngine;

public class SkillUnlock : MonoBehaviour
{
    public enum SkillType { DoubleJump, Dash, Sprint }
    public SkillType skill;
    private Animator playerAnimator;

    public void Unlock()
    {
        PlayerMovementNew playerMovement = GameManager.Instance.PlayerMovement;

        if (!playerMovement.IsGrounded) return;

        playerAnimator = GameManager.Instance.Player.GetComponent<Animator>();
        playerAnimator.SetTrigger("pickup");

        playerMovement.enabled = false;

        switch (skill)
        {
            case SkillType.DoubleJump:
                GameManager.Instance.CanDoubleJump = true;
                break;
            case SkillType.Dash:
                GameManager.Instance.CanDash = true;
                break;
            case SkillType.Sprint:
                GameManager.Instance.CanSprint = true;
                break;
        }

        Debug.Log(skill + " unlocked!");
        StartCoroutine(ReenableMovement());
    }

    private System.Collections.IEnumerator ReenableMovement()
    {
        Animator playerAnimator = GameManager.Instance.Player.GetComponent<Animator>();

        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = playerAnimator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("Pickup") && state.normalizedTime >= 1f;
        });

        GameManager.Instance.PlayerMovement.enabled = true;
    }
}