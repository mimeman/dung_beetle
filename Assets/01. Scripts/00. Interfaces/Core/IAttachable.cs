#region 설명
/* [설명]
 * 오브젝트가 다른 표면(쇠똥)에 부착되거나 떨어져 나가는 행위를 정의함.
 * '괴혼'처럼 아이템들이 쇠똥에 물리적으로 달라붙어 덩어리를 형성하는 로직.
 
 * [작동 흐름]
 * 1. Attach : 굴러가는 쇠똥에 닿으면 호출 -> 물리(Rigidbody) 끄고 쇠똥의 자식(Child)으로 들어감.
 * 2. Detach : 플레이어가 떼어가거나 충격으로 떨어짐 -> 물리 켜고 쇠똥에서 분리됨.
 */
#endregion

using UnityEngine;

public interface IAttachable
{
    // 현재 쇠똥이나 어딘가에 붙어있는 상태인지 확인합니다.
    bool IsAttached { get; }

    // 대상을 특정 표면에 부착합니다. (물리 연산 제거, Transform 종속)
    /// <param name="target">붙을 대상의 부모 Transform (주로 쇠똥)</param>
    void Attach(Transform target);

    // 부착된 상태를 해제하고 떨어져 나옵니다. (물리 연산 복구, 독립)
    /// <param name="force">떨어질 때 튕겨나가는 힘 (null이면 힘 없이 툭 떨어짐)</param>
    void Detach(Vector3? force = null);
}