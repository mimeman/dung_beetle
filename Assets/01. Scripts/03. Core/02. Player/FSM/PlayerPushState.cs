using UnityEngine;

public class PlayerPushState : PlayerState
{
    private IDungInteractable _targetDung;
    private CapsuleCollider _playerCollider;
    private Collider _ballCollider;

    private float _pushDistance = 1.2f;
    private bool _isAttached = false;
    private float _attachTimer = 0f;
    private const float ATTACH_DURATION = 0.5f;

    public PlayerPushState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        _targetDung = player.Detector.CurrentInteractable as IDungInteractable;

        if (_targetDung == null)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        // 거리 계산
        if (_targetDung is DungBallController dungController)
        {
            _pushDistance = dungController.CurrentRadius + 0.8f;
        }

        SetupPhysics();

        // 애니메이션 시작
        player.Anim.SetTrigger("PushEnter");
        player.Anim.SetBool("IsPushing", true);

        _targetDung.OnPushStart(player.gameObject);

        // Grounder 활성화 (다리가 땅에 붙음)
        player.SetGrounderWeight(1f);

        _isAttached = false;
        _attachTimer = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 부착 대기
        if (!_isAttached)
        {
            _attachTimer += Time.deltaTime;

            if (_attachTimer >= ATTACH_DURATION)
            {
                _isAttached = true;
            }

            return;
        }

        // 밀기 속도 계산
        Vector2 input = player.Input.MoveInput;
        float pushSpeed = input.magnitude;

        player.Anim.SetFloat("PushSpeed", pushSpeed);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (_targetDung == null) return;

        // 핵심: 플레이어를 공 뒤에 강제로 붙임
        StickToTarget();

        if (!_isAttached) return; // 부착 중에는 입력 무시

        Vector2 input = player.Input.MoveInput;
        if (input.sqrMagnitude < 0.01f) return;

        Vector3 moveDir = player.GetCameraBasedDirection(input);

        float pushForce = player.Stats.push.pushForce;
        float rotSpeed = player.Stats.push.pushRotationSpeed;

        // 수평 방향으로만 (Y축 제거)
        Vector3 horizontalDir = new Vector3(moveDir.x, 0, moveDir.z).normalized;
        player.PhysicsHandler.AddPushForceToDung(_targetDung, horizontalDir, pushForce);

        // 공 방향으로 회전
        RotateTowardsDung(rotSpeed);
    }

    public override void Exit()
    {
        base.Exit();

        if (_targetDung != null)
        {
            CleanupPhysics();
            _targetDung.OnPushEnd(player.gameObject);
        }

        // 애니메이션 종료
        player.Anim.SetTrigger("PushExit");
        player.Anim.SetBool("IsPushing", false);
        player.Anim.SetFloat("PushSpeed", 0f);

        player.SetGrounderWeight(1f);

        _targetDung = null;
        _playerCollider = null;
        _ballCollider = null;
        _isAttached = false;
    }

    private void SetupPhysics()
    {
        _ballCollider = _targetDung.GetTransform().GetComponent<Collider>();
        _playerCollider = player.GetComponent<CapsuleCollider>();

        if (_ballCollider != null && _playerCollider != null)
        {
            // 충돌 무시 (플레이어가 공 위로 안 올라가게)
            Physics.IgnoreCollision(_playerCollider, _ballCollider, true);
        }

        Rigidbody ballRb = _targetDung.GetTransform().GetComponent<Rigidbody>();
        if (ballRb != null && ballRb.IsSleeping())
        {
            ballRb.WakeUp();
        }
    }

    private void CleanupPhysics()
    {
        if (_ballCollider != null && _playerCollider != null)
        {
            Physics.IgnoreCollision(_playerCollider, _ballCollider, false);
        }
    }

    // 핵심: 플레이어를 공 뒤에 딱 붙임
    private void StickToTarget()
    {
        Vector3 dungPos = _targetDung.GetPosition();
        Vector3 playerPos = player.transform.position;

        // 공에서 플레이어로의 방향 (수평만)
        Vector3 directionFromDung = (playerPos - dungPos).normalized;
        directionFromDung.y = 0;

        // 방향이 유효하지 않으면 플레이어의 반대 방향
        if (directionFromDung.sqrMagnitude < 0.01f)
        {
            directionFromDung = -player.transform.forward;
        }

        // 목표 위치: 공 뒤쪽
        Vector3 targetPosition = dungPos + directionFromDung * _pushDistance;
        targetPosition.y = playerPos.y; // Y는 유지 (지면 위치)

        // 강제로 위치 고정 (부드럽게)
        player.Rb.MovePosition(Vector3.Lerp(playerPos, targetPosition, 0.9f));
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