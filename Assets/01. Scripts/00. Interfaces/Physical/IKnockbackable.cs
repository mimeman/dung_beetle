#region 설명
/* [설명]
 * 데미지와 무관하게 '물리적인 충격(밀려남)'을 처리함.
 * 예 : 쇠똥이 발에 차여 굴러감, 플레이어가 폭발에 휘말려 날아감, 새가 쪼아서 밀림.
 * 이동 시스템(MovementSystem)이나 물리 제어기(Rigidbody)가 이를 구현하여 반응함.
 
 * [함수]
 * ApplyKnockback : 물리적 힘을 가해 대상을 밀어내거나 회전시킵니다.
 */
#endregion

using UnityEngine;

public interface IKnockbackable
{
    /// <param name="force">힘의 방향과 크기가 포함된 벡터 (예: 방향 * 파워)</param>
    /// <param name="hitPoint">타격 위치 (쇠똥을 굴리거나 회전을 줄 때 필요, null이면 중심 타격)</param>
    /// <param name="source">충격을 준 주체 (공격자)</param>
    void ApplyKnockback(Vector3 force, Vector3? hitPoint = null, GameObject source = null);
}