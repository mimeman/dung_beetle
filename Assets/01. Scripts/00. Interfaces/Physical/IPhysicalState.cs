#region 설명
/* [설명]
 * 쇠똥이나 캐릭터의 '물리적 제약 상태'를 관리함.
 * 예 : 뒤집혀서 못 일어남, 진흙에 빠져서 이동 불가, 물에 젖어서 무거워짐 등.
 * Monster나 Player, DungBall 등 물리적인 상호작용이 중요한 오브젝트들이 사용함.
 
 * [함수]
 * AddState : 대상에게 특정 물리적 상태를 부여합니다.
 * RemoveState : 대상의 특정 물리적 상태를 해제합니다.
 * CheckState : 현재 대상이 특정 상태인지 확인합니다.
 */
#endregion

public enum PhysicalStateType
{
    None = 0,
    Flipped,    // 뒤집힘           (일어나려면 A,D 연타)
    StuckInMud, // 진흙에 빠짐       (용기 or 도구 필요)
    Wet,        // 젖음             (똥이 무거워짐, 미끄러움)
    Stunned     // 기절             (조작 불능)
}

public interface IPhysicalState
{
    /// <param name="state">부여할 상태 종류 (예: 진흙에 빠짐)</param>
    void AddState(PhysicalStateType state);

    /// <param name="state">해제할 상태 종류 (예: 진흙에서 빠져나옴)</param>
    void RemoveState(PhysicalStateType state);

    /// <param name="state">확인하고 싶은 상태</param>
    /// <returns>해당 상태라면 true 반환</returns>
    bool CheckState(PhysicalStateType state);
}
