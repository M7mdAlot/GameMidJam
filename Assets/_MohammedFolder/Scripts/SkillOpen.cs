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

        // --- NEW CODE: HIDE THE ITEM SAFELY ---

        // 1. Turn off the Interactable script so the player can't keep pressing 'E'
        Interactable interactScript = GetComponent<Interactable>();
        if (interactScript != null) interactScript.enabled = false;

        // 2. Hide all the visual parts of the item (including child objects)
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        
        // 3. Turn off colliders so the player doesn't bump into an invisible item
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        // --------------------------------------

        StartCoroutine(ReenableMovement());
    }

    private System.Collections.IEnumerator ReenableMovement()
    {
        Animator playerAnimator = GameManager.Instance.Player.GetComponent<Animator>();
        float timeout = 3f;
        float elapsed = 0f;

        yield return new WaitUntil(() =>
        {
            elapsed += Time.deltaTime;
            AnimatorStateInfo state = playerAnimator.GetCurrentAnimatorStateInfo(0);
            return (state.IsName("Pickup") && state.normalizedTime >= 1f) || elapsed >= timeout;
        });

        GameManager.Instance.PlayerMovement.enabled = true;

        // --- NEW CODE: CLEAN UP ---
        // Now that the player is unfrozen, it is safe to completely delete the item from the game.
        Destroy(gameObject);
    }
}