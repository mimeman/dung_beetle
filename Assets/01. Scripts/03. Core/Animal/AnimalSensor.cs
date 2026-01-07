using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dung.Data;
using UnityEngine;

/// <summary>
/// Target = target GameObject
/// TargetPrevPosition = target의 마지막 위치값.
/// IsOnSight = 플레이어를 확실하게 포착했을때의 상태
/// IsOnHeard = 플레이어를 보지는 못했는데 소리감지가 되었을때.
/// ForceDetection(GameObject) : 강제target 설정
/// </summary>
public class AnimalSensor : MonoBehaviour
{
    #region Properties
    [Header("감지 설정")]
    [SerializeField] private AnimalConfig animalConfig;
    [Tooltip("감지할 대상의 Layer")]
    [SerializeField] private LayerMask targetMask;
    [Tooltip("감지할 대상 제외 Obstacle들의 Layer")]
    [SerializeField] private LayerMask obstuctionMask;
    [Tooltip("공격 당했을시 찾아보려는 최대 거리를 구하기 위한 multiplier")]
    [SerializeField] private float rangeMultiplier = 1.5f;
    [Tooltip("대상을 기억하는 유지 시간")]
    [SerializeField] private float memoryDuration = 3.0f;
    [Tooltip("감지 Delay 시간")]
    [SerializeField] private float sensorDelay = 0.2f;

    private GameObject target;
    public GameObject Target { get { return target; } }
    private Vector3 targetLastPosition = Vector3.zero;
    public Vector3 TargeLastPosition { get { return targetLastPosition; } }
    private bool isOnSight = false;
    public bool IsOnSight { get { return isOnSight; } }
    private bool isOnHeard = false;
    public bool IsOnHeard { get { return isOnHeard; } }

    private float memoryDelta = 0f;
    private WaitForSeconds checkDelay;
    private SessionManager sessionManager;
    bool isHost;
    #endregion

    void Start()
    {
        // isHost = NetworkManager.Instance != null && NetworkManager.Instance.IsHost;
        checkDelay = new WaitForSeconds(sensorDelay);

        // // Network 설계
        // networkType = NetworkManager.Instance.Mode;
        // if (networkType == NetworkType.None)
        //     sessionManager = GameManager.Instance.SingleSession;
        // else
        //     sessionManager = NetworkManager.Instance.SessionManager;

        if (isHost)
        {   // 호스트와 싱글에서만 실행
            StartCoroutine(CheckSensorRoutine());
            return;
        }

        // Client일때 비활성화.
        this.enabled = false;
    }

    void Update()
    {
        if (isHost)
        {   // 호스트와 싱글만 기억
            if (memoryDelta > 0)
                memoryDelta -= Time.deltaTime;
        }
    }

    // 강제로 target을 설정해주는 함수
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
            yield return checkDelay;
        }
    }

    // 감지할 target을 설정해주는 Logic
    void FindTarget()
    {
        if (null == sessionManager)
            return;

        var players = sessionManager.players.Values;
        if (null == players || 0 >= players.Count())
        {
            ClearTarget();
            return;
        }

        GameObject rawTarget = null;
        float minDistance = float.MaxValue;
        foreach (var player in players)
        {
            if (null == player.gameObject)
                continue;   // dictionary 방어코드

            Vector3 playerPosition = player.gameObject.transform.position;
            float rawDistance = (transform.position - playerPosition).sqrMagnitude; // 성능을 위한 빠른 거리 계산

            if (rawDistance > minDistance)
                continue;   // 더 가까운 거리가 존재하면 pass

            if (IsTargetOnSensor(player.gameObject))
            {
                minDistance = rawDistance;
                rawTarget = player.gameObject;
            }
        }

        if (null != rawTarget)
        {   // 시야에 target 감지.
            memoryDelta = 0f;
            SetTarget(rawTarget);
        }
        else
        {   // 시야에 들어온 target 없음.
            if (memoryDelta > 0 && target != null && target.activeInHierarchy)
            {   // 이전 타겟이 존재하고 있고 기억시간이 충분하다면 해당 타겟 유지
                targetLastPosition = target.transform.position;
            }
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

        if (distance <= animalConfig.soundRange)
        {   // 소리 감지
            // TODO: Player의 소리가 안나는 상태의 움직임이 있다면 예외처리 해줘야합니다.
            isOnHeard = true;

            // 장애물이 먼저 Raycast되었을때.
            if (!Physics.Raycast(sightPos, direction, distance, obstuctionMask))
                return true;
        }

        if (distance <= animalConfig.fovRange)
        {   // 시야각 감지
            Vector3 forwardSensor = transform.forward;
            forwardSensor.y = 0;    // y축은 무시
            Vector3 targetDirection = direction;
            targetDirection.y = 0;

            if (Vector3.Angle(forwardSensor, targetDirection) < animalConfig.fovAngle / 2)
            {
                if (!Physics.Raycast(sightPos, direction, distance, obstuctionMask))
                {
                    isOnSight = true;
                    return true;
                }
            }
        }

        isOnSight = false;
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
}

// Temp Enum. 추후 네트워크 작업하면서 처리할 예정.
public enum NetworkType
{
    None,
    Client,
    Host,
    Server
}
public class SessionManager
{
    public IReadOnlyDictionary<byte, GameObject> players = new Dictionary<byte, GameObject>();
}
interface IPlayerControllable { }
