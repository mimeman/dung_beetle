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

    [Header("Flying Settings (Stats에 없음)")]
    [SerializeField] private float _flyUpForce = 10.0f;
    [SerializeField] private float _maxFlySpeed = 5.0f;

    // 외부 제어용 배율
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

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
        HandleAirAction();
        ApplyGravityModifier();
    }
    #endregion

    #region Ground Movement (카메라 기준 이동)
    private void HandleMovement()
    {
        Vector2 input = _input.MoveInput;

        // 1. 카메라 기준 방향 계산 (핵심 ⭐)
        Vector3 moveDir = GetCameraBasedDirection(input);

        // 2. 목표 속도 설정
        float targetSpeed = (input.magnitude > 0) ? _stats.movement.runSpeed : 0f;
        targetSpeed *= CurrentSpeedMultiplier;

        // 3. 가속도/감속도 적용 (Stats의 acceleration 사용)
        Vector3 currentFlatVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        Vector3 targetFlatVel = moveDir * targetSpeed;

        // 입력이 있으면 가속(Acceleration), 없으면 감속(Deceleration) 사용
        float accelRate = (input.magnitude > 0) ? _stats.movement.acceleration : _stats.movement.deceleration;

        // MoveTowards로 부드럽지만 빠릿하게 속도 변화
        Vector3 newVel = Vector3.MoveTowards(currentFlatVel, targetFlatVel, accelRate * Time.fixedDeltaTime);

        // 4. 물리 적용
        _rb.velocity = new Vector3(newVel.x, _rb.velocity.y, newVel.z);
    }

    private void HandleRotation()
    {
        Vector2 input = _input.MoveInput;
        if (input.sqrMagnitude < 0.01f) return;

        // 이동하려는 방향(moveDir)을 바라보게 회전
        Vector3 moveDir = GetCameraBasedDirection(input);
        if (moveDir == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);

        // Slerp로 부드럽게 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _stats.movement.rotationSpeed * Time.fixedDeltaTime);
    }
    #endregion

    #region Air Action
    private void HandleAirAction()
    {
        bool isGrounded = IsGrounded();

        if (_input.JumpTriggered && isGrounded)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            _rb.AddForce(Vector3.up * _stats.physics.jumpForce, ForceMode.Impulse);
        }

        // 비행
        if (_input.IsJumpPressed && !isGrounded)
        {
            if (_rb.velocity.y < _maxFlySpeed)
            {
                _rb.AddForce(Vector3.up * _flyUpForce, ForceMode.Acceleration);
            }
        }
    }

    private void ApplyGravityModifier()
    {
        if (!IsGrounded() && !_input.IsJumpPressed)
        {
            // [수정] environment -> physics로 변경
            _rb.AddForce(Physics.gravity * (_stats.physics.gravityMultiplier - 1.0f), ForceMode.Acceleration);
        }
    }
    #endregion

    #region Utils
    private Vector3 GetCameraBasedDirection(Vector2 input)
    {
        // 카메라의 앞/오른쪽 벡터 가져오기
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;

        // Y축 제거 (평면 이동)
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 입력값과 섞어서 최종 방향 결정
        return (camForward * input.y + camRight * input.x).normalized;
    }

    private bool IsGrounded()
    {
        float checkDistance = 1.3f;
return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, checkDistance, _stats.detection.groundLayer);    }
    #endregion
}