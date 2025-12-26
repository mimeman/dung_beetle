#region 설명
/* [설명]
 * 게임 상태를 저장하고 불러오기 위한 규약.
 * SaveManager가 이 인터페이스를 가진 모든 객체를 찾아 데이터를 요청함.
 
 * [작동 흐름]
 * 1. CaptureState : 저장 시점 -> 내 상태(위치, 체력 등)를 자바스크립트 객체나 클래스로 포장해서 반환.
 * 2. RestoreState : 로드 시점 -> 저장된 데이터를 받아와서 내 변수에 덮어씌움.
 */
#endregion

public interface ISaveable
{
    /// <summary>
    /// 현재 객체의 상태를 데이터(Class/Struct)로 포장해서 반환합니다.
    /// 예: return new DungSaveData(transform.position, mass);
    /// </summary>
    object CaptureState();

    // 저장된 데이터를 받아와서 객체의 상태를 복구합니다.
    /// <param name="state">불러온 데이터 객체 (형변환해서 사용)</param>
    void RestoreState(object state);
}