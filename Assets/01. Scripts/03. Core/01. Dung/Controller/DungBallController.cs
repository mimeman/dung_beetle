using Dung.Data;
using Dung.Enums;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DungBallController : MonoBehaviour, IDungInteractable, IGrowable
{
    [Header("Data Reference")]
    [SerializeField] private DungStats _stats;

    [Header("Handlers")]
    private DungInteractionHandler _interactionHandler;
    [SerializeField] private Transform _leftHandTarget;
    [SerializeField] private Transform _rightHandTarget;

    private Rigidbody _rb;
    private float _currentMass;
    private float _currentRadius;

    // --- IDungInteractable 구현 ---
    public bool IsInteractable => !_interactionHandler.IsOccupied;

    // --- IGrowable 인터페이스 구현 ---
    public float CurrentMass => _currentMass;
    public float CurrentRadius => _currentRadius;
    public bool IsMaxSize => _currentRadius >= _stats.growth.maxRadius;
    public float WeatherResistance => _stats.physics.weatherResistance;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _interactionHandler = GetComponent<DungInteractionHandler>();
        if (_interactionHandler == null) _interactionHandler = gameObject.AddComponent<DungInteractionHandler>();

        InitializeBall();
    }

    private void InitializeBall()
    {
        _currentMass = _stats.growth.initialMass;
        _currentRadius = _stats.growth.initialRadius;

        _rb.mass = _currentMass;
        _rb.drag = _stats.physics.friction;

        _interactionHandler.Initialize(_stats);
        _interactionHandler.RefreshIKPositions(_currentRadius);
    }

    // --- IDungInteractable 구현부 ---
    public void OnPushStart(GameObject user) => _interactionHandler.Occupy(user);
    public void OnPushEnd(GameObject user) => _interactionHandler.Release();

    public (Transform left, Transform right) GetIKTargets()
    {
        if (_leftHandTarget != null && _rightHandTarget != null)
        {
            Debug.Log("<color=green>[Dung]</color> 수동 IK 타겟 전달 중");
            return (_leftHandTarget, _rightHandTarget);
        }

        Debug.Log("<color=orange>[Dung]</color> 수동 타겟 없음, 핸들러 확인 중...");
        var anchors = _interactionHandler.GetPushAnchors();

        if (anchors.left == null) Debug.LogWarning("<color=red>[Dung]</color> IK 타겟이 최종적으로 Null입니다!");
        return anchors;
    }

    public Vector3 GetPosition() => transform.position;
    public Transform GetTransform() => transform;

    // --- IGrowable 구현: 성장 로직 ---
    public bool Grow(float amount, GrowthType type)
    {
        if (IsMaxSize) return false;

        // 1. 질량 증가 및 물리 적용
        _currentMass = Mathf.Min(_currentMass + amount, _stats.growth.maxMass);
        _rb.mass = _currentMass;

        // 2. 질량에 비례하여 반지름 계산
        float t = (_currentMass - _stats.growth.initialMass) / (_stats.growth.maxMass - _stats.growth.initialMass);
        _currentRadius = Mathf.Lerp(_stats.growth.initialRadius, _stats.growth.maxRadius, t);

        // 3. 비주얼 크기 변경 (직경 = 반지름 * 2)
        transform.localScale = Vector3.one * (_currentRadius * 2f);

        // 4. IK 타겟 위치 갱신
        _interactionHandler.RefreshIKPositions(_currentRadius);

        return true;
    }

    public bool Shrink(float amount, ShrinkType type) => false;
    public void Crumble() { /* 파괴 연출 로직 */ }
}