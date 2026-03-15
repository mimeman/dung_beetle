using UnityEngine;
using RootMotion.FinalIK;

public class PlayerPushState : PlayerState
{
    private IDungInteractable _targetDung;
    private CapsuleCollider _playerCollider;
    private Collider _ballCollider;

    private Transform _leftBackLegTarget;
    private Transform _rightBackLegTarget;
    private Transform _leftFrontLegTarget;
    private Transform _rightFrontLegTarget;

    private FullBodyBipedIK _ikSolver;

    private float _pushDistance = 1.2f;
    private bool _isAttached = false;
    private float _attachTimer = 0f;
    private const float ATTACH_DURATION = 0.5f;
    private const float ROTATION_DURATION = 0.3f;

    private bool _isRotating = false;
    private float _rotationTimer = 0f;
    private Quaternion _startRotation;
    private Quaternion _targetRotation;

    private float _ikWeightTimer = 0f;
    private const float IK_BLEND_DURATION = 1.0f;
    private float _currentIKWeight = 0f;

    private float _walkCycleTimer = 0f;
    private const float STEP_DURATION = 0.5f;
    private const float STEP_HEIGHT = 0.15f;
    private const float STEP_LENGTH = 0.2f;

    // Exit 처리
    private bool _isExiting = false;
    private float _exitTimer = 0f;
    private const float EXIT_DURATION = 0.8f;
    private Quaternion _exitStartRotation;
    private Quaternion _exitTargetRotation;

    public PlayerPushState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        CameraManager.Instance?.OnPushStart();

        _targetDung = player.Detector.CurrentInteractable as IDungInteractable;
        if (_targetDung == null)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        _ikSolver = player.IKSolver;
        InitializeIKTargets();

        if (_targetDung is DungBallController dungController)
        {
            _pushDistance = dungController.CurrentRadius + 0.8f;
        }

        SetupPhysics();

        player.Anim.SetTrigger("PushEnter");
        player.Anim.SetBool("IsPushing", true);
        _targetDung.OnPushStart(player.gameObject);

        player.SetGrounderWeight(1f);

        _isRotating = true;
        _rotationTimer = 0f;
        _startRotation = player.transform.rotation;
        Vector3 toDung = (_targetDung.GetPosition() - player.transform.position).normalized;
        _targetRotation = Quaternion.LookRotation(-toDung);

        _isAttached = false;
        _attachTimer = 0f;

        _ikWeightTimer = 0f;
        _currentIKWeight = 0f;
        _walkCycleTimer = 0f;

        _isExiting = false;
        _exitTimer = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (_isExiting)
        {
            HandleExit();
            return;
        }

        if (_isRotating)
        {
            _rotationTimer += Time.deltaTime;
            float t = _rotationTimer / ROTATION_DURATION;

            player.transform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, t);

