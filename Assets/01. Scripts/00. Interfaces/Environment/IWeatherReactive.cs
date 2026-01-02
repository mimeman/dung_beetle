using UnityEngine;

#region 설명
/* [설명]
 * 날씨 효과(비, 바람)에 반응하는 오브젝트의 규약
 */
#endregion

public interface IWeatherReactive
{
    /// <param name="intensity">비의 세기 (0.0 = 그침, 1.0 = 폭우)</param>
    void OnRain(float intensity);

    /// <param name="force">바람의 방향과 세기 (월드 공간)</param>
    void OnWeatherForce(Vector3 force);
}