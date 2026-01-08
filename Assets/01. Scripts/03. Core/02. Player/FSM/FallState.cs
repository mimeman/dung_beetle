#region Class Description
/*
 * [FallState] (Child of AirState)
 * - 아래로 떨어지는 상태입니다.
 * - PhysicsUpdate(): 빠른 낙하를 위해 추가 중력 적용
 */
#endregion

using UnityEngine;

public class FallState : AirState
{
    public FallState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 더 묵직한 낙하감을 위해 중력 추가 적용
        player.Rb.AddForce(Physics.gravity * (player.Stats.physics.gravityMultiplier - 1.0f), ForceMode.Acceleration);
    }
}