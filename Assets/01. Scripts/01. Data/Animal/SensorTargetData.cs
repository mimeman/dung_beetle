using UnityEngine;

public enum TargetSearchType
{
    PlayerManager,  // SessionManager에서 플레이어 목록 가져오기
    WorldObject     // Physics.OverlapSphere로 주변 오브젝트 찾기 (Dung 등)
}

[CreateAssetMenu(fileName = "TargetData", menuName = "DungBeetle/AI/Target Data")]
public class SensorTargetData : ScriptableObject
{
    [Header("타겟 식별 정보")]
    public string targetName;        // 에디터 식별용 (예: "Player", "Dung")
    public TargetSearchType searchType; // 탐색 방식 결정

    [Header("물리 연산 설정")]
    public LayerMask targetLayer;    // Physics 탐색 시 사용할 레이어 마스크
}