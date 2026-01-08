using Dung.Data;
using UnityEngine;
using UnityEngine.Events;

public class AnimalHealth : MonoBehaviour
{
    #region Properties
    [Header("Realtime Health State")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float defense;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isDead;

    public float MaxHP { get { return maxHealth; } }
    public float CurrentHP { get { return currentHealth; } }
    public bool IsDead { get { return isDead; } }
    public int hitCounter { get; private set; } = 0;

    [HideInInspector] public UnityEvent OnHit;
    [HideInInspector] public UnityEvent OnDeath;
    public System.Action<float> OnHealthChanged;

    private float hitTimer = 0;
    #endregion

    void Update()
    {
        if (hitTimer > 0)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0)
                hitCounter = 0;
        }
    }

    public void Initialize(AnimalConfig config)
    {
        maxHealth = config.maxHP;
        defense = config.defense;
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float prevHealth = currentHealth;
        float calcDamage = Mathf.Max(damage - defense, 0f);
        currentHealth -= calcDamage;
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            OnDeath?.Invoke();
        }
        else
        {
            OnHit?.Invoke();
        }
    }

    public void SpawnLoot()
    {
        if (!NetworkManager.Instance || !NetworkManager.Instance.IsHost)
            return;

        // TODO : Loot Spawn Logic here.
        float groundY = transform.position.y;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 100f))
            groundY = hit.point.y;

        float dropRate = 0.2f;
        if (Random.Range(0, 100) <= dropRate)
        {
            float scatterDistance = 1.0f;
            Vector2 randomCircle = Random.insideUnitCircle * scatterDistance;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            spawnPos.y = groundY + 0.5f;

            // DO Spawn here
        }
    }

    #region Network
    public void SetHealthFromNetwork(float current, float max)
    {
        if (max != maxHealth)
            maxHealth = max;
        if (current != currentHealth)
            currentHealth = current;

        bool wasDead = isDead;
        isDead = currentHealth <= 0;

        if (!wasDead && isDead)
        {
            OnDeath?.Invoke();
            Debug.Log($"[Network Sync] {gameObject.name} is Dead from Network.");
        }
    }
    #endregion

}