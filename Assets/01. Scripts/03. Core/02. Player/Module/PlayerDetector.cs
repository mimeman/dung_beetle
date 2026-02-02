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
        Ray ray = new Ray(origin, direction);

        if (Physics.SphereCast(ray, radius, out RaycastHit hit, range, layer))
        {
            if (hit.collider.TryGetComponent(out IDungInteractable dung))
            {
                CurrentInteractable = dung;
                return;
            }
        }

        CurrentInteractable = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (_player == null) return;

        float radius = _player.Stats.detection.detectRadius;
        float range = _player.Stats.detection.detectRange;

        Vector3 origin = _player.transform.position + Vector3.up * 0.5f;
        Vector3 direction = _player.transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(origin + direction * range, radius);
        Gizmos.DrawLine(origin, origin + direction * range);
    }
}