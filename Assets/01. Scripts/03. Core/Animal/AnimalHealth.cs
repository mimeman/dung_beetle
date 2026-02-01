using Dung.Data;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 동물의 체력 및 데미지 처리를 관리하는 컴포넌트
/// </summary>
public class AnimalHealth : MonoBehaviour
{
    #region Constants
    private const float HIT_COUNTER_RESET_TIME = 1.0f;
    private const float LOOT_DROP_RATE = 0.2f;
    private const float LOOT_SCATTER_DISTANCE = 1.0f;
    private const float GROUND_CHECK_HEIGHT = 0.5f;
    private const float GROUND_CHECK_DISTANCE = 100f;
    private const float LOOT_SPAWN_HEIGHT = 0.5f;
    #endregion

    #region Serialized Fields
    [Header("Realtime Health State")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float defense;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isDead;
    #endregion

    #region Properties
    public float MaxHP => maxHealth;
    public float CurrentHP => currentHealth;
    public bool IsDead => isDead;
    public int HitCounter { get; private set; }
    #endregion

    #region Events
    [HideInInspector] public UnityEvent OnHit;
    [HideInInspector] public UnityEvent OnDeath;
    public System.Action<float> OnHealthChanged;
    #endregion

    #region Private Fields
    private float hitTimer;
    #endregion

    #region Unity Lifecycle
    private void Update()
    {
        UpdateHitCounter();
    }
    #endregion

    #region Initialization
    public void Initialize(AnimalConfig config)
    {
        if (config == null)
        {
            Debug.LogError($"[AnimalHealth] Initialize - config is null");
            return;
        }

        maxHealth = config.maxHP;
        defense = config.defense;
        currentHealth = maxHealth;
        isDead = false;
        HitCounter = 0;
        hitTimer = 0f;
    }
    #endregion

    #region Health Management
    /// <summary>
    /// 데미지를 받습니다
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        float actualDamage = CalculateDamage(damage);
        ApplyDamage(actualDamage);

        IncrementHitCounter();

        CheckDeath();
    }

    private float CalculateDamage(float rawDamage)
    {
        return Mathf.Max(rawDamage - defense, 0f);
    }

    private void ApplyDamage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0f);
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void CheckDeath()
    {
        if (currentHealth <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
        else
        {
            OnHit?.Invoke();
        }
    }

    private void IncrementHitCounter()
    {
        HitCounter++;
        hitTimer = HIT_COUNTER_RESET_TIME;
    }

    private void UpdateHitCounter()
    {
        if (hitTimer <= 0)
            return;

        hitTimer -= Time.deltaTime;

        if (hitTimer <= 0)
            HitCounter = 0;
    }
    #endregion

    #region Loot System
    /// <summary>
    /// 전리품을 스폰합니다
    /// </summary>
    public void SpawnLoot()
    {
        if (!IsHostOrSinglePlayer())
            return;

        if (!ShouldDropLoot())
            return;

        Vector3 spawnPosition = CalculateLootSpawnPosition();
        SpawnLootAtPosition(spawnPosition);
    }

    private bool IsHostOrSinglePlayer()
    {
        return NetworkManager.Instance == null || NetworkManager.Instance.IsHost;
    }

    private bool ShouldDropLoot()
    {
        float randomValue = Random.Range(0f, 1f);
        return randomValue <= LOOT_DROP_RATE;
    }

    private Vector3 CalculateLootSpawnPosition()
    {
        // 지면 높이 찾기
        float groundY = transform.position.y;
        Vector3 rayOrigin = transform.position + Vector3.up * GROUND_CHECK_HEIGHT;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, GROUND_CHECK_DISTANCE))
            groundY = hit.point.y;

        // 랜덤한 원형 범위 내 위치 계산
        Vector2 randomCircle = Random.insideUnitCircle * LOOT_SCATTER_DISTANCE;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        spawnPosition.y = groundY + LOOT_SPAWN_HEIGHT;

        return spawnPosition;
    }

    private void SpawnLootAtPosition(Vector3 position)
    {
        // TODO: 실제 전리품 스폰 로직 구현
        Debug.Log($"[AnimalHealth] Spawning loot at {position}");
    }
    #endregion

    #region Network Synchronization
    /// <summary>
    /// 네트워크에서 체력 정보를 동기화합니다
    /// </summary>
    public void SetHealthFromNetwork(float current, float max)
    {
        bool healthChanged = false;

        if (max != maxHealth)
        {
            maxHealth = max;
            healthChanged = true;
        }

        if (current != currentHealth)
        {
            currentHealth = current;
            healthChanged = true;
        }

        bool wasDead = isDead;
        isDead = currentHealth <= 0;

        // 사망 상태 변경 감지
        if (!wasDead && isDead)
        {
            OnDeath?.Invoke();
            Debug.Log($"[Network Sync] {gameObject.name} died (synced from network)");
        }

        // 체력 변경 이벤트 발생
        if (healthChanged)
        {
            OnHealthChanged?.Invoke(currentHealth);
        }
    }
    #endregion
}