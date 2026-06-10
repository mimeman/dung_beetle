using UnityEngine;
using Dung.Data;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    private PlayerController player;
    private float currentHP;
    private bool isInvincible;
    private bool isDead;

    public float CurrentHealth => currentHP;
    public float MaxHealth => player.Stats.health.maxHP;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Start()
    {
        currentHP = MaxHealth;
    }

    public void TakeDamage(float amount, GameObject instigator = null)
    {
        if (isInvincible || isDead) return;

        currentHP = Mathf.Max(currentHP - amount, 0f);
        Debug.Log($"[PlayerHealth] Took {amount} damage from {instigator?.name ?? "Unknown"}. Current HP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibleRoutine());
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("<color=red>[PlayerHealth] PLAYER DIED!</color>");
        // TODO: 기절 애니메이션이나 리스폰 로직은 향후 구현
    }

    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        
        float timer = 0f;
        float duration = player.Stats.health.invincibleDuration;
        float blinkInterval = player.Stats.health.blinkInterval;

        // 시각적 깜빡임 (Renderer 활성/비활성)
        // Renderer가 여러 개일 수 있으므로 하위 오브젝트 전체 검색
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        while (timer < duration)
        {
            SetRenderersEnabled(renderers, false);
            yield return new WaitForSeconds(blinkInterval);
            SetRenderersEnabled(renderers, true);
            yield return new WaitForSeconds(blinkInterval);
            
            timer += blinkInterval * 2;
        }

        SetRenderersEnabled(renderers, true);
        isInvincible = false;
    }

    private void SetRenderersEnabled(Renderer[] renderers, bool enabled)
    {
        foreach (var r in renderers)
        {
            if (r != null) r.enabled = enabled;
        }
    }
}
