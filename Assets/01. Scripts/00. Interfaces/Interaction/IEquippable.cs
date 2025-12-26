using UnityEngine;

#region 설명
/* [설명]
 * 플레이어가 턱(입)에 문 아이템을 '장비(Equipment)'로서 활성화하는 규약.
 
 * [작동 흐름]
 * 1. Equip : 아이템을 입에 물었을 때 호출 -> (예 : 빛나는 돌 켜짐)
 * 2. Unequip : 아이템을 뱉거나 넣었을 때 호출 -> 스탯 원상복구.
 */
#endregion

public interface IEquippable
{
    // 아이템을 장착합니다. (모델 위치 고정, 스탯 적용)
    /// <param name="user">장착한 주체 (플레이어)</param>
    void OnEquip(GameObject user);

    // 아이템을 장착 해제합니다. (스탯 제거, 효과 끄기)
    void OnUnequip();
}