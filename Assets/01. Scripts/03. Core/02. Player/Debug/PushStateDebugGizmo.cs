using UnityEngine;

/// <summary>
/// 씬 뷰에서 Push 상태의 IK 타겟, 공 앵커, 거리를 시각화합니다.
/// PlayerController가 있는 GameObject에 붙이세요.
/// </summary>
public class PushStateDebugGizmo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController _player;

    [Header("Gizmo Settings")]
    [SerializeField] private float _sphereSize = 0.03f;
    [SerializeField] private bool _showAlways = true;

    private void Reset()
    {
        _player = GetComponent<PlayerController>();
    }

    private void OnDrawGizmos()
    {
        if (!_showAlways || _player == null) return;
        DrawPushGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (_showAlways || _player == null) return;
        DrawPushGizmos();
    }

    private void DrawPushGizmos()
    {
        if (!Application.isPlaying) return;
        if (_player.StateMachine?.CurrentState is not PlayerPushState) return;

        Transform ikTargets = _player.transform.Find("IKTargets");
        if (ikTargets == null) return;

        // ── 뒷다리 IK 타겟 (노란색) ──
        DrawIKTarget(ikTargets, "LeftBackLegTarget", Color.yellow, "LB");
        DrawIKTarget(ikTargets, "RightBackLegTarget", Color.yellow, "RB");

        // ── 앞다리 IK 타겟 (초록색) ──
        DrawIKTarget(ikTargets, "LeftFrontLegTarget", Color.green, "LF");
        DrawIKTarget(ikTargets, "RightFrontLegTarget", Color.green, "RF");

        // ── 공 표면 IK 앵커 (빨간색) ──
        var detector = _player.Detector;
        if (detector != null && detector.CurrentInteractable != null)
        {
            var dung = detector.CurrentInteractable;
            var anchors = dung.GetIKTargets();

            if (anchors.left != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(anchors.left.position, _sphereSize);
                DrawLabel(anchors.left.position, "Anchor L");
            }
            if (anchors.right != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(anchors.right.position, _sphereSize);
                DrawLabel(anchors.right.position, "Anchor R");
            }

            // ── pushDistance 링 (시안) ──
            Vector3 dungPos = dung.GetPosition();
            float radius = (dung is DungBallController dc) ? dc.CurrentRadius : 0.5f;
            float pushDist = radius + _player.Stats.push.pushDistanceOffset;

            Gizmos.color = Color.cyan;
            DrawCircle(dungPos, pushDist, 32);

            // ── 공 반지름 (회색) ──
            Gizmos.color = new Color(1, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(dungPos, radius);
        }

        // ── 플레이어 → 공 연결선 (흰색) ──
        if (detector?.CurrentInteractable != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(_player.transform.position, detector.CurrentInteractable.GetPosition());
        }
    }

    private void DrawIKTarget(Transform parent, string childName, Color color, string label)
    {
        Transform target = parent.Find(childName);
        if (target == null) return;

        Gizmos.color = color;
        Gizmos.DrawSphere(target.position, _sphereSize);
        Gizmos.DrawLine(_player.transform.position, target.position);
        DrawLabel(target.position, label);
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float step = 360f / segments;
        Vector3 prev = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = step * i * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    private void DrawLabel(Vector3 position, string text)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(position + Vector3.up * 0.05f, text);
#endif
    }
}