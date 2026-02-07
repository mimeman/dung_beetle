using System.Collections;
using System.ComponentModel;
using Dung.Data;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AI 동물의 핵심 컨트롤러. State Machine 패턴을 사용하여 동물의 행동을 관리합니다.
/// </summary>
[RequireComponent(typeof(AnimalSensor))]
public class AIController : MonoBehaviour
{
    #region Constants
    private const float DESPAWN_DELAY = 5.0f;
    #endregion

    #region Serialized Fields
    [Header("Components")]
    [SerializeField] protected Animator animator;

    [Header("Animal Settings")]
    [SerializeField] protected AnimalConfig config;

    [Header("Animation Hash Settings")]
    [SerializeField] protected AnimalAnimationConfig animConfig;
    #endregion

    #region Properties
    public AnimalConfig Config => config;
    public BaseState<AIController> CurrentState { get; private set; }
    public BaseStateMachine StateMachine { get; private set; }
    public AnimalSensor Sensor { get; private set; }
    public Transform Target { get; protected set; }
    public bool IsAnyPlayerNear => Sensor.IsAnyPlayerNear;

    public bool ArrivedAtDestination
    {
        get
        {
            if (agent && agent.enabled)
                return !agent.pathPending && agent.remainingDistance <= config.stoppingDistance;
            else
                return Vector3.Distance(transform.position, currentDestination) < config.stoppingDistance;
        }
    }
    #endregion

    #region Private Fields
    private NavMeshMovement movement;
    protected AnimalHealth health;
    private NavMeshAgent agent;
    private Vector3 currentDestination;
    protected bool isHost;
    #endregion

    #region Animation Hashes
    public int HashMoveSpeed { get; private set; }
    public int HashIsWalking { get; private set; }
    public int HashIsRunning { get; private set; }
    public int HashIsFlying { get; private set; }
    public int HashIsLanding { get; private set; }
    public int HashFlyDirection { get; private set; }
    public int HashAttack1 { get; private set; }
    public int HashAttack2 { get; private set; }
    public int HashAttack3 { get; private set; }
    public int HashAttack4 { get; private set; }
    public int HashHit { get; private set; }
    public int HashDie { get; private set; }
    public int HashEat { get; private set; }
    public int HashPoo { get; private set; }
    public int HashSleep { get; private set; }
    #endregion

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        StateMachine = GetComponent<BaseStateMachine>();
        movement = GetComponent<NavMeshMovement>();
        health = GetComponent<AnimalHealth>();
        Sensor = GetComponent<AnimalSensor>();
        agent = GetComponent<NavMeshAgent>();

        if (agent)
            agent.stoppingDistance = config.stoppingDistance;

