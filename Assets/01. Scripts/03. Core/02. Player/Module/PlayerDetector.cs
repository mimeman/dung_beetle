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

        // Stats에서 기획 수치를 실시간으로 가져옴
        float radius = _player.Stats.detection.detectRadius;
        float range = _player.Stats.detection.detectRange;
        LayerMask layer = _player.Stats.detection.interactLayer;

        // SphereCast를 사용하여 너비가 있는 감지 수행
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);

        if (UnityEngine.Physics.SphereCast(ray, radius, out RaycastHit hit, range, layer))
        {
            if (hit.collider.TryGetComponent(out IDungInteractable dung))
            {
                CurrentInteractable = dung;
                Debug.Log("공 감지 성공");
                return;
            }
        }

        CurrentInteractable = null;
    }
}