            if (t >= 1f)
            {
                _isRotating = false;
            }
            return;
        }

        if (!_isAttached)
        {
            _attachTimer += Time.deltaTime;
            if (_attachTimer >= ATTACH_DURATION)
            {
                _isAttached = true;
                EnableIK();
            }
            return;
        }

        if (_currentIKWeight < 1f)
        {
            _ikWeightTimer += Time.deltaTime;
            _currentIKWeight = Mathf.Clamp01(_ikWeightTimer / IK_BLEND_DURATION);
            UpdateIKWeight(_currentIKWeight);
        }

        Vector2 input = player.Input.MoveInput;
        if (input.sqrMagnitude > 0.01f)
        {
            _walkCycleTimer += Time.deltaTime * 2f;
        }

        float pushSpeed = input.magnitude;
        player.Anim.SetFloat("PushSpeed", pushSpeed);

        UpdateIKTargetsWithWalking();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (_targetDung == null) return;

        if (!_isExiting && _isAttached)
        {
            StickToTarget();
        }

        if (!_isAttached || _isRotating || _isExiting) return;

        Vector2 input = player.Input.MoveInput;
        if (input.sqrMagnitude < 0.01f) return;

        Vector3 moveDir = player.GetCameraBasedDirection(input);
        Vector3 forwardDir = player.transform.forward;
        Vector3 rightDir = player.transform.right;

        float forwardInput = Vector3.Dot(moveDir, forwardDir);
        float rightInput = Vector3.Dot(moveDir, rightDir);

        Vector3 pushDir = (forwardDir * forwardInput + rightDir * rightInput).normalized;

        player.PhysicsHandler.AddPushForceToDung(_targetDung, pushDir, player.Stats.push.pushForce);
    }

    public void StartExit()
    {
        if (_isExiting) return;

        _isExiting = true;
        _exitTimer = 0f;

        _exitStartRotation = player.transform.rotation;
        Vector3 toDung = (_targetDung.GetPosition() - player.transform.position).normalized;
        _exitTargetRotation = Quaternion.LookRotation(toDung);

        player.Anim.SetTrigger("PushExit");

        if (_targetDung != null)
        {
            _targetDung.OnPushEnd(player.gameObject);
        }
    }

    private void HandleExit()
    {
        _exitTimer += Time.deltaTime;
        float t = _exitTimer / EXIT_DURATION;

        float ikT = Mathf.Clamp01(t * 2f);
        float ikWeight = Mathf.Lerp(_currentIKWeight, 0f, ikT);
        UpdateIKWeight(ikWeight);

        if (t > 0.3f)
        {
            float rotT = Mathf.Clamp01((t - 0.3f) / 0.5f);
            player.transform.rotation = Quaternion.Slerp(_exitStartRotation, _exitTargetRotation, rotT);
        }

        if (t >= 1f)
        {
            CleanupPhysics();
            DisableIK();

            player.Anim.SetBool("IsPushing", false);
            player.Anim.SetFloat("PushSpeed", 0f);
            player.SetGrounderWeight(1f);

            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        CameraManager.Instance?.OnPushEnd();

        if (!_isExiting)
        {
            DisableIK();
            CleanupPhysics();

            if (_targetDung != null)
            {
                _targetDung.OnPushEnd(player.gameObject);
            }

            player.Anim.SetTrigger("PushExit");
            player.Anim.SetBool("IsPushing", false);
            player.Anim.SetFloat("PushSpeed", 0f);
            player.SetGrounderWeight(1f);
        }

        _targetDung = null;
        _isAttached = false;
        _isRotating = false;
        _currentIKWeight = 0f;
        _isExiting = false;
    }

    #region IK Setup
    private void InitializeIKTargets()
    {
        Transform ikTargets = player.transform.Find("IKTargets");
        if (ikTargets == null)
        {
            Debug.LogError("IKTargets not found!");
            return;
        }

        _leftBackLegTarget = ikTargets.Find("LeftBackLegTarget");
        _rightBackLegTarget = ikTargets.Find("RightBackLegTarget");
        _leftFrontLegTarget = ikTargets.Find("LeftFrontLegTarget");
        _rightFrontLegTarget = ikTargets.Find("RightFrontLegTarget");

        if (_leftBackLegTarget == null) Debug.LogError("LeftBackLegTarget not found!");
        if (_rightBackLegTarget == null) Debug.LogError("RightBackLegTarget not found!");
        if (_leftFrontLegTarget == null) Debug.LogError("LeftFrontLegTarget not found!");
        if (_rightFrontLegTarget == null) Debug.LogError("RightFrontLegTarget not found!");
    }

    private void EnableIK()
    {
        if (_ikSolver == null)
        {
            Debug.LogError("IKSolver is null!");
            return;
        }

        _ikSolver.solver.leftFootEffector.target = _leftBackLegTarget;
        _ikSolver.solver.rightFootEffector.target = _rightBackLegTarget;
        _ikSolver.solver.leftHandEffector.target = _leftFrontLegTarget;
        _ikSolver.solver.rightHandEffector.target = _rightFrontLegTarget;

        _ikSolver.solver.leftFootEffector.positionWeight = 0f;
        _ikSolver.solver.rightFootEffector.positionWeight = 0f;
        _ikSolver.solver.leftHandEffector.positionWeight = 0f;
        _ikSolver.solver.rightHandEffector.positionWeight = 0f;

        Debug.Log("IK Enabled - Targets Connected");
    }

    private void UpdateIKWeight(float weight)
    {
        if (_ikSolver == null) return;

        _ikSolver.solver.leftFootEffector.positionWeight = weight;
        _ikSolver.solver.rightFootEffector.positionWeight = weight;
        _ikSolver.solver.leftHandEffector.positionWeight = weight;
        _ikSolver.solver.rightHandEffector.positionWeight = weight;
    }

    private void DisableIK()
    {
        if (_ikSolver == null) return;

        _ikSolver.solver.leftFootEffector.positionWeight = 0f;
        _ikSolver.solver.rightFootEffector.positionWeight = 0f;
        _ikSolver.solver.leftHandEffector.positionWeight = 0f;
        _ikSolver.solver.rightHandEffector.positionWeight = 0f;
    }

    private void UpdateIKTargetsWithWalking()
    {
        if (_targetDung == null) return;

        Vector3 dungPos = _targetDung.GetPosition();
        Vector3 playerPos = player.transform.position;
        float radius = (_targetDung is DungBallController dc) ? dc.CurrentRadius : 1f;

        Vector3 toDung = (dungPos - playerPos).normalized;
        Vector3 rightDir = Vector3.Cross(Vector3.up, toDung).normalized;

        float cycle = _walkCycleTimer % (STEP_DURATION * 2);

        float leftFootPhase = Mathf.Sin(cycle / STEP_DURATION * Mathf.PI);
        float rightFootPhase = Mathf.Sin((cycle + STEP_DURATION) / STEP_DURATION * Mathf.PI);

        Vector3 backLegCenter = dungPos - toDung * radius;
        float legOffset = 0.3f;

        Vector3 leftBackBase = backLegCenter + rightDir * legOffset;
        Vector3 rightBackBase = backLegCenter - rightDir * legOffset;

        _leftBackLegTarget.position = leftBackBase
            + Vector3.up * (0.2f + STEP_HEIGHT * Mathf.Max(0, leftFootPhase))
            + toDung * (STEP_LENGTH * 0.5f * Mathf.Cos(cycle / STEP_DURATION * Mathf.PI));

        _rightBackLegTarget.position = rightBackBase
            + Vector3.up * (0.2f + STEP_HEIGHT * Mathf.Max(0, rightFootPhase))
            + toDung * (STEP_LENGTH * 0.5f * Mathf.Cos((cycle + STEP_DURATION) / STEP_DURATION * Mathf.PI));

        Vector3 frontLegBase = playerPos - toDung * 0.5f;

        float frontLeftPhase = Mathf.Sin((cycle + STEP_DURATION * 0.5f) / STEP_DURATION * Mathf.PI);
        float frontRightPhase = Mathf.Sin((cycle + STEP_DURATION * 1.5f) / STEP_DURATION * Mathf.PI);

        _leftFrontLegTarget.position = frontLegBase + rightDir * 0.2f
            + Vector3.up * (STEP_HEIGHT * 0.5f * Mathf.Max(0, frontLeftPhase))
            - toDung * (STEP_LENGTH * Mathf.Max(0, frontLeftPhase));

        _rightFrontLegTarget.position = frontLegBase - rightDir * 0.2f
            + Vector3.up * (STEP_HEIGHT * 0.5f * Mathf.Max(0, frontRightPhase))
            - toDung * (STEP_LENGTH * Mathf.Max(0, frontRightPhase));
    }
    #endregion

    #region Physics
    private void SetupPhysics()
    {
        _ballCollider = _targetDung.GetTransform().GetComponent<Collider>();
        _playerCollider = player.GetComponent<CapsuleCollider>();

        if (_ballCollider != null && _playerCollider != null)
        {
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

    private void StickToTarget()
    {
        if (_targetDung == null) return;

        Vector3 dungPos = _targetDung.GetPosition();
        Vector3 playerPos = player.transform.position;

        Vector3 toDung = (dungPos - playerPos).normalized;

        Vector3 targetPosition = dungPos - toDung * _pushDistance;
        targetPosition.y = playerPos.y;

        float distance = Vector3.Distance(playerPos, targetPosition);
        if (distance < 0.1f) return;

        float lerpSpeed = Mathf.Clamp01(distance / _pushDistance) * 0.5f;
        player.Rb.MovePosition(Vector3.Lerp(playerPos, targetPosition, lerpSpeed));
    }
    #endregion
}