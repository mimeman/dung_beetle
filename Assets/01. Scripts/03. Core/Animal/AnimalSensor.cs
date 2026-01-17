using System.Collections;
using System.Linq;
using Dung.Data;
using UnityEngine;
using UnityEngine.AI;

// TODO : 추후 따로 구현해야함
public interface IPlayerControllable
{
    string Id { get; }
}

/// <summary>
/// 
/// IsOnSight = 플레이어를 확실하게 포착했을때의 상태
/// IsOnHeard = 플레이어를 보지는 못했는데 소리감지가 되었을때.
/// ForceDetection(GameObject) : 강제target 설정
/// </summary>
/// 
public class AnimalSensor : MonoBehaviour
{
    #region Properties
    [Header("Sensor Settings")]
    [Tooltip("감지할 대상의 Layer")]
    [SerializeField] private LayerMask targetMask;
    [Tooltip("감지할 대상 제외 Obstacle들의 Layer")]
    [SerializeField] private LayerMask obstuctionMask;
    [Tooltip("공격 당했을시 찾아보려는 최대 거리를 구하기 위한 multiplier")]
    [SerializeField] private float rangeMultiplier = 1.5f;
    [Tooltip("대상을 기억하는 유지 시간")]
    [SerializeField] private float memoryDuration = 3.0f;
    [Tooltip("Sensor 감지 주기")]
    [SerializeField] private float sensorInterval = 0.2f;

    [Header("LOD Settings")]
    [Tooltip("NavMeshAgent 활성화 범위(제곱값)")]
    [SerializeField] private float activeRadiusSq = 3000f;
    [Tooltip("Active 감지 주기")]
    [SerializeField] private float activeInterval = 0.5f;

    private GameObject target;
    public GameObject Target { get { return target; } }
    private Vector3 targetLastPosition = Vector3.zero;
    public Vector3 TargeLastPosition { get { return targetLastPosition; } }
    // private bool isOnSight = false;
    public bool IsOnSight { get { return isOnSight; } }
    // private bool isOnHeard = false;
    public bool IsOnHeard { get { return isOnHeard; } }
    // private bool isAnyPlayerNear;
    public bool IsAnyPlayerNear { get { return isAnyPlayerNear; } }

    private SessionManager sessionManager;
    private AnimalConfig animalConfig;
    private NavMeshAgent agent;
    private WaitForSeconds sensorDelay;
    private WaitForSeconds activeDelay;
    private float memoryDelta = 0f;
    private bool isHost;
    #endregion

    void Start()
    {
        animalConfig = GetComponent<AIController>().Config;
        agent = GetComponent<NavMeshAgent>();
        sensorDelay = new WaitForSeconds(sensorInterval);
        activeDelay = new WaitForSeconds(activeInterval);

        // Network 설계
        sessionManager = NetworkManager.Instance.SessionManager;
        isHost = NetworkManager.Instance != null && NetworkManager.Instance.IsHost;

        if (isHost)
        {   // 호스트와 싱글에서만 실행
            StartCoroutine(CheckSensorRoutine());
            return;
        }
    }

    void Update()
    {
        if (isHost)
        {   // 호스트와 싱글만 기억
            if (memoryDelta > 0)
                memoryDelta -= Time.deltaTime;
        }
    }

    // 강제로 target을 설정해주는 함수 statemachine에서 hit에서 호출해주자. (if (!sensor.isonsight)일때)
    public void ForceDetection(GameObject attacker)
    {
        if (!isHost)
        {   // 방어 코드
            if (attacker == null)
                Debug.LogError($"[AnimalSensor] ForceDetection의 attacker인수가 null입니다.");
            return;
        }

        // attacker의 거리가 감지할수도 없이 너무 멀면 포기.
        float distance = (attacker.transform.position - transform.position).magnitude;
        if (distance >= animalConfig.fovRange * rangeMultiplier && attacker.TryGetComponent<IPlayerControllable>(out _))
        {
            SetTarget(attacker);
            isOnSight = true;
            memoryDelta = memoryDuration;
        }
    }

    IEnumerator CheckSensorRoutine()
    {
        while (true)
        {
            // TODO: 몬스터의 Health 체크 필요 healthmanager.isdead ?-> ClearTarget();yield break;
            FindTarget();
            if (isAnyPlayerNear)
            {
                ActivateAI();
                yield return sensorDelay;
            }
            else
            {
                DeactivateAI();
                yield return activeDelay;
            }
        }
    }

