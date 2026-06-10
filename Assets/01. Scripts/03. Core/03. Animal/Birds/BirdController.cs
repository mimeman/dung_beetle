using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : AIController
{
    public new BirdStateMachine StateMachine;
    public Dung.Data.CrowFlightConfig FlightConfig;
    new Rigidbody rigidbody;

    public float NoiseSeed { get; private set; }
    public Vector3 CurrentVelocity { get; set; }

    new void Awake()
    {
        base.Awake();
        StateMachine = GetComponent<BirdStateMachine>();
    }

    new void Start()
    {
        Initialize();
        ChangeState(StateMachine.IdleState);
        rigidbody = GetComponent<Rigidbody>();
        NoiseSeed = Random.Range(0f, 1000f);
    }

    new void Update()
    {
        if (!isHost) return;
        if (CurrentState == null || health.IsDead)
            return;

        // Target update
        if (Sensor.Target)
            Target = Sensor.Target.transform;
        else
            Target = null;

        // 다음 State로 넘어가기 위한 state의 updateState 로직
        BaseState<AIController> nextState = CurrentState.UpdateState(this);
        if (nextState != CurrentState)
            ChangeState(nextState);
    }

    public void MoveForward(float speed)
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void RotateTowards(Vector3 targetPos, float turnSpeed)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction == Vector3.zero) return;

        Quaternion lookRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * Time.deltaTime);
    }

    #region Boids & Flocking
    public Vector3 CalculateFlockingVelocity()
    {
        if (FlightConfig == null) return Vector3.zero;

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int flockCount = 0;

        Collider[] nearby = Physics.OverlapSphere(transform.position, FlightConfig.flockRange);
        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (col.TryGetComponent<BirdController>(out BirdController other))
            {
                // Separation
                Vector3 diff = transform.position - other.transform.position;
                separation += diff.normalized / diff.magnitude;

                // Alignment
                alignment += other.transform.forward;

                // Cohesion
                cohesion += other.transform.position;

                flockCount++;
            }
        }

        if (flockCount == 0) return Vector3.zero;

        separation /= flockCount;
        alignment = (alignment / flockCount).normalized;
        cohesion = (cohesion / flockCount - transform.position).normalized;

        return (separation * FlightConfig.separationWeight + 
                alignment * FlightConfig.alignmentWeight + 
                cohesion * FlightConfig.cohesionWeight);
    }
    #endregion
}
