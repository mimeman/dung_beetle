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
    private DungPhysicsHandler _physicsHandler;

    [SerializeField] private Transform _leftHandTarget;
    [SerializeField] private Transform _rightHandTarget;

    private Rigidbody _rb;
    private float _currentMass;
    private float _currentRadius;

    public bool IsInteractable => !_interactionHandler.IsOccupied;
    public float CurrentMass => _currentMass;
    public float CurrentRadius => _currentRadius;
    public bool IsMaxSize => _currentRadius >= _stats.growth.maxRadius;
    public float WeatherResistance => _stats.physics.weatherResistance;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _interactionHandler = GetComponent<DungInteractionHandler>();
        if (_interactionHandler == null)
            _interactionHandler = gameObject.AddComponent<DungInteractionHandler>();

        _physicsHandler = GetComponent<DungPhysicsHandler>();
        if (_physicsHandler == null)
            _physicsHandler = gameObject.AddComponent<DungPhysicsHandler>();

        InitializeBall();
    }

    private void InitializeBall()
    {
        _currentMass = _stats.growth.initialMass;
        _currentRadius = _stats.growth.initialRadius;

        _rb.mass = _currentMass;
        _rb.drag = _stats.physics.friction;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _interactionHandler.Initialize(_stats);
        _interactionHandler.RefreshIKPositions(_currentRadius);

        _physicsHandler.Initialize(_stats, this);
    }

    public void OnPushStart(GameObject user) => _interactionHandler.Occupy(user);
    public void OnPushEnd(GameObject user) => _interactionHandler.Release();

    public (Transform left, Transform right) GetIKTargets()
    {
        if (_leftHandTarget != null && _rightHandTarget != null)
        {
            return (_leftHandTarget, _rightHandTarget);
        }

        return _interactionHandler.GetPushAnchors();
    }

    public Vector3 GetPosition() => transform.position;
    public Transform GetTransform() => transform;

    public bool Grow(float amount, GrowthType type)
    {
        if (IsMaxSize) return false;

        _currentMass = Mathf.Min(_currentMass + amount, _stats.growth.maxMass);
        _rb.mass = _currentMass;

        float t = (_currentMass - _stats.growth.initialMass) / (_stats.growth.maxMass - _stats.growth.initialMass);
        _currentRadius = Mathf.Lerp(_stats.growth.initialRadius, _stats.growth.maxRadius, t);

        transform.localScale = Vector3.one * (_currentRadius * 2f);

        _interactionHandler.RefreshIKPositions(_currentRadius);

        return true;
    }

    public bool Shrink(float amount, ShrinkType type) => false;
    public void Crumble() { }
}