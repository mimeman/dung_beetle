using Dung.Enums;

public interface ISurfaceEffector
{
    // 이동 속도 배율 (1.0 = 정상, 0.5 = 느려짐)
    float SpeedMultiplier { get; }

    // 바닥이 부여하는 상태 이상 (예: Stuck, Slippery)
    PhysicalStateType SurfaceState { get; }

    // 밟았을 때 입는 초당 데미지 (예: 가시) 0이면 없음
    float DamagePerSecond { get; }
}