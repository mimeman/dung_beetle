namespace Dung.Enums
{
    // [아이템 분류] - 엑셀에서 관리할 타입들
    public enum ItemType
    {
        Resource,   // 잡동사니 (돌, 나뭇가지)
        Consumable, // 먹어서 즉시 효과 (체력 회복 등)
        Equipment,  // 입에 물어서 장착 (횃불, 뿔피리)
    }

    // [똥 성장 관련] - IGrowable에서 사용
    public enum GrowthType
    {
        Organic,    // 유기물 (기본 똥, 음식, 식물)
        Mineral,    // 무기물 (돌, 고철)
        Liquid,     // 액체 (물, 진흙)
        Special     // 특수 (별, 유물)
    }

    public enum ShrinkType
    {
        Collision,  // 물리적 충돌
        Explosion,  // 폭발
        Burn,       // 불
        Weather,    // 날씨 (비)
        Dissolve    // 용해
    }

    // [상태 이상 관련] - IPhysicalState에서 사용
    public enum PhysicalStateType
    {
        None = 0,
        Flipped,    // 뒤집힘
        Stuck,      // 끼임/진흙
        Wet,        // 젖음
        Burning,    // 불탐
        Stunned     // 기절
    }

    // [장비 관련] - IEquippable에서 사용
    public enum EquipmentSlot
    {
        Mouth,      // 입
        Shell,      // 등
        Legs        // 다리
    }
    // [바닥 재질] - 발소리 및 굴러가는 소리 결정
    public enum SurfaceType
    {
        Dirt,       // 흙 (기본)
        Stone,      // 돌/바위 (딱딱한 소리)
        Grass,      // 풀/낙엽 (부스럭 소리)
        Water,      // 물 (첨벙 소리)
        Mud,        // 진흙 (질척이는 소리)
        Wood        // 나무 (통통 소리)
    }
}