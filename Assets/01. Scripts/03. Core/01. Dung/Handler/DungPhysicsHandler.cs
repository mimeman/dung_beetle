using UnityEngine;
using Dung.Data;

public class DungPhysicsHandler : MonoBehaviour
{
    private Rigidbody _rb;
    private DungStats _stats;
    private DungBallController _controller;

    private Vector3 _surfaceNormal;
    private bool _isGrounded;

    public void Initialize(DungStats stats, DungBallController controller)
    {
        _stats = stats;
        _controller = controller;
        _rb = GetComponent<Rigidbody>();

        // Rigidbody의 기본 물리 특성 설정
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplySlopePhysics();
    }

    private void CheckGround()
    {
        // 공의 하단으로 레이를 쏘아 지면의 기울기를 파악합니다.
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
        if (!_isGrounded) return;

        // 경사면의 방향 벡터를 계산합니다.
        Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, _surfaceNormal).normalized;

        // 지면의 기울기 정도를 파악합니다 (Up 벡터와 Normal 벡터 사이의 각도).
        float angle = Vector3.Angle(Vector3.up, _surfaceNormal);

        if (angle > 1f) // 경사가 어느 정도 있을 때만 작동
        {
            // 경사면 가속도 계산: 질량이 클수록 더 강하게 끌어당겨집니다.
            float slopeForceMagnitude = _controller.CurrentMass * Physics.gravity.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);
            _rb.AddForce(slopeDirection * slopeForceMagnitude, ForceMode.Force);
        }
    }

    // 플레이어가 미는 힘을 전달받는 메서드 (PushState에서 호출 예정)
    public void AddPushForce(Vector3 direction, float power)
    {
        // 질량에 반비례하여 힘을 적용 (무거울수록 덜 밀림)
        float force = power / _controller.CurrentMass;
        _rb.AddForce(direction * force, ForceMode.VelocityChange);
    }
}