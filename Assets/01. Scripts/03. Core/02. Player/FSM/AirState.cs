#region Class Description
/*
 * [AirState] (Parent State)
 * - 공중에 떠 있는 모든 상태(Jump, Fall, Fly)의 부모입니다.
 * - 공통 역할:
 * 1. 땅에 닿았는지 감지 -> IdleState로 전환 (착지)
 * 2. 점프키 꾹 누름 감지 -> FlyState로 전환 (비행 시도)
 */
#endregion

using UnityEngine;

public class AirState : PlayerState
{
    public AirState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

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

        // 공중에서도 이동은 가능 (Air Control)
        player.Move(player.Stats.movement.walkSpeed * 0.8f); // 공중 감속 예시
    }
    #endregion

    #region State Transition Checks
    // 착지 체크
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