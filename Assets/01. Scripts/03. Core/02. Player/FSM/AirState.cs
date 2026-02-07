#region Class Description
/*
 * [AirState] (Parent State)
 * - 공중에 떠 있는 모든 상태(Jump, Fall, Fly)의 부모입니다.
 * - 공통 역할:
 * 1. 땅에 닿았는지 감지 -> IdleState로 전환 (착지)
 * 2. 점프키 꾹 누름 감지 -> FlyState로 전환 (비행 시도)
 * 3. 공중에서는 Grounder 비활성화 (자연스러운 점프 모션)
 */
#endregion

using UnityEngine;

public class AirState : PlayerState
{
    public AirState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    #region Enter & Exit
    public override void Enter()
    {
        base.Enter();
        player.SetGrounderWeight(0f); // 공중에서 발 자유롭게
        player.SetIKWeight(0f);

    }

    public override void Exit()
    {
        base.Exit();
        player.SetGrounderWeight(1f); // 착지 시 발 다시 땅에 붙임
        player.SetIKWeight(1f);
    }
    #endregion

    #region Logic Update
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        CheckLanding();
    }
    #endregion

    #region Physics Update
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.Move(player.Stats.movement.walkSpeed * 0.8f);
    }
    #endregion

    #region State Transition Checks
    private void CheckLanding()
    {
        if (player.CheckIfGrounded() && player.Rb.velocity.y <= 0.01f)
        {
            player.AbilityManager.StartJumpCooldown();
            stateMachine.ChangeState(player.IdleState);
        }
    }
    #endregion
}