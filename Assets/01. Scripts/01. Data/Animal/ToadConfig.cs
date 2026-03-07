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

    [Header("Tongue Visual")]
    public float tongueWidth = 0.15f;          // 혀 두께
    public Color tongueColor = new Color(0.85f, 0.3f, 0.4f); // 혀 색상 (분홍)
    public float tongueRetractSpeed = 15f;     // 혀 수축 속도
}