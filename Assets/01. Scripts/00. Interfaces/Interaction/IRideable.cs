using UnityEngine;

#region 설명
/* [설명]
 * 플레이어가 탑승할 수 있는 오브젝트(쇠똥, 뗏목)에 대한 규약.
 * 탑승 시 플레이어의 이동 제어권을 탑승물이 가져가거나, 플레이어 위치를 고정함.
 
 * [작동 흐름]
 * 1. Mount : 탑승 시도 -> 플레이어를 탑승 위치(MountPoint)로 이동 및 부모 설정.
 * 2. Dismount : 내리기 -> 플레이어의 부모 해제 및 이동 제어권 복구.
 */
#endregion

public interface IRideable
{
    //플레이어가 탑승할 정확한 위치 (좌석, 똥 위치)
     Transform MountPoint { get; }
     bool IsRiding { get; }

    // 대상을 탑승시킵니다.
    /// <param name="rider">탑승하려는 대상 (플레이어)</param>
    void Mount(GameObject rider);

    // 탑승 상태를 해제하고 내립니다.
    /// <param name="dismountPosition">내릴 위치 (null이면 기본 옆 위치)</param>
    void Dismount(Vector3? dismountPosition = null);
}