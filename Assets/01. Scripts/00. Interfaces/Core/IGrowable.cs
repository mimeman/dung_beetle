using UnityEngine;

// 참고: GrowthType, ShrinkType Enum은 Scripts/Data/Enums/DungEnums.cs 에 정의됨.

#region 설명
/* [설명]
 * 오브젝트의 '질량(Mass)'과 '크기(Scale)' 변화를 관리하는 규약.
 * 쇠똥(DungBall)이 아이템을 흡수해 커지거나, 충격/날씨로 인해 작아지는 로직 담당.
 *
 * * [핵심 개념]
 * - Mass : 물리적 무게.
 * - Radius : 시각적 크기.
 * - WeatherResistance : 비나 물에 녹지 않는 저항력 (0.0 ~ 1.0).
 
 * * [함수]
 * Grow : 아이템 흡수.
 * Shrink : 충격이나 비(Weather)로 인해 크기 감소.
 * Crumble : 완전 파괴.
 */
#endregion

public interface IGrowable
{
    // [상태 접근자]
    float CurrentMass { get; }
    float CurrentRadius { get; }
    bool IsMaxSize { get; }
    float WeatherResistance { get; }


    /// <param name="amount">추가할 질량의 양</param>
    /// <param name="type">성장 재료 유형 (GrowthType)</param>
    bool Grow(float amount, GrowthType type);


    /// <param name="amount">감소할 질량의 양</param>
    /// <param name="type">감소 원인 (ShrinkType.Weather 등)</param>
    bool Shrink(float amount, ShrinkType type);

    // 대상이 형태를 잃고 완전히 부서집니다.
    void Crumble();
}