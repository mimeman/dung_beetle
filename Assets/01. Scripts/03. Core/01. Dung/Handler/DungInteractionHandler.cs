using UnityEngine;
using Dung.Data;

public class DungInteractionHandler : MonoBehaviour
{
    private DungStats _stats;
    private Transform _leftIKTarget;
    private Transform _rightIKTarget;

    private bool _isOccupied;

    public bool IsOccupied => _isOccupied;

    public void Initialize(DungStats stats)
    {
        _stats = stats;
        CreateIKTargets();
    }

    private void CreateIKTargets()
    {
        // IK 타겟 오브젝트 생성 및 초기화
        _leftIKTarget = new GameObject("LeftHand_IK_Target").transform;
        _rightIKTarget = new GameObject("RightHand_IK_Target").transform;

        _leftIKTarget.SetParent(transform);
        _rightIKTarget.SetParent(transform);

        // 초기 반지름 기준으로 위치 설정
        RefreshIKPositions(_stats.growth.initialRadius);
    }

    public void RefreshIKPositions(float currentRadius)
    {
        if (_stats == null || _leftIKTarget == null) return;

        // 1. 방향 벡터 추출 및 정규화
        Vector3 leftDir = _stats.visual.leftHandOffset.normalized;
        Vector3 rightDir = _stats.visual.rightHandOffset.normalized;

        // 2. 방향 * 현재 반지름으로 위치 확정
        _leftIKTarget.localPosition = leftDir * currentRadius;
        _rightIKTarget.localPosition = rightDir * currentRadius;

        // 3. 회전은 항상 중심을 바라보도록 설정
        _leftIKTarget.LookAt(transform.position);
        _rightIKTarget.LookAt(transform.position);
    }

    public (Transform left, Transform right) GetPushAnchors()
    {
        return (_leftIKTarget, _rightIKTarget);
    }

    // 플레이어 점유 관리 (상태를 바꾸며 타겟 반환)
    public (Transform left, Transform right) Occupy(GameObject user)
    {
        _isOccupied = true;
        return (_leftIKTarget, _rightIKTarget);
    }

    public void Release() => _isOccupied = false;
}