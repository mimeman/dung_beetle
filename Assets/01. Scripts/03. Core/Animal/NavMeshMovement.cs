using Dung.Data;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMeshAgent를 사용한 동물 이동 관리
/// LOD 시스템을 통해 먼 거리에서는 NavMesh 없이 수동 이동
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshMovement : MonoBehaviour
{
    #region Constants
    private const int GROUND_CHECK_FRAME_INTERVAL = 30;
    private const float GROUND_CHECK_DISTANCE = 2f;
    private const float MANUAL_ROTATION_SPEED = 5f;
    #endregion

    #region Serialized Fields
    [Header("Movement Settings")]
    [Tooltip("가속도")]
    [SerializeField] private float accelerationRate = 5f;

    [Tooltip("감속도")]
    [SerializeField] private float decelerationRate = 10f;

    [Header("LOD Settings")]
    [Tooltip("지면 감지 레이어")]
    [SerializeField] private LayerMask groundLayer;
    #endregion

    #region Private Fields
    private Animator animator;
    private Vector3 destination;
    private AnimalAnimationConfig animationConfig;
    private NavMeshAgent agent;
    private float targetSpeed;
    private float stoppingDistance;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeNavMeshAgent();
    }

    private void Update()
    {
        UpdateMovement();
    }
    #endregion

    #region Initialization
    private void InitializeNavMeshAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent)
        {
            agent.updateRotation = true;
            stoppingDistance = agent.stoppingDistance;
        }
    }

    public void SetUp(Animator animator, AnimalAnimationConfig animationConfig)
    {
        this.animator = animator;
        this.animationConfig = animationConfig;
    }
    #endregion

    #region Movement Update
    private void UpdateMovement()
    {
        if (agent == null) return;

        if (agent.enabled)
        {
            UpdateNavMeshMovement();
        }
        else
        {
            SimulateLODMovement();
        }
    }

    private void UpdateNavMeshMovement()
    {
        float currentSpeed = agent.speed;
        float accelerationFactor = (currentSpeed < targetSpeed) ? accelerationRate : decelerationRate;

        agent.speed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * accelerationFactor);

        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        if (animator && animationConfig != null)
        {
            float velocity = agent.velocity.magnitude;
            animator.SetFloat(animationConfig.moveSpeedFloat, velocity);
        }
    }
    #endregion

    #region LOD Movement
    /// <summary>
    /// NavMeshAgent가 비활성화된 경우 수동으로 이동을 시뮬레이션
    /// </summary>
    private void SimulateLODMovement()
    {
        Vector3 direction = (destination - transform.position).normalized;
        float distanceSq = (transform.position - destination).sqrMagnitude;
        float stoppingDistanceSq = stoppingDistance * stoppingDistance;

        // 목적지에 거의 도착했으면 이동 중지
        if (distanceSq <= stoppingDistanceSq)
            return;

        // 이동
        MoveTowardsDestination(direction);

        // 회전
        RotateTowardsDirection(direction);

        // 주기적으로 지면 높이 보정
        CorrectGroundHeight();
    }

    private void MoveTowardsDestination(Vector3 direction)
    {
        transform.position += direction * targetSpeed * Time.deltaTime;
    }

    private void RotateTowardsDirection(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * MANUAL_ROTATION_SPEED
        );
    }

    private void CorrectGroundHeight()
    {
        // 최적화: 매 프레임이 아닌 일정 간격으로만 체크
        if (Time.frameCount % GROUND_CHECK_FRAME_INTERVAL != 0)
            return;

        Vector3 rayOrigin = transform.position + Vector3.up;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, GROUND_CHECK_DISTANCE, groundLayer))
        {
            Vector3 correctedPosition = transform.position;
            correctedPosition.y = hit.point.y;
            transform.position = correctedPosition;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 지정된 목적지로 이동
    /// </summary>
    public void Move(Vector3 destination, float speed)
    {
        targetSpeed = speed;
        this.destination = destination;

        if (agent != null && agent.enabled)
        {
            agent.SetDestination(destination);
        }
    }

    /// <summary>
    /// 특정 위치를 향해 회전
    /// </summary>
    public void TurnTowards(Vector3 worldTargetPosition, float turnSpeed)
    {
        if (agent != null && agent.enabled)
        {
            // NavMeshAgent가 활성화된 경우 제자리에 멈춤
            agent.SetDestination(transform.position);
        }

        // 목표 위치를 바라봄 (Y축 회전만)
        Vector3 lookPosition = new Vector3(
            worldTargetPosition.x,
            transform.position.y,
            worldTargetPosition.z
        );

        transform.LookAt(lookPosition);
    }

    /// <summary>
    /// 이동 중지
    /// </summary>
    public void Stop()
    {
        if (agent != null && agent.enabled && agent.hasPath)
        {
            agent.ResetPath();
        }

        targetSpeed = 0f;
    }
    #endregion
}