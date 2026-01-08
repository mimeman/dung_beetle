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

    private NavMeshAgent agent;

    private float targetSpeed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
    }

    void Update()
    {
        float currentSpeed = agent.speed;
        float rate = (currentSpeed < targetSpeed) ? accelerationRate : decelerationRate;

        agent.speed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * rate);
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void Move(Vector3 destination, float speed)
    {
        targetSpeed = speed;
        agent.SetDestination(destination);
    }

    public void TurnTowards(Vector3 worldTargetPosition, float turnSpeed)
    {
        agent.SetDestination(transform.position);
        transform.LookAt(new Vector3(worldTargetPosition.x, transform.position.y, worldTargetPosition.z));
    }

    public void Stop()
    {
        if (agent.hasPath)
            agent.ResetPath();

        targetSpeed = 0f;
    }
}