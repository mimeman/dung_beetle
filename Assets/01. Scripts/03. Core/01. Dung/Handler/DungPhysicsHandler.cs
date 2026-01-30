using UnityEngine;
using Dung.Data;

public class DungPhysicsHandler : MonoBehaviour
{
    private Rigidbody _rb;

    [SerializeField] private DungStats _stats;
    private DungBallController _controller;

    private Vector3 _surfaceNormal;
    private bool _isGrounded;

    private bool _isInitialized = false;

    private void Awake()
    {
        // 1. 필요한 컴포넌트들을 스스로 먼저 찾아봅니다.
        _rb = GetComponent<Rigidbody>();
        _controller = GetComponent<DungBallController>();

        // 2. 만약 인스펙터에서 Stats를 넣어줬다면 자동으로 초기화 시도
        if (_stats != null && _controller != null && _rb != null)
        {
            _isInitialized = true;
        }
    }

    public void Initialize(DungStats stats, DungBallController controller)
    {
        _stats = stats;
        _controller = controller;
        _rb = GetComponent<Rigidbody>();

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!_isInitialized) return;

        CheckGround();
        ApplySlopePhysics();
    }

    private void CheckGround()
    {
        if (_controller == null) return;

        float rayDistance = _controller.CurrentRadius + 0.2f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayDistance))
        {
            _isGrounded = true;
            _surfaceNormal = hit.normal;
        }
        else
        {
            _isGrounded = false;
        }
    }

    private void ApplySlopePhysics()
    {
        if (!_isGrounded || _stats == null || _controller == null) return;

        Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, _surfaceNormal).normalized;
        float angle = Vector3.Angle(Vector3.up, _surfaceNormal);

        if (angle > 1f)
        {
            float slopeForceMagnitude = _controller.CurrentMass * Physics.gravity.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);
            _rb.AddForce(slopeDirection * slopeForceMagnitude, ForceMode.Force);
        }
    }

    public void AddPushForce(Vector3 direction, float power)
    {
        if (!_isInitialized || _rb == null || _controller == null)
        {
            Debug.LogWarning($"{gameObject.name}: 물리 핸들러가 아직 준비되지 않았습니다.");
            return;
        }

        // 질량에 반비례하여 힘을 적용
        float force = power / _controller.CurrentMass;
        _rb.AddForce(direction * force, ForceMode.VelocityChange);
    }
}