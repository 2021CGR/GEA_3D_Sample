using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;

    public string speedParam = "speed";
    public string groundedParam = "IsGrounded";
    public string jumpTrigger = "jump";
    public string attackTrigger = "Attack";
    public string buildTrigger = "Build";
    public string toolTypeParam = "ToolType";
    public string moveParam = "move";
    public string headTurnParam = "head_turn";
    public float headTurnSmoothing = 8f;
    public float headTurnScale = 0.5f;
    float _headTurnCurrent = 0f;
    bool _jumpIsTrigger = true;
    public bool walkOnly = true;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            foreach (var p in animator.parameters)
            {
                if (p.name == jumpTrigger)
                {
                    _jumpIsTrigger = p.type == AnimatorControllerParameterType.Trigger;
                    break;
                }
            }
        }
    }

    public void SetSpeed(float speed)
    {
        if (animator == null) return;
        animator.SetFloat(speedParam, speed);
    }

    public void SetGrounded(bool grounded)
    {
        if (animator == null) return;
        animator.SetBool(groundedParam, grounded);
    }

    public void TriggerJump()
    {
        if (animator == null) return;
        if (walkOnly) return;
        if (_jumpIsTrigger)
        {
            animator.SetTrigger(jumpTrigger);
        }
        else
        {
            animator.SetBool(jumpTrigger, true);
            CancelInvoke(nameof(ResetJumpBool));
            Invoke(nameof(ResetJumpBool), 0.1f);
        }
    }

    void ResetJumpBool()
    {
        if (animator == null) return;
        animator.SetBool(jumpTrigger, false);
    }

    public void TriggerAttack()
    {
        if (animator == null) return;
        if (walkOnly) return;
        animator.SetTrigger(attackTrigger);
    }

    public void TriggerBuild()
    {
        if (animator == null) return;
        if (walkOnly) return;
        animator.SetTrigger(buildTrigger);
    }

    public void SetTool(BlockType? tool)
    {
        if (animator == null) return;
        if (walkOnly) return;
        int t = 0;
        if (tool.HasValue)
        {
            switch (tool.Value)
            {
                case BlockType.IronSword: t = 1; break;
                case BlockType.Axe: t = 2; break;
                case BlockType.Pickax: t = 3; break;
                default: t = 0; break;
            }
        }
        animator.SetInteger(toolTypeParam, t);
    }

    public void ApplyLocomotion(float h, float v, bool run)
    {
        if (animator == null) return;
        int moveState = 0;
        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
        {
            moveState = 1;
            animator.SetFloat(speedParam, v >= 0f ? 1f : -1f);
        }
        else
        {
            animator.SetFloat(speedParam, 1f);
        }
        animator.SetInteger(moveParam, moveState);
    }

    public void SetHeadTurn(float mouseXDelta)
    {
        if (animator == null) return;
        if (walkOnly)
        {
            animator.SetInteger(headTurnParam, 0);
            return;
        }
        int dir = 0;
        if (mouseXDelta < -0.15f) dir = 1;
        else if (mouseXDelta > 0.15f) dir = 2;
        animator.SetInteger(headTurnParam, dir);
    }
}
