#region Class Description
/*
 * [IdleState] (Child State)
 * - 플레이어가 정지해 있을 때 실행되는 상태입니다.
 * - 상속: GroundedState (따라서 점프/낙하 체크 기능 포함)
 * - 로직:
 * 1. 이동 입력이 들어오면 -> MoveState로 전환
 * 2. 물리적으로 미끄러짐 방지를 위해 속도 0 유지
 */
#endregion

using UnityEngine;

public class IdleState : GroundedState
{
    public IdleState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.Input.MoveInput != Vector2.zero)
        {
            stateMachine.ChangeState(player.MoveState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.Move(0f); 
    }
}