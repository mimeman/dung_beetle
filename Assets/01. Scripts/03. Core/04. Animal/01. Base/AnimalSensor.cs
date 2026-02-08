using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dung.Data;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 플레이어 감지 및 추적을 담당하는 센서 컴포넌트
/// </summary>
public class AnimalSensor : MonoBehaviour
{
    #region Constants
    private const float DEFAULT_RANGE_MULTIPLIER = 1.5f;
    private const float DEFAULT_MEMORY_DURATION = 3.0f;
    private const float DEFAULT_SENSOR_INTERVAL = 0.2f;
    private const float DEFAULT_ACTIVE_INTERVAL = 0.5f;
    private const float DEFAULT_ACTIVE_RADIUS_SQ = 3000f;
    #endregion

    #region Serialized Fields
    [Header("Sensor Settings")]
    [Tooltip("감지할 방향")]
    [SerializeField] private Direction sensorDirection = Direction.Forward;

    [Tooltip("공격 당했을 시 찾아보려는 최대 거리 배수")]
    [SerializeField] private float rangeMultiplier = DEFAULT_RANGE_MULTIPLIER;

    [Tooltip("대상을 기억하는 유지 시간")]
    [SerializeField] private float memoryDuration = DEFAULT_MEMORY_DURATION;

    [Tooltip("센서 감지 주기")]
    [SerializeField] private float sensorInterval = DEFAULT_SENSOR_INTERVAL;

    [Header("Debug")]
    [SerializeField] private bool enableDebug;
    [SerializeField] private bool isOnSight;
    [SerializeField] private bool isOnHeard;
    [SerializeField] private bool isAnyPlayerNear;

    [Header("LOD Settings")]
    [Tooltip("NavMeshAgent 활성화 범위(제곱값)")]
    [SerializeField] private float activeRadiusSq = DEFAULT_ACTIVE_RADIUS_SQ;

    [Tooltip("Active 감지 주기")]
    [SerializeField] private float activeInterval = DEFAULT_ACTIVE_INTERVAL;
    #endregion

    #region Optimization Variables
    // [최적화] OverlapSphereNonAlloc을 위한 버퍼 크기 제한
    private const int MAX_COLLIDERS = 20;
    // [최적화] 매 프레임 할당을 방지하기 위한 미리 할당된 배열
    private Collider[] _colliderBuffer = new Collider[MAX_COLLIDERS];
    #endregion

    #region Properties
    public GameObject Target { get; private set; }
    public Vector3 TargetLastPosition { get; private set; }
    public bool IsOnSight => isOnSight;
    public bool IsOnHeard => isOnHeard;
    public bool IsAnyPlayerNear => isAnyPlayerNear;
    #endregion

    #region Private Fields
    private SessionManager sessionManager;
    private AnimalConfig config;
    private NavMeshAgent agent;
    private WaitForSeconds sensorDelay;
    private WaitForSeconds activeDelay;
    private AnimalHealth animalHealth;
    private float memoryDelta;
    private bool isHost;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeComponents();
        InitializeNetwork();

