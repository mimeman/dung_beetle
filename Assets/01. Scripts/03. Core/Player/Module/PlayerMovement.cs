using UnityEngine;
using Dung.Data;

public class PlayerMovement : MonoBehaviour
{
    #region Variables & Inspector
    [Header("Dependencies")]
    [SerializeField] private PlayerInputHandler _input;
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _cameraTransform;

    private bool _isFlying = false;
    private bool _jumpRequested = false;

    private float _currentFlyTimer = 0f;
    private float _currentCooldown = 0f;

    public float CurrentSpeedMultiplier { get; set; } = 1.0f;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;

        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        UpdateTimers();   
        CheckJumpInput();    
        CheckFlightState();  
    }

    private void FixedUpdate()
    {
        MoveCharacter();     
        RotateCharacter();   
        ApplyJumpPhysics();  
        ApplyFlightPhysics();
        ApplyGravity();      
    }
    #endregion

    #region Logic Methods (Update용)
    private void UpdateTimers()
    {
        // 쿨타임 감소
        if (_currentCooldown > 0)
        {
            _currentCooldown -= Time.deltaTime;
        }

        // 땅에 닿으면 비행 시간 초기화
        if (IsGrounded())
        {
            _currentFlyTimer = 0f;
        }
    }

    private void CheckJumpInput()
    {
        // 단순히 입력만 받아서 메모해둠
        if (_input.JumpTriggered)
        {
            _jumpRequested = true;
        }
    }

    private void CheckFlightState()
    {
        // 비행 조건: 키 누름 + 공중 + 쿨타임 없음
        bool canFly = _input.IsJumpPressed && !IsGrounded() && _currentCooldown <= 0;

        if (canFly)
        {
            if (_currentFlyTimer < _stats.flight.maxDuration)
            {
                _isFlying = true;
                _currentFlyTimer += Time.deltaTime;
            }
            else
            {
                // 시간 초과 시 강제 종료 및 쿨타임 시작
                if (_isFlying) StartCooldown();
            }
        }
        else
        {
            _isFlying = false;
        }
    }

    private void StartCooldown()
    {
        _isFlying = false;
        _currentCooldown = _stats.flight.cooldown; // Stats에서 값 가져옴
        Debug.Log($" 엔진 과열 {_stats.flight.cooldown}초간 비행 불가.");
    }
    #endregion

    #region Physics Methods (FixedUpdate용)
    private void MoveCharacter()
    {
        Vector2 input = _input.MoveInput;
        Vector3 moveDir = GetCameraBasedDirection(input);

        bool isSprinting = _input.IsDashPressed;
        float targetSpeed = (input.magnitude > 0)
            ? (isSprinting ? _stats.movement.runSpeed : _stats.movement.walkSpeed)
            : 0f;

        targetSpeed *= CurrentSpeedMultiplier;

        Vector3 currentFlatVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        Vector3 targetFlatVel = moveDir * targetSpeed;
        float accelRate = (input.magnitude > 0) ? _stats.movement.acceleration : _stats.movement.deceleration;

        Vector3 newVel = Vector3.MoveTowards(currentFlatVel, targetFlatVel, accelRate * Time.fixedDeltaTime);
        _rb.velocity = new Vector3(newVel.x, _rb.velocity.y, newVel.z);
    }

    private void RotateCharacter()
    {
        Vector2 input = _input.MoveInput;
        if (input.sqrMagnitude < 0.01f) return;

        Vector3 moveDir = GetCameraBasedDirection(input);
        if (moveDir == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _stats.movement.rotationSpeed * Time.fixedDeltaTime);
    }

    private void ApplyJumpPhysics()
    {
        if (_jumpRequested)
        {
            if (IsGrounded())
            {
                _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
                _rb.AddForce(Vector3.up * _stats.physics.jumpForce, ForceMode.Impulse);
            }
            _jumpRequested = false; // 처리 후 초기화
        }
    }

    private void ApplyFlightPhysics()
    {
        if (_isFlying)
        {
            // 중력 상쇄 + 상승 힘 (Stats 사용)
            Vector3 flyForce = Vector3.up * _stats.flight.flyUpForce;
            _rb.AddForce(-Physics.gravity + flyForce, ForceMode.Acceleration);

            // 속도 제한 (Stats 사용)
            if (_rb.velocity.y > _stats.flight.maxFlySpeed)
            {
                _rb.velocity = new Vector3(_rb.velocity.x, _stats.flight.maxFlySpeed, _rb.velocity.z);
            }
        }
    }

    private void ApplyGravity()
    {
        // 날고 있지 않고 + 공중이면 중력 가속
        if (!_isFlying && !IsGrounded())
        {
            _rb.AddForce(Physics.gravity * (_stats.physics.gravityMultiplier - 1.0f), ForceMode.Acceleration);
        }
    }
    #endregion

    #region Utils
    private Vector3 GetCameraBasedDirection(Vector2 input)
    {
        if (_cameraTransform == null) return Vector3.forward;
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
        return (camForward * input.y + camRight * input.x).normalized;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1.5f, _stats.detection.groundLayer);
    }
    #endregion
}