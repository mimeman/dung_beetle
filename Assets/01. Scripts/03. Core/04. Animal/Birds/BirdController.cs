using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : AIController
{
    public new BirdStateMachine StateMachine;
    Rigidbody rigidbody;

    void Start()
    {
        base.Start();
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
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
}
