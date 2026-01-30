using UnityEngine;
using UnityEngine.AI;

namespace AIStates
{
    /// <summary>
    /// Idle : 가만히 서있는 기본 상태.
    /// [ Enter ] Random 시간 Idle 상태 애니메이션 재생
    /// [ Update ] Sensor.IsOnSight -> Trace
    ///            time end -> Patrol
    ///            + AnimalStates에서 override시켜서 eat, poo, sleep을 해주도록 하면 될듯?
    /// </summary>
    public class Idle : BaseState<AIController>
    {
        private float idleTime;
        private float timer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Idle State Entered");
            animal.StopMoving();
            animal.SetAnimBool(animal.hashIsWalking, false);
            animal.SetAnimBool(animal.hashIsRunning, false);
            idleTime = Random.Range(animal.Config.idleMinTime, animal.Config.idleMaxTime);
            timer = 0f;
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            if (!animal.Config.friendly && animal.sensor.IsOnSight)
            {   // Target 발견시 Trace상태로 변경
                if ((BirdController)animal)
                    return ((BirdStateMachine)animal.stateMachine).StalkingState;
                return animal.stateMachine.TraceState;
            }
            timer += Time.deltaTime;
            if (timer >= idleTime)
            {
                if ((BirdController)animal)
                    return ((BirdStateMachine)animal.stateMachine).PatrolState;
                return animal.stateMachine.PatrolState;
            }
            return this;
        }
    }

    /// <summary>
    /// Patrol : 탐색할 요소를 찾아서 이동하는 상태.
    /// [ Enter ] RandomPatrolDestination 설정
    ///           Anim -> Walk
    ///           + PatrolType같이 어떤것을 탐색할지 해주는 방법도 낫베드
    /// [ Update ] !friendly + Sensor.isOnSight -> Trace
    ///            !friendly + Sensor.isOnHeard -> Find?
    /// [ Exit ] -> movement.Stop
    /// </summary>
    public class Patrol : BaseState<AIController>
    {
        // private PatrolType patrolType;   // 어떤것을 탐색할것인가?
        private Vector3 patrolDestination;
        private float entryTimer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Patrol State Entered");
            if (!animal.Config.friendly && animal.sensor.IsOnHeard)   // 무언가 들은게 있다면
                patrolDestination = animal.sensor.TargeLastPosition;
            else    // 아무것도 없다면 랜덤 좌표로 탐색
                patrolDestination = animal.GetPatrolDestination();

            animal.SetAnimFloat(animal.hashMoveSpeed, 1f);
            entryTimer = 0f;
        }

        public override void ExitState(AIController animal)
        {
            animal.StopMoving();
            animal.SetAnimFloat(animal.hashMoveSpeed, 0f);
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // 우호적이지 않고 플레이어를 발견했거나 소리를 들었다면
            if (!animal.Config.friendly && animal.sensor.IsOnSight)
                return animal.stateMachine.TraceState;  // 시야에 들어왔다면 바로 쫓아가서 싸움
            else if (!animal.Config.friendly && animal.sensor.IsOnHeard)
                return animal.stateMachine.PatrolState; // 소리만 들었다면 해당 좌표로 순찰

            animal.MoveTo(patrolDestination);
            entryTimer += Time.deltaTime;
            if (entryTimer > 0.1f && animal.arrivedAtDestination)
            {
                return animal.stateMachine.IdleState;
            }

            return this;
        }
    }

    /// <summary>
    /// Trace : Entity가 있다면 해당 적을 쫓아가는 상태.
    /// [ Enter ] Anim -> Run
    /// [ Update ] !friendly or attacked and in attackrange (적대적이거나 공격당했을때(화났을때)) -> AttackState
    ///            target -> Move(target)
    ///            ArrivedToTarget && !Sensor.isOnSight (목적지 도착 & Sensor에 시야에 잡히는게 없을때)
    /// </summary>
    public class Trace : BaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Trace State Entered");
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

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            if (animal.target)
                animal.MoveTo(animal.target.transform.position);

            // 목적지에 도달했으나, target의 센서에서 감지가 되지 않을때.
            if (!animal.sensor.IsOnSight && animal.arrivedAtDestination)
                return animal.stateMachine.IdleState;

            if (animal.GetDistanceToTarget() <= animal.Config.attackRange)
            {
                if (animal.Config.friendly)
                    return animal.stateMachine.IdleState;
                else
                    return animal.stateMachine.AttackState;
            }

            return this;
        }
    }

    /// <summary>
    /// Attack : 목표 Entity에게 공격을 가하는 상태
    /// [ Enter ] Anim -> Attack
    ///           StopMoving, LookAt(target)
    /// [ Update ] StopMoving, LookAt(target)
    /// </summary>
    public class Attack : BaseState<AIController>
    {
        private float timer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Attack State Entered");
            animal.StopMoving();
            animal.SetAnimTrigger(animal.hashAttack1);

            LookAt(animal);

            timer = 0;
        }

        public override void ExitState(AIController animal) { animal.StopAllCoroutines(); }

        public override BaseState<AIController> UpdateState(AIController animal)
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

    public class Interact : BaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Interact State Entered");
            throw new System.NotImplementedException();
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Hit : BaseState<AIController>
    {
        private float hitStunDuration = 0.5f;
        private float timer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Hit");
            timer = 0f;
            if (animal.TryGetComponent<NavMeshAgent>(out var agent))
                agent.speed = animal.Config.runSpeed * 0.3f;
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;
            if (timer > -hitStunDuration)
                return animal.stateMachine.TraceState;
            return this;
        }
    }

    public class Die : BaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Dead");
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

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            return this;
        }
    }

}