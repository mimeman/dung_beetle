#region Class Description
/*
 * [MoveState] (Child State)
 * - 플레이어가 이동 중일 때 실행되는 상태입니다.
 * - 상속: GroundedState (따라서 점프/낙하 체크 기능 포함)
 * - 로직:
 * 1. 입력이 멈추면 -> IdleState로 전환
 * 2. 물리 업데이트에서 실제 이동 함수(player.Move) 호출
 */
#endregion

using UnityEngine;

public class MoveState : GroundedState
{
    public MoveState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.Input.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 입력 받기
        Vector2 input = player.Input.MoveInput;
        bool isDashing = player.Input.IsDashPressed;

        // 속도 계산
        float targetSpeed = isDashing
            ? player.Stats.movement.runSpeed
            : player.Stats.movement.walkSpeed;

        player.Move(targetSpeed);

        // ===== 핵심: Animator 파라미터 업데이트 =====
        player.Anim.SetFloat("Horizontal", input.x);
        player.Anim.SetFloat("Vertical", input.y);

        // Speed로 Locomotion <-> Run 전환 제어
        float normalizedSpeed = isDashing ? 1f : 0.5f;
        player.Anim.SetFloat("Speed", normalizedSpeed);
    }
}