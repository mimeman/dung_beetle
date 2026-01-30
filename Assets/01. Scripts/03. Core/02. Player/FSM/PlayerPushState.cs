using UnityEngine;
using RootMotion.FinalIK;

public class PlayerPushState : PlayerState
{
    private IDungInteractable _targetDung;
    private float _pushPower = 20f; // 미는 힘

    // 생성자: 부모 클래스의 규칙을 따름
    public PlayerPushState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        // 1. 공 감지 및 할당
        _targetDung = player.Detector.CurrentInteractable as IDungInteractable;

        if (_targetDung != null)
        {
            // 2. 물리 컴포넌트 참조
            SphereCollider ballCollider = _targetDung.GetTransform().GetComponent<SphereCollider>();
            CapsuleCollider playerCollider = player.GetComponent<CapsuleCollider>();
            Rigidbody ballRb = _targetDung.GetTransform().GetComponent<Rigidbody>();

            // 3. 올라타기 방지 및 물리 연결
            if (ballCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, ballCollider, true);

                // 공이 자고 있다면 즉시 깨워서 반응하게 함
                if (ballRb != null && ballRb.IsSleeping()) ballRb.WakeUp();
            }

            // 4. 상호작용 알림
            _targetDung.OnPushStart(player.gameObject);

            // 5. IK 설정 및 초기화
            var (leftHand, rightHand) = _targetDung.GetIKTargets();

            if (leftHand != null && rightHand != null)
            {
                // 가중치 0에서 시작하도록 초기화 (부드러운 전환용)
                player.IKSolver.solver.IKPositionWeight = 0f;
                player.IKSolver.solver.leftHandEffector.positionWeight = 0f;
                player.IKSolver.solver.rightHandEffector.positionWeight = 0f;

                player.IKSolver.solver.leftHandEffector.target = leftHand;
                player.IKSolver.solver.rightHandEffector.target = rightHand;

                // 부드럽게 손 붙이기 시작
                player.StartCoroutine(player.SetIKWeight(1.0f, 0.5f));
            }
        }
        else
        {
            // 예외 처리: 공이 없으면 즉시 복귀
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (_targetDung == null) return;

        float targetSpeed = player.Stats.movement.walkSpeed * player.Stats.push.speedMultiplier;
        float pushForce = player.Stats.push.pushForce;
        float rotSpeed = player.Stats.push.pushRotationSpeed;

        Vector3 moveDir = player.Input.MoveDirection;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            // 1. 공에 물리 힘 전달 (Stats의 pushForce 사용)
            player.PhysicsHandler.AddPushForceToDung(_targetDung, moveDir, pushForce);

            // 2. 캐릭터 이동 (Stats의 감속된 속도 사용)
            player.Move(targetSpeed);

            // 3. 캐릭터 회전 (Stats의 pushRotationSpeed 사용)
            RotateTowardsDung(rotSpeed);
        }
    }



    public override void Exit()
    {
        base.Exit();

        if (_targetDung != null)
        {
            Collider ballCollider = _targetDung.GetTransform().GetComponent<Collider>();
            CapsuleCollider playerCollider = player.GetComponent<CapsuleCollider>();
            if (ballCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, ballCollider, false);
            }

            // 9. 쇠똥 점유 해제 및 IK 가중치 제거
            _targetDung.OnPushEnd(player.gameObject);
            player.StartCoroutine(player.SetIKWeight(0f, 0.3f));

            // 타겟 참조 초기화
            player.IKSolver.solver.leftHandEffector.target = null;
            player.IKSolver.solver.rightHandEffector.target = null;
        }
    }

    private void RotateTowardsDung(float rotSpeed)
    {
        Vector3 dir = (_targetDung.GetPosition() - player.transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
        {
            player.transform.rotation = Quaternion.Slerp(
                player.transform.rotation,
                Quaternion.LookRotation(dir),
                rotSpeed * Time.fixedDeltaTime
            );
        }
    }
}