        if (isHost)
            StartCoroutine(CheckSensorRoutine());
    }

    private void Update()
    {
        if (!isHost) return;

        // 기억 시간 감소
        if (memoryDelta > 0)
            memoryDelta -= Time.deltaTime;
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        AIController controller = GetComponent<AIController>();
        if (controller)
            config = controller.Config;

        agent = GetComponent<NavMeshAgent>();
        animalHealth = GetComponent<AnimalHealth>();

        sensorDelay = new WaitForSeconds(sensorInterval);
        activeDelay = new WaitForSeconds(activeInterval);
    }

    private void InitializeNetwork()
    {
        if (NetworkManager.Instance != null)
        {
            sessionManager = NetworkManager.Instance.SessionManager;
            isHost = NetworkManager.Instance.IsHost;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 강제로 타겟을 설정합니다. (피격 시 호출) 
    /// </summary>
    public void ForceDetection(GameObject attacker)
    {
        if (!isHost)
        {
            if (attacker == null)
                Debug.LogError($"[AnimalSensor] ForceDetection - attacker is null");
            return;
        }

        if (attacker == null) return;

        // 공격자가 감지 가능 범위 내에 있는지 확인
        float distance = Vector3.Distance(attacker.transform.position, transform.position);
        float maxDetectionRange = config.fovRange * rangeMultiplier;

        if (distance <= maxDetectionRange && attacker.TryGetComponent<IPlayerControllable>(out _))
        {
            SetTarget(attacker);
            isOnSight = true;
            memoryDelta = memoryDuration;
        }
    }
    #endregion

    #region Sensor Logic
    private IEnumerator CheckSensorRoutine()
    {
        while (true)
        {
            FindTarget();

            if (isAnyPlayerNear && !animalHealth.IsDead)
            {
                ActivateAI();
                yield return sensorDelay;
            }
            else
            {
                if (animalHealth.IsDead)
                    ClearTarget();

                DeactivateAI();
                yield return activeDelay;
            }
        }
    }

    private void FindTarget()
    {
        if (sessionManager == null)
            return;

        var players = sessionManager.Players.Values;
        if (players == null || !players.Any())
        {
            ClearTarget();
            return;
        }

        // 최적화된 Target Finding
        GameObject nearestTarget = null;
        float minDistanceSq = activeRadiusSq; // 활성화 범위(제곱)로 초기화
        isAnyPlayerNear = false; // "타겟이 근처에 있어 AI가 활동해야 하는가?"로 의미 확장

        SensorTargetData settings = config.targetSetting;

        // 설정이 누락되었으면 리턴
        if (settings == null) return;

        switch (settings.searchType)
        {
            case TargetSearchType.PlayerManager:
                // 매니저를 통한 플레이어 탐색
                if (sessionManager != null && sessionManager.Players != null)
                {
                    foreach (var player in sessionManager.Players.Values)
                    {
                        if (player == null) continue;
                        ProcessCandidate(player.gameObject, ref nearestTarget, ref minDistanceSq);
                    }
                }
                break;

            case TargetSearchType.WorldObject:
                // 물리 엔진을 통한 오브젝트(Dung) 탐색
                float radius = Mathf.Sqrt(activeRadiusSq);

                // ★ ScriptableObject에 저장된 LayerMask를 그대로 사용 (비트 연산 불필요!)
                int layerMask = settings.targetLayer.value;

                int hitCount = Physics.OverlapSphereNonAlloc(transform.position, radius, _colliderBuffer, layerMask);

                for (int i = 0; i < hitCount; i++)
                {
                    ProcessCandidate(_colliderBuffer[i].gameObject, ref nearestTarget, ref minDistanceSq);
                }
                break;
        }

        // 결과 적용
        if (nearestTarget != null)
        {
            memoryDelta = 0f;
            SetTarget(nearestTarget);
        }
        else
        {
            HandleLostTarget();
        }
    }

    /// <summary>
    /// 개별 후보군(플레이어 or 똥)에 대해 거리 및 감지 여부를 검사하는 헬퍼 함수
    /// ref 키워드를 사용해 최단 거리와 타겟 변수를 직접 갱신
    /// </summary>
    private void ProcessCandidate(GameObject candidate, ref GameObject nearestTarget, ref float minDistanceSq)
    {
        if (candidate == null || !candidate.activeInHierarchy)
            return;

        Vector3 targetPos = candidate.transform.position;
        float distanceSq = (transform.position - targetPos).sqrMagnitude;

        // 1. 활성화 범위(또는 현재 찾은 최단 거리)보다 멀면 패스
        if (distanceSq > minDistanceSq)
            return;

        // 타겟 후보가 범위 내에 있으므로 AI를 활성화 상태로 유지
        isAnyPlayerNear = true;

        // 2. 시야/청각 센서 정밀 검사
        if (IsTargetInSensorRange(candidate))
        {
            // 더 가까운 타겟 발견 시 갱신
            minDistanceSq = distanceSq;
            nearestTarget = candidate;
        }
    }

    private bool IsTargetInSensorRange(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError($"[AnimalSensor] IsTargetInSensorRange - target is null");
            return false;
        }

        isOnHeard = false;

        Vector3 sensorDir = sensorDirection == Direction.Up ? Vector3.up :
                            sensorDirection == Direction.Down ? Vector3.down :
                            sensorDirection == Direction.Left ? Vector3.left :
                            sensorDirection == Direction.Right ? Vector3.right :
                            sensorDirection == Direction.Forward ? Vector3.forward :
                            Vector3.back;

        Vector3 sightPosition = transform.position + sensorDir * config.fovHeight;

        Vector3 targetPosition = target.transform.position;
        Vector3 direction = (targetPosition - sightPosition).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        isOnSight = CheckFieldOfView(distance, sightPosition, direction);
        isOnHeard = CheckSoundRange(distance);

        return isOnHeard || isOnSight;
    }

    private bool CheckFieldOfView(float distance, Vector3 origin, Vector3 direction)
    {
        if (distance > config.fovRange)
            return false;

        // 시야각 체크 (Y축 무시)
        Vector3 forwardSensor = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 targetDirection = new Vector3(direction.x, 0, direction.z).normalized;

        if (Vector3.Angle(forwardSensor, targetDirection) >= config.fovAngle * 0.5f)
            return false;

        // 장애물 체크
        if (Physics.Raycast(origin, direction, distance, config.obstacleLayer))
            return false;

        return true;
    }

    private bool CheckSoundRange(float distance)
    {
        return distance <= config.soundRange;
    }

    private void HandleLostTarget()
    {
        // 이전 타겟이 존재하고 기억 시간이 남아있으면 유지
        if (memoryDelta > 0 && Target != null && Target.activeInHierarchy)
        {
            TargetLastPosition = Target.transform.position;
        }
        else
        {
            ClearTarget();
        }
    }
    #endregion

    #region Target Management
    private void SetTarget(GameObject target)
    {
        Target = target;

        if (target != null)
            TargetLastPosition = target.transform.position;
    }

    private void ClearTarget()
    {
        Target = null;
        isOnSight = false;
        isOnHeard = false;
    }
    #endregion

    #region AI Activation
    private void ActivateAI()
    {
        if (agent == null) return;

        if (!agent.enabled)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }
    }

    private void DeactivateAI()
    {
        if (agent == null) return;

        if (agent.enabled)
            agent.enabled = false;
    }
    #endregion

    #region Debug Visualization
    private void OnDrawGizmosSelected()
    {
        if (!enableDebug || config == null)
            return;

        // 소리 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, config.soundRange);

        // 시야 감지 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, config.fovRange);

        // 시야각 표시
        float halfAngle = config.fovAngle * 0.5f;
        Vector3 leftBoundary = Quaternion.AngleAxis(halfAngle, transform.up) * transform.forward * config.fovRange;
        Vector3 rightBoundary = Quaternion.AngleAxis(-halfAngle, transform.up) * transform.forward * config.fovRange;

        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);

        // 타겟 연결선
        if (Target != null)
        {
            Gizmos.color = Color.green;
            Vector3 sightPosition = transform.position + Vector3.up * config.fovHeight;
            Gizmos.DrawLine(sightPosition, Target.transform.position);
        }
    }
    #endregion
}

#region Supporting Types
public enum Direction
{
    Up,
    Down,
    Left,
    Right,
    Forward,
    Back
}
#endregion

public interface IPlayerControllable
{
    string Id { get; }
}