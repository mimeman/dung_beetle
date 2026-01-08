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
        CheckFlyTransition();
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
            stateMachine.ChangeState(player.IdleState);
        }
    }

    // 비행 전환 체크 (AbilityManager 통해서만 확인)
    private void CheckFlyTransition()
    {
        // 이미 비행 중이면 체크 안함
        if (player.AbilityManager.IsFlying) return;

        // 점프 키 누름 + 비행 가능 상태
        if (player.Input.IsJumpPressed && player.AbilityManager.CanFly)
        {
            stateMachine.ChangeState(player.FlyState);
        }
    }
    #endregion
}