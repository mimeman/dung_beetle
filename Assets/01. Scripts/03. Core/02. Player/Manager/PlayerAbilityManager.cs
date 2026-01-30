using UnityEngine;
using Dung.Data;

public class PlayerAbilityManager : MonoBehaviour
{
    #region Variables
    private PlayerStats _stats;

    // 점프 관련
    private float _jumpCooldownTimer;

    #endregion

    #region Properties
    // 점프 가능 여부
    public bool CanJump => _jumpCooldownTimer <= 0f;

    #endregion

    #region Initialization
    public void Initialize(PlayerStats stats)
    {
        _stats = stats;
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
        Debug.Log("모든 쿨타임 강제 리셋");
    }
    #endregion
}