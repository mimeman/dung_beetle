using System.Collections;
using Dung.Data;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AnimalSensor))]
public class AIController : MonoBehaviour
{
    #region Properties
    [Header("Components")]
    [SerializeField] protected Animator animator;

    [Header("Animal Settings")]
    [SerializeField] protected AnimalConfig config;
    public AnimalConfig Config { get { return config; } }

    [Header("Animation Hash Settings")]
    [SerializeField] protected AnimalAnimationConfig animConfig;

    public BaseState<AIController> CurrentState { get; private set; }
    public BaseStateMachine stateMachine { get; private set; }
    public AnimalSensor sensor { get; private set; }
    public GameObject target { get; protected set; }
    public bool attacked { get; private set; }
    public bool isAnyPlayerNear { get { return sensor.IsAnyPlayerNear; } }
    public bool arrivedAtDestination
    {
        get
        {
            if (agent && agent.enabled)
                return !agent.pathPending && agent.remainingDistance <= config.stoppingDistance;
            else
                return Vector3.Distance(transform.position, currentDestination) < config.stoppingDistance;
        }
    }

    private NavMeshMovement movement;   // 추후 interface로 받기
    protected AnimalHealth health;
    private NavMeshAgent agent;
    private Vector3 currentDestination;
    public void SetDestination(Vector3 dest) { currentDestination = dest; }
    protected bool isHost;
    #endregion

    void Awake()
    {
        stateMachine = GetComponent<BaseStateMachine>();
        movement = GetComponent<NavMeshMovement>();
        health = GetComponent<AnimalHealth>();
        sensor = GetComponent<AnimalSensor>();
        agent = GetComponent<NavMeshAgent>();

        if (agent)
            agent.stoppingDistance = config.stoppingDistance;

        health.Initialize(config);
        InitializeAnimationHashes();
    }

    void Start()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        isHost = NetworkManager.Instance != null && NetworkManager.Instance.IsHost;

