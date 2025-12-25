using UnityEngine;

#region 설명
/* [설명]
 * 아이템을 보관할 수 있는 공간에 대한 규약.
 * 플레이어의 입(Mouth - 1칸), 보물상자(Chest - 다수), 쇠똥 내부(Dung - 다수) 등이 사용.
 
 * [핵심 개념]
 * - 플레이어 : MaxCapacity가 1인 컨테이너. (입에 하나만 물 수 있음)
 * - 똥에 붙는거 : MaxCapacity가 여러 개인 컨테이너.
 
 * [함수]
 * AddItem : 아이템 넣기 (입이 비었으면 물기).
 * RemoveItem : 아이템 빼기 (뱉기).
 * PeekItem : 현재 들어있는 아이템 확인 (가장 최근 것 or 입에 문 것).
 */
#endregion

public interface IContainer
{
    // [상태 접근자]
    int ItemCount { get; }      // 현재 개수
    int MaxCapacity { get; }    // 최대 용량 (입은 1)
    bool CanAddItem { get; }    // 공간 남았는지

    /// <param name="itemId">아이템 ID</param>
    /// <param name="amount">수량</param>
    int AddItem(string itemId, int amount = 1);

    /// <param name="itemId">아이템 ID</param>
    /// <param name="amount">수량</param>
    bool RemoveItem(string itemId, int amount = 1);

    string PeekItem();

    bool HasItem(string itemId);
}