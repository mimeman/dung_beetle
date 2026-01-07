using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace AIBaseStates
{
    public class Idle : AnimalBaseState<AIController>
    {
        private float idleTime;
        private float timer;

        public override void EnterState(AIController animal)
        {
            animal.StopMoving();
            animal.SetAnimBool(animal.hashIsWalking, false);
            animal.SetAnimBool(animal.hashIsRunning, false);
            idleTime = Random.Range(animal.Config.idleMinTime, animal.Config.idleMaxTime);
            timer = 0f;
        }

        public override void ExitState(AIController animal) { }

        public override AnimalBaseState<AIController> UpdateState(AIController animal)
        {
            if (animal.sensor.IsOnSight)
            {   // Target 발견시 Trace상태로 변경
                return animal.stateMachine.TraceState;
            }
            timer += Time.deltaTime;
            if (timer >= idleTime)
            {
                return animal.stateMachine.PatrolState;
            }
            return this;
        }
    }

    public class Patrol : AnimalBaseState<AIController>
    {
        // private PatrolType patrolType;   // 어떤것을 탐색할것인가?
        private Vector3 patrolDestination;
        private float entryTimer;

        public override void EnterState(AIController animal)
        {
            patrolDestination = animal.GetPatrolDestination();
            animal.SetAnimFloat(animal.hashMoveSpeed, 1f);
            entryTimer = 0f;
        }

        public override void ExitState(AIController animal)
        {
            animal.StopMoving();
            animal.SetAnimFloat(animal.hashMoveSpeed, 0f);
        }

        public override AnimalBaseState<AIController> UpdateState(AIController animal)
        {
            if (!animal.Config.friendly && (animal.sensor.IsOnSight || animal.sensor.IsOnHeard))
            {   // 우호적이지 않고 플레이어를 발견했거나 소리를 들었다면
                return animal.stateMachine.TraceState;
            }

            animal.MoveTo(patrolDestination);
            entryTimer += Time.deltaTime;
            if (entryTimer > 0.1f && animal.arrivedAtDestination)
            {
                return animal.stateMachine.IdleState;
            }

            return this;
        }
    }

    public class Trace : AnimalBaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            animal.SetAnimFloat(animal.hashMoveSpeed, 2f);
        }

        public override void ExitState(AIController animal)
        {
            if (animal.CurrentState != animal.stateMachine.AttackState)
            // || animal.CurrentState != animal.stateMachine.BlockState)
            {
                animal.SetAnimFloat(animal.hashMoveSpeed, 0f);
            }
        }

        public override AnimalBaseState<AIController> UpdateState(AIController animal)
        {
            if (animal.GetDistanceToTarget() <= animal.Config.attackRange || animal.Config.friendly && animal.attacked)
            {   // 우호적이나 공격당했을때.
                return animal.stateMachine.AttackState;
            }

            if (animal.target)
                animal.MoveTo(animal.target.transform.position);

            // 목적지에 도달했으나, target의 센서에서 감지가 되지 않을때.
            if (!animal.sensor.IsOnSight && animal.arrivedAtDestination)
                return animal.stateMachine.PatrolState;
            return this;
        }
    }

    public class Interact : AnimalBaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            throw new System.NotImplementedException();
        }

        public override void ExitState(AIController animal) { }

        public override AnimalBaseState<AIController> UpdateState(AIController animal)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Attack : AnimalBaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            throw new System.NotImplementedException();
        }

        public override void ExitState(AIController animal) { }

        public override AnimalBaseState<AIController> UpdateState(AIController animal)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Hit : AnimalBaseState<AIController>
    {
        private float hitStunDuration = 0.5f;
        private float timer;

        public override void EnterState(AIController animal)
        {
            timer = 0f;
            if (animal.TryGetComponent<NavMeshAgent>(out var agent))
                agent.speed = animal.Config.runSpeed * 0.3f;
        }

        public override void ExitState(AIController animal) { }

        public override AnimalBaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;
            if (timer > -hitStunDuration)
                return animal.stateMachine.TraceState;
            return this;
        }
    }

    public class Die : AnimalBaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            throw new System.NotImplementedException();
        }

        public override void ExitState(AIController animal) { }

        public override AnimalBaseState<AIController> UpdateState(AIController animal)
        {
            throw new System.NotImplementedException();
        }
    }

}