        if (!isHost)
        {   // Client인 경우 : navmeshagent와 sensor 비활성화
            if (!agent) agent.enabled = false;
            if (!sensor) sensor.enabled = false;
            return;
        }
        else
        {   // Host인 경우 : (Singleplayer도 포함되어있습니다.)
            health.OnHit.AddListener(HandleHit);
            health.OnDeath.AddListener(HandleHit);

            if (movement)
                movement.SetUp(animator, animConfig);
        }
        ChangeState(stateMachine.IdleState);
    }

    void OnDisable()
    {
        if (health)
            health.OnHit.RemoveAllListeners();
    }

    void Update()
    {
        if (!isHost) return;
        if (CurrentState == null || health.IsDead)
            return;

        // Target update
        target = sensor.Target;

        // 다음 State로 넘어가기 위한 state의 updateState 로직
        BaseState<AIController> nextState = CurrentState.UpdateState(this);
        if (nextState != CurrentState)
            ChangeState(nextState);
    }

    protected void ChangeState(BaseState<AIController> newState)
    {
        CurrentState?.ExitState(this);
        CurrentState = newState;
        CurrentState?.EnterState(this);
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }

    #region Movement
    // 일반 이동 함수
    public void MoveTo(Vector3 destination)
    {
        if (currentDestination == destination)
            return;

        currentDestination = destination;
        float speed = (CurrentState == stateMachine.TraceState) ? config.runSpeed : config.walkSpeed;

        movement.Move(destination, speed);
    }
    // 특정 방향으로의 이동 함수
    public void MoveDirection(Vector3 direction, float speed)
    {
        movement.Move(transform.position + direction, speed);
    }
    public void StopMoving()
    {
        if (movement)
            movement.Stop();
    }
    public void LookAt(Vector3 target)
    {
        movement.TurnTowards(target, config.rotateSpeed);
    }
    public Vector3 GetPatrolDestination()
    {
        float distance = Random.Range(config.patrolMinRadius, config.patrolMaxRadius);
        Vector3 randDirection = Random.onUnitSphere * distance;
        randDirection.y = 0;
        Vector3 destination = transform.position + randDirection;

        // 도착지가 갈수 있는곳인지 raycast확인
        if (Physics.Raycast(destination + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            return hit.point;
        return destination;
    }
    #endregion

    #region Combat
    public float GetDistanceToTarget()
    {
        if (!target)
            return Mathf.Infinity;
        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPos = new Vector3(target.transform.position.x, 0, target.transform.position.z);
        return Vector3.Distance(myPos, targetPos);
    }
    public void ApplyDamageToTarget()
    {
        if (!target || health.IsDead || !isHost)
            return;
        if (GetDistanceToTarget() <= config.attackRange)
        {
            if (target.TryGetComponent<IPlayerControllable>(out IPlayerControllable playerControllable))
            {   // Target이 Player일때
                SessionManager sessionManager = NetworkManager.Instance.SessionManager;
                // 서버 권위적 데미지 처리: 호스트는 데미지를 직접 적용하는 대신, 중앙 관리자(SessionManager)에게 데미지 처리를 요청
                // SessionManager 해당 플레이어 클라이언트에게 데미지를 입으라는 메시지를 보냅니다.
                if (sessionManager)
                    sessionManager.HandleDamage(playerControllable.Id, config.attackDamage);
            }
        }
    }

    private void HandleHit()
    {
        if (!isHost || health.IsDead) return;

        if (stateMachine)
        {
            SetAnimTrigger(hashHit);
            ChangeState(stateMachine.HitState);
        }
    }
    private void HandleDeath()
    {
        if (!isHost) return;

        StopAllCoroutines();

        if (!string.IsNullOrEmpty(animConfig.dieTrigger1))
            SetAnimTrigger(hashDie);

        ChangeState(stateMachine.DieState);

        StartCoroutine(DespawnRoutine());
    }
    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(5.0f);

        // // 스폰 매니저를 통해 몬스터를 풀에 반환하고 모든 클라이언트에게 despawn 메시지를 보냅니다.
        // if (SpawnManager.Instance)
        //     SpawnManager.Instance.ReturnMonsterToPool(gameObject);
        // else

        Destroy(gameObject);
    }
    #endregion

    #region Animation Wrapper
    public int hashMoveSpeed { get; private set; }
    public int hashIsWalking { get; private set; }
    public int hashIsRunning { get; private set; }
    public int hashAttack1 { get; private set; }
    public int hashAttack2 { get; private set; }
    public int hashAttack3 { get; private set; }
    public int hashAttack4 { get; private set; }
    public int hashHit { get; private set; }
    public int hashDie { get; private set; }
    public int hashEat { get; private set; }
    public int hashPoo { get; private set; }
    public int hashSleep { get; private set; }

    protected void InitializeAnimationHashes()
    {
        hashMoveSpeed = Animator.StringToHash(animConfig.moveSpeedFloat);
        hashIsWalking = Animator.StringToHash(animConfig.isWalkingBool);
        hashIsRunning = Animator.StringToHash(animConfig.isRunningBool);
        hashAttack1 = Animator.StringToHash(animConfig.actionTrigger1);
        hashAttack2 = Animator.StringToHash(animConfig.actionTrigger2);
        hashAttack3 = Animator.StringToHash(animConfig.actionTrigger3);
        hashAttack4 = Animator.StringToHash(animConfig.actionTrigger4);
        hashHit = Animator.StringToHash(animConfig.hitTrigger1);
        hashDie = Animator.StringToHash(animConfig.dieTrigger1);
    }

    public void SetAnimBool(int animHash, bool value)
    {
        if (animator) animator.SetBool(animHash, value);
    }

    public void SetAnimTrigger(int animHash)
    {
        if (animator) animator.SetTrigger(animHash);
    }

    public void SetAnimFloat(int animHash, float value)
    {
        if (animator) animator.SetFloat(animHash, value);
    }

    public void SetAnimInt(int animHash, int value)
    {
        if (animator) animator.SetInteger(animHash, value);
    }
    #endregion
}