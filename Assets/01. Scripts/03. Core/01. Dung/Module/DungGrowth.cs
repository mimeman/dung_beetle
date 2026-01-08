using UnityEngine;
using System;
using Dung.Data;
using Dung.Enums;

public class DungGrowth : MonoBehaviour
{
    public event Action<float> OnMassChanged;
    public event Action OnCrumble;

    private DungStats _stats;
    private float _currentMass;

    public float CurrentMass => _currentMass;
    public bool IsMaxSize => _stats != null && _currentMass >= _stats.growth.maxMass;

    // 성장 시스템을 초기화합니다.
    /// <param name="stats">쇠똥 데이터</param>
    public void Initialize(DungStats stats)
    {
        _stats = stats;
        _currentMass = _stats.growth.initialMass;

        OnMassChanged?.Invoke(_currentMass);
    }

    // 저장된 게임을 불러올 때 질량을 복구합니다.
    /// <param name="savedMass">저장된 질량값</param>
    public void LoadMassState(float savedMass)
    {
        _currentMass = savedMass;
        OnMassChanged?.Invoke(_currentMass);
    }

    // 아이템을 흡수하여 성장합니다.
    /// <param name="amount">추가할 질량 (kg)</param>
    /// <param name="type">재료 유형</param>
    public bool CalculateGrowth(float amount, GrowthType type)
    {
        if (_stats == null || IsMaxSize) return false;

        // [공식] 최종 증가량 = 기본 양 * 흡수율
        float addedMass = amount * _stats.growth.absorptionRate;

        _currentMass += addedMass;

        // 최대치 제한
        if (_currentMass > _stats.growth.maxMass)
            _currentMass = _stats.growth.maxMass;

        OnMassChanged?.Invoke(_currentMass);
        return true;
    }

    // 충격이나 날씨로 인해 질량이 감소합니다.
    /// <param name="amount">감소할 질량 (kg)</param>
    /// <param name="type">감소 원인 (Weather, Physical 등)</param>
    public bool CalculateShrink(float amount, ShrinkType type)
    {
        if (_stats == null) return false;

        // 타입별 저항력 적용
        float resistance = type switch
        {
            ShrinkType.Weather => _stats.physics.weatherResistance,
            ShrinkType.Physical => _stats.physics.impactResistance,
            _ => 0f
        };

        // 실제 감소량 = 데미지 * (1 - 저항력)
        float actualDamage = amount * (1.0f - resistance);
        _currentMass -= actualDamage;

        // 최소 질량 체크
        if (_currentMass <= 0)
        {
            _currentMass = 0;
            OnCrumble?.Invoke();
            return false;
        }

        OnMassChanged?.Invoke(_currentMass);
        return true;
    }
}