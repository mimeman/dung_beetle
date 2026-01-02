using UnityEngine;

#region 설명
/* [설명]
 * 아이템을 보관할 수 있는 공간에 대한 규약
 * 플레이어의 입(Mouth - 1칸), 보물상자(Chest - 다수), 쇠똥 내부(Dung - 다수) 등이 사용
 
 * [핵심 개념]
 * - 플레이어 : MaxCapacity가 1인 컨테이너. (입에 하나만 물 수 있음)
 * - 똥에 붙는거 : MaxCapacity가 여러 개인 컨테이너
 
 * [함수]
 * AddItem : 아이템 넣기 (입이 비었으면 물기)
 * RemoveItem : 아이템 빼기 (뱉기)
 * PeekItem : 현재 들어있는 아이템 확인 (가장 최근 것 or 입에 문 것)
 */
#endregion

public interface IContainer
{
    int ItemCount { get; }
    int MaxCapacity { get; }
    bool CanAddItem { get; }


    // 컨테이너에 아이템을 추가
    /// <param name="itemId">아이템 ID (ItemData의 itemID와 동일)</param>
    /// <param name="amount">추가할 수량</param>
    /// <returns>실제로 추가된 개수 (공간 부족 시 일부만 추가될 수 있음)</returns>
    int AddItem(int itemId, int amount = 1);

    // 컨테이너에서 아이템을 제거
    /// <param name="itemId">아이템 ID</param>
    /// <param name="amount">제거할 수량</param>
    bool RemoveItem(int itemId, int amount = 1);

    // 가장 최근에 들어온 아이템 ID를 확인
    int PeekItem();

    // 특정 아이템을 보유하고 있는지 확인
    /// <param name="itemId">아이템 ID</param>
    /// <returns>보유 여부</returns>
    bool HasItem(int itemId);

    // 특정 아이템의 보유 개수를 반환
    /// <param name="itemId">아이템 ID</param>
    /// <returns>보유 개수 (없으면 0)</returns>
    int GetItemCount(int itemId);
}