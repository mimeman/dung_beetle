#region 설명
/* [설명]
 * 저장 시스템(SaveSystem)이 객체를 식별하기 위한 고유 ID 제공자.
 * 유니티의 InstanceID는 게임 재실행 시 바뀌므로, 변하지 않는 고유 문자열(GUID)이 필요함.
 
 * [작동 흐름]
 * 1. 맵 로딩 시 SaveManager가 모든 IIDProvider를 스캔.
 * 2. 저장 파일에 있는 ID와 일치하는 객체를 찾아서 RestoreState 호출.
 */
#endregion

public interface IIDProvider
{
    // 저장 시스템이 객체를 찾기 위한 고유 ID (GUID).
    // 프리팹 인스턴스마다 다른 값을 가져야 함.
    string SaveID { get; }
}