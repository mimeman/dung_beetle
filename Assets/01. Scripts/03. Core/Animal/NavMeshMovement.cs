using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;

    [Header("Accel & Decel Settings")]
    [SerializeField] private float accelerationRate = 5f;
    [SerializeField] private float decelerationRate = 10f;

    [Header("LOD Settings")]
    [SerializeField] private LayerMask groundLayer;

    private Vector3 destination;
    private NavMeshAgent agent;
    private float targetSpeed;
    private float stoppingDistance;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        stoppingDistance = agent.stoppingDistance;
    }

    void Update()
    {
        float currentSpeed = agent.speed;
        float rate = (currentSpeed < targetSpeed) ? accelerationRate : decelerationRate;

        if (agent.enabled)
        {
            agent.speed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * rate);
            animator.SetFloat("speed", agent.velocity.magnitude);
        }
        else
            SimulateLODMovement();
    }

    private void SimulateLODMovement()
    {
        Vector3 direction = (destination - transform.position).normalized;
        float distance = (transform.position - destination).sqrMagnitude;

        // 목적지에 거의 도착했으면 이동 중지
        if (distance < stoppingDistance) return;

        // 이동을 위한 가벼운 Vector 연산
        transform.position += direction * targetSpeed * Time.deltaTime;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        if (Time.frameCount % 30 == 0)
        {   // 최적화를 위해 프레임이 아닌 30프레임마다 체크 땅에서 움직이도록 보정
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 2f, groundLayer))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
        }
    }

    public void Move(Vector3 destination, float speed)
    {
        targetSpeed = speed;
        if (agent.enabled)
            agent.SetDestination(destination);
        else
        {   // 수동 이동
            Debug.Log($"수동 Move");
            this.destination = destination;
        }
    }

    public void TurnTowards(Vector3 worldTargetPosition, float turnSpeed)
    {
        if (agent.enabled)
            agent.SetDestination(transform.position);

        transform.LookAt(new Vector3(worldTargetPosition.x, transform.position.y, worldTargetPosition.z));
    }

    public void Stop()
    {
        if (agent.enabled && agent.hasPath)
            agent.ResetPath();

        targetSpeed = 0f;
    }
}