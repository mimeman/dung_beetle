using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    private PlayerController _player;
    public IDungInteractable CurrentInteractable { get; private set; }

    public void Initialize(PlayerController player)
    {
        _player = player;
    }

    private void Update()
    {
        if (_player == null) return;

        float radius = _player.Stats.detection.detectRadius;
        float range = _player.Stats.detection.detectRange;
        LayerMask layer = _player.Stats.detection.interactLayer;

        Vector3 origin = _player.transform.position + Vector3.up * 0.5f;
        Vector3 direction = _player.transform.forward;

        // 앞쪽 영역 내 모든 콜라이더 감지
        Vector3 boxCenter = origin + direction * (range * 0.5f);
        Vector3 boxSize = new Vector3(radius * 2f, radius * 2f, range);
        Quaternion boxRotation = _player.transform.rotation;

        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize * 0.5f, boxRotation, layer);

        IDungInteractable closestDung = null;
        float closestDistance = float.MaxValue;

        // 가장 가까운 똥 찾기
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out IDungInteractable dung))
            {
                float distance = Vector3.Distance(origin, col.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDung = dung;
                }
            }
        }

        CurrentInteractable = closestDung;
    }

    private void OnDrawGizmosSelected()
    {
        if (_player == null) return;

        float radius = _player.Stats.detection.detectRadius;
        float range = _player.Stats.detection.detectRange;

        Vector3 origin = _player.transform.position + Vector3.up * 0.5f;
        Vector3 direction = _player.transform.forward;

        // 감지 박스 영역 표시
        Vector3 boxCenter = origin + direction * (range * 0.5f);
        Vector3 boxSize = new Vector3(radius * 2f, radius * 2f, range);

        Gizmos.color = CurrentInteractable != null ? Color.green : Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, _player.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = Matrix4x4.identity;

        // 감지된 똥 표시
        if (CurrentInteractable != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, CurrentInteractable.GetPosition());
            Gizmos.DrawWireSphere(CurrentInteractable.GetPosition(), 0.3f);
        }
    }
}