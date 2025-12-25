#region 설명
/* [설명]
 * 공격받거나 파괴될 수 있는 모든 오브젝트의 '피격 및 사망'을 관리함.
 * 예 : 몬스터의 체력 감소, 나무 상자 파괴, 플레이어 사망 등.
 
 * [함수]
 * TakeDamage : 외부로부터 데미지를 입습니다. (가해자 정보 포함)
 * Die : 체력이 0이 되거나 즉사 조건일 때 사망 처리를 수행합니다.
 */
#endregion
using UnityEngine;

public interface IDamageable
{
    /// <param name="amount">데미지 수치)</param>
    /// <param name="instigator">가해자 정보)</param>
    void TakeDamage(float amount, GameObject instigator);
    void Die(); //사망
}
