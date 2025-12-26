using UnityEngine;

#region 설명
/* [설명]
 * 플레이어가 닿았을 때 부활 지점(Spawn Point)을 갱신해주는 오브젝트 규약
 * 맵 곳곳에 배치된 투명한 박스(Trigger)나 깃발 오브젝트에 사용됨
 *
 * [작동 흐름]
 * 1. OnTriggerEnter : 플레이어가 닿음.
 * 2. GameManager에게 "이제부터 여기가 부활 위치야"라고 알림 (SetRespawnPoint)
 * 3. (선택) 시각적 효과 (깃발 펄럭임, 불 켜짐) 재생
 */
#endregion

public interface ICheckpoint
{
    // 이 체크포인트가 활성화되었을 때 플레이어가 부활할 정확한 위치
    Transform SpawnPoint { get; }

    // 체크포인트에 닿았을 때 호출
    void OnActivate();
}