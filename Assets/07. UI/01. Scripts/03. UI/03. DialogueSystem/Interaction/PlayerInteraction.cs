using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _detectRadius = 3.0f;
    [SerializeField] private LayerMask _npcLayer;

    private Collider[] colliders = new Collider[10];
    private NPCInteraction _currentNPC; // 현재 감지된 NPC

    void Update()
    {
        FindClosestNPC();
        HandleInput();
    }

    // 1. 내 주변에서 가장 가까운 NPC 찾기 (조준 필요 X)
    private void FindClosestNPC()
    {
        // 내 위치(transform.position) 주변의 모든 NPC 콜라이더를 가져옴
        Physics.OverlapSphereNonAlloc(transform.position, _detectRadius, colliders, _npcLayer);

        NPCInteraction closestNPC = null;
        float minDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (!col) continue;

            // 거리 재기
            float distance = Vector3.Distance(col.transform.position, transform.position);
            if (distance > _detectRadius)
                continue;

            NPCInteraction npc = col.GetComponent<NPCInteraction>();
            if (npc != null)
            {
                // 거리를 재서 가장 가까운 놈을 찾음
                float dist = Vector3.Distance(transform.position, npc.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestNPC = npc;
                }
            }
        }

        // [상황 A] 새로운 NPC가 감지됨
        if (_currentNPC != closestNPC)
        {
            // 기존 거 끄기
            if (_currentNPC != null) _currentNPC.SetFocused(false);

            // 새 거 등록
            _currentNPC = closestNPC;

            if (_currentNPC != null)
            {
                _currentNPC.SetFocused(true);
                Debug.Log($"[Player] NPC 감지됨! -> {_currentNPC.name}");
            }
        }
    }

    // 2. 키 입력 처리
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_currentNPC != null)
            {
                // UI가 켜져있으면 무시
                if (UIManager.Instance != null && UIManager.Instance.IsPopupOpen) return;

                Debug.Log($"[Player] 대화 시도 -> {_currentNPC.name}");
                _currentNPC.Interact(transform.position);
            }
            else
            {
                Debug.Log("[Player] 주변에 대화할 NPC가 없습니다.");
            }
        }
    }

    // 3. 에디터에서 감지 범위 눈으로 보기
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectRadius);
    }
}