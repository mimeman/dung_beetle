#region 설명
/* [DungPhysics]
 * 쇠똥의 물리적 육체(Rigidbody, Collider)를 담당하는 모듈.

 * 1. Initialize : 기획 데이터(Stats)를 기반으로 마찰력, 탄성, 무게 초기화.
 * 2. UpdateMass : 성장 로직에 따라 물리적 질량 갱신.
 * 3. SetGrabbedState : 잡기/놓기 상태에 따라 회전 저항(AngularDrag) 조절.
 */
#endregion

using UnityEngine;
using Dung.Data;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class DungPhysics : MonoBehaviour
{
    #region Variables
    private Rigidbody _rb;
    private SphereCollider _collider;
    private DungStats _stats;
    #endregion

    #region Initialization
    public void Initialize(DungStats stats, float initialMass)
    {
        _stats = stats;
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();

        PhysicMaterial mat = new PhysicMaterial
        {
            bounciness = stats.physics.bounciness,
            dynamicFriction = stats.physics.friction,
            staticFriction = stats.physics.friction,
            frictionCombine = PhysicMaterialCombine.Average,
            bounceCombine = PhysicMaterialCombine.Average
        };

        _collider.material = mat;

        _rb.mass = initialMass;
        _rb.drag = 0.1f;
        _rb.angularDrag = stats.physics.friction;

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.centerOfMass = Vector3.zero;
    }
    #endregion

    #region Physics Logic

    // 성장했을 때 실제 물리 무게를 업데이트
    public void UpdateMass(float newMass)
    {
        if (_rb != null)
        {
            _rb.mass = newMass;
        }
    }

    // 플레이어가 잡았을 때 / 놓았을 때 물리 상태를 변경
    public void SetGrabbedState(bool isGrabbed)
    {
        if (_rb == null) return;

        if (isGrabbed)
        {
            // 잡았을 때는 잘 굴러가도록 저항을 낮춤
            _rb.angularDrag = 0.05f;

            // 물리 엔진이 자고 있으면 깨워서 즉각 반응하게 함
            if (_rb.IsSleeping()) _rb.WakeUp();
        }
        else
        {
            float originalFriction = (_stats != null) ? _stats.physics.friction : 0.5f;
            _rb.angularDrag = originalFriction;
        }
    }

    // 외부 충격(바람, 폭발 등)을 가할 때 사용
    public void AddForce(Vector3 force, ForceMode mode = ForceMode.Impulse)
    {
        _rb.AddForce(force, mode);
    }
    #endregion
}