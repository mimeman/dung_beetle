using UnityEngine;

public interface IEffectHandler
{
    // 특정 상황에 맞는 이펙트(VFX)와 소리(SFX)를 재생합니다.
    /// <param name="effectKey">이펙트 이름 (예: "Hit_Wood", "Attach_Gold")</param>
    /// <param name="position">재생 위치</param>
    /// <param name="normal">충돌 표면의 방향 (이펙트 회전용)</param>
    void PlayEffect(string effectKey, Vector3 position, Vector3 normal);
}