        health.Initialize(config);
    }

    protected virtual void Start()
    {
        Initialize();
    }

    protected virtual void Update()
    {
        if (!isHost) return;
        if (CurrentState == null || health.IsDead) return;

        // Target update
        if (Sensor.Target)
            Target = Sensor.Target.transform;
        else
            Target = null;

        // State machine update
        BaseState<AIController> nextState = CurrentState.UpdateState(this);
        if (nextState != CurrentState)
            ChangeState(nextState);
    }

    protected virtual void OnDisable()
    {
        if (health != null)
        {
            health.OnHit.RemoveListener(HandleHit);
            health.OnDeath.RemoveListener(HandleDeath);
        }
    }
    #endregion

    #region Initialization
    protected void Initialize()
    {
        InitializeAnimationHashes();
        isHost = NetworkManager.Instance != null && NetworkManager.Instance.IsHost;

        if (!isHost)
        {
            // Client: NavMeshAgent와 Sensor 비활성화
            if (agent) agent.enabled = false;
            if (Sensor) Sensor.enabled = false;
            return;
        }

        // Host/Single Player: 이벤트 등록 및 초기화
        health.OnHit.AddListener(HandleHit);
        health.OnDeath.AddListener(HandleDeath);

        if (movement)
            movement.SetUp(config, animator, animConfig);

        ChangeState(StateMachine.IdleState);
    }

    private void InitializeAnimationHashes()
    {
        HashMoveSpeed = Animator.StringToHash(animConfig.moveSpeedFloat);
        HashIsWalking = Animator.StringToHash(animConfig.isWalkingBool);
        HashIsRunning = Animator.StringToHash(animConfig.isRunningBool);
        HashIsFlying = Animator.StringToHash(animConfig.flyingBoolHash);
        HashIsLanding = Animator.StringToHash(animConfig.landingBoolHash);
        HashFlyDirection = Animator.StringToHash(animConfig.flyingDirectionHash);
        HashAttack1 = Animator.StringToHash(animConfig.actionTrigger1);
        HashAttack2 = Animator.StringToHash(animConfig.actionTrigger2);
        HashAttack3 = Animator.StringToHash(animConfig.actionTrigger3);
        HashAttack4 = Animator.StringToHash(animConfig.actionTrigger4);
        HashHit = Animator.StringToHash(animConfig.hitTrigger1);
        HashDie = Animator.StringToHash(animConfig.dieTrigger1);
    }
    #endregion

    #region State Management
    public void ChangeState(BaseState<AIController> newState)
    {
        CurrentState?.ExitState(this);
        CurrentState = newState;
        CurrentState?.EnterState(this);
    }
    #endregion

    #region Movement
    /// <summary>
    /// 지정된 목적지로 이동합니다.
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        if (currentDestination == destination)
            return;

        currentDestination = destination;
        float speed = (CurrentState == StateMachine.TraceState) ? config.runSpeed : config.walkSpeed;
        movement.Move(destination, speed);
    }

    /// <summary>
    /// 특정 방향으로 이동합니다.
    /// </summary>
    public void MoveDirection(Vector3 direction, float speed)
    {
        movement.Move(transform.position + direction, speed);
    }

    /// <summary>
    /// 이동을 중지합니다.
    /// </summary>
    public void StopMoving()
    {
        if (movement)
            movement.Stop();
    }

    /// <summary>
    /// 특정 위치를 바라봅니다.
    /// </summary>
    public void LookAt(Vector3 target)
    {
        movement.TurnTowards(target, config.rotateSpeed);
    }

    /// <summary>
    /// 랜덤한 순찰 목적지를 반환합니다.
    /// </summary>
    public Vector3 GetPatrolDestination()
    {
        float distance = Random.Range(config.patrolMinRadius, config.patrolMaxRadius);
        Vector3 randomDirection = Random.onUnitSphere * distance;
        randomDirection.y = 0;
        Vector3 destination = transform.position + randomDirection;

        // 목적지가 이동 가능한 위치인지 Raycast로 확인
        if (Physics.Raycast(destination + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            return hit.point;

        return destination;
    }

    public void SetDestination(Vector3 dest)
    {
        currentDestination = dest;
    }
    #endregion

    #region Combat
    /// <summary>
    /// 타겟까지의 거리를 반환합니다 (Y축 무시).
    /// </summary>
    public float GetDistanceToTarget()
    {
        if (!Target)
            return Mathf.Infinity;

        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPos = new Vector3(Target.transform.position.x, 0, Target.transform.position.z);
        return Vector3.Distance(myPos, targetPos);
    }

    /// <summary>
    /// 타겟에게 데미지를 입힙니다.
    /// </summary>
    public void ApplyDamageToTarget()
    {
        if (!Target || health.IsDead || !isHost)
            return;

        if (GetDistanceToTarget() <= config.attackRange)
        {
            if (Target.TryGetComponent<IPlayerControllable>(out IPlayerControllable playerControllable))
            {
                SessionManager sessionManager = NetworkManager.Instance?.SessionManager;
                if (sessionManager)
                    sessionManager.HandleDamage(playerControllable.Id, config.attackDamage);
            }
        }
    }

    private void HandleHit()
    {
        if (!isHost || health.IsDead) return;

        if (StateMachine)
        {
            SetAnimTrigger(HashHit);
            ChangeState(StateMachine.HitState);
        }
    }

    private void HandleDeath()
    {
        if (!isHost) return;

        StopAllCoroutines();

        if (!string.IsNullOrEmpty(animConfig.dieTrigger1))
            SetAnimTrigger(HashDie);

        ChangeState(StateMachine.DieState);
        StartCoroutine(DespawnRoutine());
    }

    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(DESPAWN_DELAY);
        Destroy(gameObject);
    }
    #endregion

    #region Animation Wrapper
    public void SetAnimBool(int animHash, bool value)
    {
        if (animator)
            animator.SetBool(animHash, value);
    }

    public void SetAnimTrigger(int animHash)
    {
        if (animator)
            animator.SetTrigger(animHash);
    }

    public void SetAnimFloat(int animHash, float value)
    {
        if (animator)
            animator.SetFloat(animHash, value);
    }

    public void SetAnimInt(int animHash, int value)
    {
        if (animator)
            animator.SetInteger(animHash, value);
    }
    #endregion

    #region Utility
    public void Despawn()
    {
        Destroy(gameObject);
    }
    #endregion
}