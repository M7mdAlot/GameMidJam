using UnityEngine;

public class SkillOpen: MonoBehaviour
{
    public enum SkillType { DoubleJump, Dash, Sprint }
    public SkillType skill;

    public void Unlock()
    {
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
    }
}