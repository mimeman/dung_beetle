using UnityEngine;

public interface IWeatherReactive
{

    //날씨 저항력 (0.0 ~ 1.0). 1.0이면 완전 면역.
    float WeatherResistance { get; }

    // 비가 내릴 때 호출됨.
    /// <param name="intensity">비의 세기 (0.0 ~ 1.0)</param>
    void OnRain(float intensity);

    // 바람 등 물리적 날씨 효과가 있을 때 호출됨.
    void OnWeatherForce(Vector3 force);
}