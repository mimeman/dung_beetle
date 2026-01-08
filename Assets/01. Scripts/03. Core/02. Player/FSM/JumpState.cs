#region Class Description
/*
 * [JumpState] (Child of AirState)
 * - 땅에서 점프를 시작하는 순간입니다.
 * - Enter(): 위쪽으로 힘을 가함 (Impulse)
 * - LogicUpdate(): Y축 속도가 줄어들면(정점 도달) FallState로 전환
 */
#endregion

using UnityEngine;

public class JumpState : AirState
{
    public JumpState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        player.Jump(player.Stats.physics.jumpForce);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 상승하다가 속도가 떨어지기 시작하면 Fall 상태로 자연스럽게 전환
        if (player.Rb.velocity.y < 0)
        {
            stateMachine.ChangeState(player.FallState);
        }
    }
}