using UnityEngine;
using Dung.Data;

public class DungPhysicsHandler : MonoBehaviour
{
    private Rigidbody _rb;
    private DungStats _stats;
    private DungBallController _controller;

    private Vector3 _surfaceNormal;
    private bool _isGrounded;
    private bool _isInitialized = false;

    [SerializeField] private float _groundStickForce = 5f; // 땅에 붙이는 힘

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _controller = GetComponent<DungBallController>();
    }

    public void Initialize(DungStats stats, DungBallController controller)
    {
        _stats = stats;
        _controller = controller;

        if (_rb == null) _rb = GetComponent<Rigidbody>();

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!_isInitialized) return;

        CheckGround();
        ApplySlopePhysics();
        StickToGround(); // 땅에 붙이기
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

    // 공이 땅에서 떨어지지 않도록
    private void StickToGround()
    {
        if (!_isGrounded) return;

        // Y축 속도가 위로 가는 경우 억제
        if (_rb.velocity.y > 0.1f)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        }

        // 추가로 아래 방향 힘 적용
        _rb.AddForce(Vector3.down * _groundStickForce, ForceMode.Force);
    }

    public void AddPushForce(Vector3 direction, float power)
    {
        if (!_isInitialized || _rb == null || _controller == null) return;

        // Y축 제거 (수평 방향으로만)
        direction.y = 0;
        direction.Normalize();

        float force = power / _controller.CurrentMass;
        _rb.AddForce(direction * force, ForceMode.VelocityChange);
    }
}