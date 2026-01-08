#region 설명
/*
 * [GroundedState] (Parent State)
 * - 땅에 붙어있는 상태들의 '부모 클래스'입니다.
 * - IdleState와 MoveState는 이 클래스를 상속받습니다.
 *
 * - 역할: 
 * 1. 점프 입력 감지 -> JumpState 전환
 * 2. 땅에서 떨어짐 감지 -> FallState 전환
 */
#endregion

using UnityEngine;

public class GroundedState : PlayerState
{
    public GroundedState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.Input.JumpTriggered) stateMachine.ChangeState(player.JumpState);
        if (!player.CheckIfGrounded()) stateMachine.ChangeState(player.FallState);
    }
}