using UnityEngine;

[CreateAssetMenu(fileName = "ToadConfig", menuName = "")]
public class ToadConfig : ScriptableObject
{
    [Header("Attack Settings")]
    public float aimingTime = 1.5f;     // 조준 시간
    public float tongueSpeed = 20f;     // 혀 발사 속도
    public float maxTongueRange = 15f;  // 최대 사거리
    public int biteDamage = 30;         // 최종 섭식 데미지

    [Header("Pull Settings")]
    public float pullSpeed = 5f;        // 당기는 속도
    public float pullDamagePerTick = 2f;// 당기는 중 틱 데미지

    [Header("Stun Settings")]
    public float stunDuration = 3.0f;   // 스턴 지속 시간
    public float cooldownTime = 5.0f;   // 공격 후 대기 시간

    [Header("Camouflage")]
    public float camouflageFadeTime = 1.5f; // 위장 해제 속도
}