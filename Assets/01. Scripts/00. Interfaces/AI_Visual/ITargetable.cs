using UnityEngine;

public interface ITargetable
{
    // 적이 조준해야 할 정확한 위치 (몸통 중심 등).
    Transform TargetTransform { get; }

    // 현재 공격 가능한 상태인지 (죽거나 사라지면 false).
    bool IsValidTarget { get; }

    // 어그로 우선순위 (높을수록 적이 먼저 공격함).
    // 예: 플레이어=1, 미끼 똥=10
    int AggroPriority { get; }
}