    // 감지할 target을 설정해주는 Logic
    void FindTarget()
    {
        if (null == sessionManager)
            return;

        var players = sessionManager.Players.Values;
        if (null == players || 0 >= players.Count())
        {
            ClearTarget();
            return;
        }

        GameObject rawTarget = null;
        float minDistance = activeRadiusSq;
        isAnyPlayerNear = false;
        // 근처 플레이어 탐지.
        foreach (var player in players)
        {
            if (null == player.gameObject)
                continue;   // dictionary 방어코드

            Vector3 playerPosition = player.gameObject.transform.position;
            float rawDistance = (transform.position - playerPosition).sqrMagnitude; // 성능을 위한 빠른 거리 계산

            // Debug.Log($"raw:{rawDistance} vs min:{minDistance}");
            if (rawDistance > minDistance)
                continue;
            else
                isAnyPlayerNear = true;

            if (IsTargetOnSensor(player.gameObject))
            {
                minDistance = rawDistance;
                rawTarget = player.gameObject;
            }
        }

        if (null != rawTarget)
        {   // Sensor에 target 감지.
            memoryDelta = 0f;
            SetTarget(rawTarget);
        }
        else
        {   // Sensor에 들어온 target 없음
            if (memoryDelta > 0 && target != null && target.activeInHierarchy)  // 이전 타겟이 존재하고 있고 기억시간이 충분하다면 해당 타겟 유지
                targetLastPosition = target.transform.position;
            else
                ClearTarget();
        }
    }
    // target 감지 Logic
    bool IsTargetOnSensor(GameObject target)
    {
        if (target == null)
        {   // 방어코드
            Debug.LogError($"[AnimalSensor] Target Check의 인수인 target이 null입니다.");
            return false;
        }

        isOnHeard = false;

        Vector3 sightPos = transform.position + Vector3.up * animalConfig.fovHeight;
        Vector3 targetPos = target.transform.position;
        Vector3 direction = (targetPos - sightPos).normalized;
        float distance = Vector3.Distance(transform.position, targetPos);

        isOnSight = CheckFov(distance, sightPos, direction);
        isOnHeard = CheckSound(distance);

        return isOnHeard || isOnSight;
    }

    bool CheckFov(float distance, Vector3 original, Vector3 direction)
    {   // 시야 감지 로직
        if (distance <= animalConfig.fovRange)
        {   // 시야각 감지
            Vector3 forwardSensor = transform.forward;
            forwardSensor.y = 0;    // y축은 무시
            Vector3 targetDirection = direction;
            targetDirection.y = 0;

            if (Vector3.Angle(forwardSensor, targetDirection) < animalConfig.fovAngle / 2)
            {
                if (!Physics.Raycast(original, direction, distance, obstuctionMask))
                {
                    isOnSight = true;
                    return true;
                }
            }
        }
        return false;
    }

    bool CheckSound(float distance)
    {   // 사운드 감지 로직
        if (distance <= animalConfig.soundRange)
        {   // 소리 감지
            // TODO: Player의 소리가 안나는 상태의 움직임이 있다면 예외처리 해줘야합니다.

            isOnHeard = true;
            return true;
        }
        return false;
    }

    // target 설정
    void SetTarget(GameObject target)
    {
        this.target = target;

        if (null != target)
            targetLastPosition = target.transform.position;
    }
    // target 초기화
    void ClearTarget()
    {
        target = null;
        isOnSight = false;
        isOnHeard = false;
    }

    void ActivateAI()
    {
        if (!agent.enabled)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }
    }

    void DeactivateAI()
    {
        if (agent.enabled)
            agent.enabled = false;
    }

    #region Debug
    [Header("Debug")]
    public bool debug;
    // [SerializeField] private GameObject target;
    // [SerializeField] private Vector3 targetLastPosition = Vector3.zero;
    [SerializeField] private bool isOnSight = false;
    [SerializeField] private bool isOnHeard = false;
    [SerializeField] private bool isAnyPlayerNear;

    private void OnDrawGizmosSelected()
    {
        // 방어코드
        if (!debug || !animalConfig)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, animalConfig.soundRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, animalConfig.fovRange);

        Vector3 fovLine1 = Quaternion.AngleAxis(animalConfig.fovAngle / 2, transform.up) * transform.forward * animalConfig.fovRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-animalConfig.fovAngle / 2, transform.up) * transform.forward * animalConfig.fovRange;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);

        if (target)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up * animalConfig.fovHeight, target.transform.position);
        }
    }
    #endregion
}
