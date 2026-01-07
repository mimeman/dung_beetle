using Dung.Data;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    #region Properties
    [Header("Animal 설정")]
    [SerializeField] private AnimalConfig config;
    public AnimalConfig Config { get { return config; } }

    [Header("Animation Hash 설정")]
    [SerializeField] private AnimalAnimationConfig animConfig;

    public AnimalBaseState<AIController> CurrentState { get; private set; }
    public AnimalStateMachine stateMachine { get; private set; }
    public AnimalSensor sensor { get; private set; }
    public GameObject target { get; private set; }
    public Vector3 currentDestination { get; private set; }
    public bool attacked { get; private set; }
    public bool arrivedAtDestination
    {
        get
        {
            if (agent)
                return !agent.pathPending && agent.remainingDistance <= config.stoppingDistance;
            else
                return Vector3.Distance(transform.position, currentDestination) < config.stoppingDistance;
        }
    }

    private Animator animator;
    private NavMeshAgent agent;
    private bool isHost;
    #endregion

    void Awake()
    {
        sensor = GetComponent<AnimalSensor>();
        agent = GetComponent<NavMeshAgent>();

        InitializeAnimationHashes();
    }

    void Start()
    {
        // isHost = NetworkManager.Instance != null && NetworkManager.Instance.IsHost;

        if (!isHost)
        {   // Client인 경우 : navmeshagent와 sensor 비활성화
            if (null != agent) agent.enabled = false;
            if (null != sensor) sensor.enabled = false;
            return;
        }
        else
        {   // Host인 경우 : (Singleplayer도 포함되어있습니다.)
            /// TODO : 필요한 경우 AddListener작업 해줍시다.
        }
        ChangeState(stateMachine.IdleState);
    }
    void OnDisable()
    {
        // TODO : event Listener를 제거해줍시다.
    }

    void InitializeAnimationHashes()
    {
        // hashMoveSpeed = Animator.StringToHash(animConfig.hash);
        // hashIsWalking = Animator.StringToHash(animConfig.hash);
        // hashIsRunning = Animator.StringToHash(animConfig.hash);
        // hashAttack1 = Animator.StringToHash(animConfig.hash);
        // hashAttack2 = Animator.StringToHash(animConfig.hash);
        // hashAttack3 = Animator.StringToHash(animConfig.hash);
        // hashAttack4 = Animator.StringToHash(animConfig.hash);
        // hashHit = Animator.StringToHash(animConfig.hash);
        // hashDie = Animator.StringToHash(animConfig.hash);
    }

    void Update()
    {
        if (!isHost) return;
        if (CurrentState == null)// || health.isDead)
            return;

        // Target update
        target = sensor.Target;

        // 다음 State로 넘어가기 위한 state의 updateState 로직
        AnimalBaseState<AIController> nextState = CurrentState.UpdateState(this);
        if (nextState != CurrentState)
            ChangeState(nextState);
    }

    void ChangeState(AnimalBaseState<AIController> newState)
    {
        CurrentState?.ExitState(this);
        CurrentState = newState;
        CurrentState?.EnterState(this);
    }

    #region Movement
    // 일반 이동 함수
    public void MoveTo(Vector3 destination)
    {
        currentDestination = destination;
        float speed = (CurrentState == stateMachine.TraceState) ? config.runSpeed : config.walkSpeed;

        // movement.Move(destination, speed);

        // if (!agent)
        //     movement.TurnToward(destination, config.rotateSpeed);
    }
    // 특정 방향으로의 이동 함수
    public void MoveDirection(Vector3 direction, float speed)
    {
        if (!agent)
        {
            agent.speed = speed;
            agent.SetDestination(transform.position + direction);
        }
        // else
        //     movement.Move(transform.position + direction, speed);
    }
    public void StopMoving()
    {
        // movement.Stop();
    }
    public void LookAt(Vector3 target)
    {
        // movement.TurnTowards(target, config.rotateSpeed);
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
    public float GetDistanceToTarget()
    {
        if (!target)
            return Mathf.Infinity;
        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPos = new Vector3(target.transform.position.x, 0, target.transform.position.z);
        return Vector3.Distance(myPos, targetPos);
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