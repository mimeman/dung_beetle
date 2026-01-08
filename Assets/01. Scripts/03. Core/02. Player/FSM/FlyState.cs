#region Class Description
/*
 * [FlyState] (Child of AirState)
 * - 글라이딩/비행 상태입니다.
 * - 기능:
 * 1. 지속 시간(1.5초) 체크 -> 초과 시 FallState로 강제 전환
 * 2. 쿨타임(10초) 관리 -> AbilityManager가 담당
 * 3. 물리: 중력 상쇄 + 상승 힘 적용
 */
#endregion

using UnityEngine;

public class FlyState : AirState
{
    public FlyState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    #region State Lifecycle
    public override void Enter()
    {
        base.Enter();
        player.AbilityManager.StartFly();
    }

    public override void Exit()
    {
        base.Exit();
        player.AbilityManager.EndFly();
    }
    #endregion

    #region Logic Update
    public override void LogicUpdate()
    {
        // 주의: base.LogicUpdate() 호출하면 AirState의 CheckFlyTransition()이 
        // 또 실행되므로 여기서는 직접 착지만 체크

        CheckLanding();
        CheckFlyEnd();
    }
    #endregion

    #region Physics Update
    public override void PhysicsUpdate()
    {
        ApplyFlight();
        LimitVerticalSpeed();
    }
    #endregion

    #region State Transition Checks
    // 착지 체크 (AirState 로직 복사)
    private void CheckLanding()
    {
        if (player.CheckIfGrounded() && player.Rb.velocity.y <= 0.01f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    /// <summary>비행 종료 조건 체크</summary>
    private void CheckFlyEnd()
    {
        // 1. 시간 초과
        if (player.AbilityManager.IsFlyDurationOver())
        {
            stateMachine.ChangeState(player.FallState);
            return;
        }

        // 2. 키를 뺌 (선택적 조기 종료)
        if (!player.Input.IsJumpPressed)
        {
            stateMachine.ChangeState(player.FallState);
        }
    }
    #endregion

    #region Flight Physics
    // 비행 물리 적용
    private void ApplyFlight()
    {
        // 수평 이동
        player.Move(player.Stats.movement.walkSpeed);

        // 상승력 적용 (중력 상쇄 + 추가 힘)
        Vector3 flyForce = Vector3.up * player.Stats.flight.flyUpForce;
        player.Rb.AddForce(-Physics.gravity + flyForce, ForceMode.Acceleration);
    }

    // 최대 상승 속도 제한
    private void LimitVerticalSpeed()
    {
        if (player.Rb.velocity.y > player.Stats.flight.maxFlySpeed)
        {
            player.Rb.velocity = new Vector3(
                player.Rb.velocity.x,
                player.Stats.flight.maxFlySpeed,
                player.Rb.velocity.z
            );
        }
    }
    #endregion
}