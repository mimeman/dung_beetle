#region Class Description
/*
 * [PlayerAbilityManager]
 * - 플레이어의 특수 능력 (비행, 대시 등) 관리
 * - 역할:
 * 1. 쿨타임 시스템 관리
 * 2. 지속 시간 추적
 * 3. 사용 가능 여부 판단
 * 
 * - State는 이 클래스를 통해서만 능력 사용 가능 여부 확인
 * - 독립적인 Update로 시간 관리
 */
#endregion

using UnityEngine;
using Dung.Data;

public class PlayerAbilityManager : MonoBehaviour
{
    #region Variables
    private PlayerStats _stats;

    // 비행 관련
    private float _flyCooldownTimer;
    private float _flyDurationTimer;
    private bool _isFlying;
    #endregion

    #region Properties
    //비행 가능 여부 (쿨타임 끝 + 비행 중 아님)
    public bool CanFly => _flyCooldownTimer <= 0f && !_isFlying;

    //현재 비행 중인지
    public bool IsFlying => _isFlying;

    //남은 비행 시간 (초)
    public float FlyTimeRemaining => Mathf.Max(0, _stats.flight.maxDuration - _flyDurationTimer);

    //쿨타임 진행도
    public float CooldownProgress => _flyCooldownTimer / _stats.flight.cooldown;
    #endregion

    #region Initialization
    public void Initialize(PlayerStats stats)
    {
        _stats = stats;
        _flyCooldownTimer = 0f;
        _flyDurationTimer = 0f;
        _isFlying = false;
    }
    #endregion

    #region Unity Lifecycle
    private void Update()
    {
        UpdateCooldowns();
    }
    #endregion

    #region Cooldown Management
    private void UpdateCooldowns()
    {
        // 쿨타임 감소
        if (_flyCooldownTimer > 0)
        {
            _flyCooldownTimer -= Time.deltaTime;
            if (_flyCooldownTimer <= 0)
            {
                _flyCooldownTimer = 0;
                Debug.Log("비행 준비 완료!");
            }
        }

        // 비행 지속시간 증가
        if (_isFlying)
        {
            _flyDurationTimer += Time.deltaTime;
        }
    }
    #endregion

    #region Fly Control
    //비행 시작
    public void StartFly()
    {
        if (!CanFly) return;

        _isFlying = true;
        _flyDurationTimer = 0f;
        Debug.Log("비행 시작!");
    }

    //비행 종료 (쿨타임 시작)
    public void EndFly()
    {
        if (!_isFlying) return;

        _isFlying = false;
        _flyCooldownTimer = _stats.flight.cooldown;
        _flyDurationTimer = 0f;
        Debug.Log($"비행 종료. 쿨타임 {_stats.flight.cooldown}초");
    }

    //비행 시간 초과 체크
    public bool IsFlyDurationOver()
    {
        return _flyDurationTimer >= _stats.flight.maxDuration;
    }
    #endregion

    #region Cheat/Debug
    //강제 쿨타임 리셋 (치트나 아이템용)
    [ContextMenu("Reset Cooldown")]
    public void ResetCooldown()
    {
        _flyCooldownTimer = 0f;
        _isFlying = false;
        Debug.Log("쿨타임 강제 리셋");
    }
    #endregion
}