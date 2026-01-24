using UnityEngine;
using Dung.Data;

public class PlayerAbilityManager : MonoBehaviour
{
    #region Variables
    private PlayerStats _stats;

    // 점프 관련
    private float _jumpCooldownTimer;

    // 비행 관련
    private float _flyCooldownTimer;
    private float _flyDurationTimer;
    private bool _isFlying;
    #endregion

    #region Properties
    // 점프 가능 여부
    public bool CanJump => _jumpCooldownTimer <= 0f;

    // 비행 가능 여부 (쿨타임 끝 + 비행 중 아님)
    public bool CanFly => _flyCooldownTimer <= 0f && !_isFlying;

    // 현재 비행 중인지
    public bool IsFlying => _isFlying;

    // 남은 비행 시간 (초)
    public float FlyTimeRemaining => Mathf.Max(0, _stats.flight.maxDuration - _flyDurationTimer);

    // 비행 쿨타임 진행도
    public float CooldownProgress => _flyCooldownTimer / _stats.flight.cooldown;
    #endregion

    #region Initialization
    public void Initialize(PlayerStats stats)
    {
        _stats = stats;
        _jumpCooldownTimer = 0f; // [추가] 점프 초기화
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
        // 점프 쿨타임 감소
        if (_jumpCooldownTimer > 0)
        {
            _jumpCooldownTimer -= Time.deltaTime;
        }
    }
    #endregion

    #region Jump Control
    // 점프 쿨타임 시작 (착지 시 AirState에서 호출 예정)
    public void StartJumpCooldown()
    {
        // Stats에 설정된 jumpCooldown 값을 사용
        _jumpCooldownTimer = _stats.physics.jumpCooldown;
        Debug.Log($"점프 쿨타임 시작: {_stats.physics.jumpCooldown}초");
    }
    #endregion


    #region Cheat/Debug
    // 강제 쿨타임 리셋
    [ContextMenu("Reset Cooldown")]
    public void ResetCooldown()
    {
        _jumpCooldownTimer = 0f;
        _flyCooldownTimer = 0f;
        _isFlying = false;
        Debug.Log("모든 쿨타임 강제 리셋");
    }
    #endregion
}