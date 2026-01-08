using UnityEngine;
using UnityEngine.AI;

namespace AIStates
{
    public class Idle : AnimalBaseState<AIController>
    {
        private float idleTime;
        private float timer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"Idle State Entered");
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
            Debug.Log($"Patrol State Entered");
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
            Debug.Log($"Trace State Entered");
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

    public class Attack : AnimalBaseState<AIController>
    {
        private float timer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"Attack State Entered");
            animal.StopMoving();
            animal.SetAnimTrigger(animal.hashAttack1);

            LookAt(animal);

            timer = 0;
        }

        public override void ExitState(AIController animal) { animal.StopAllCoroutines(); }

        public override AnimalBaseState<AIController> UpdateState(AIController animal)
        {
            animal.StopMoving();

            LookAt(animal);

            timer += Time.deltaTime;
            if (timer >= animal.Config.attackTimeout)
                return animal.stateMachine.TraceState;

            return this;
        }

        private void LookAt(AIController animal)
        {
            if (animal.target)
                animal.LookAt(animal.target.transform.position);
        }
    }

    public class Interact : AnimalBaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            Debug.Log($"Interact State Entered");
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
            Debug.Log($"Hit");
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
            Debug.Log($"Dead");
            animal.StopMoving();
            animal.StopAllCoroutines();

            // TODO : 추후 AIController에 Die코드 만들어서 실행해줘도 될것 같음.
            if (animal.TryGetComponent<Collider>(out var collider))
                collider.enabled = false;

            if (animal.TryGetComponent<NavMeshAgent>(out var agent))
                agent.enabled = false;

            // Sample Loot Logic
            // AnimalHealth health = animal.GetComponent<AnimalHealth>();
            // if (!health)// && !animal.Config.lootTable
            //     health.SpawnLoot();// animal.Config.lootTable);

            // TODO : Network Logic here
            // if (!AnimalManager.Instance)
            //     AnimalManager.Instance.RegisterAnimalDied();
        }

        public override void ExitState(AIController animal) { }

        public override AnimalBaseState<AIController> UpdateState(AIController animal)
        {
            return this;
        }
    }

}