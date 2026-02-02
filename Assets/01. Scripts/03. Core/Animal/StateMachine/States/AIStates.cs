using UnityEngine;
using UnityEngine.AI;

namespace AIStates
{
    /// <summary>
    /// Idle: 기본 대기 상태
    /// - 일정 시간 대기 후 Patrol 상태로 전환
    /// - 적대적 동물의 경우 플레이어 감지 시 Trace 상태로 전환
    /// </summary>
    public class Idle : BaseState<AIController>
    {
        private float idleTime;
        private float timer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} entered Idle state");

            animal.StopMoving();
            animal.SetAnimBool(animal.HashIsWalking, false);
            animal.SetAnimBool(animal.HashIsRunning, false);

            idleTime = Random.Range(animal.Config.idleMinTime, animal.Config.idleMaxTime);
            timer = 0f;
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;

            // 적대적이고 플레이어를 발견했다면 즉시 추적
            if (!animal.Config.friendly && animal.Sensor.IsOnSight)
                return animal.StateMachine.TraceState;

            // Idle 시간이 끝나면 순찰 시작
            if (timer >= idleTime)
                return animal.StateMachine.PatrolState;

            return this;
        }
    }

    /// <summary>
    /// Patrol: 탐색 상태
    /// - 랜덤한 위치로 이동하며 탐색
    /// - 플레이어 발견 시 Trace 상태로 전환
    /// </summary>
    public class Patrol : BaseState<AIController>
    {
        private Vector3 patrolDestination;
        private float entryTimer;
        private const float DESTINATION_CHECK_DELAY = 0.1f;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} entered Patrol state");

            // 소리를 감지했다면 해당 위치로, 아니면 랜덤 위치로 순찰
            if (!animal.Config.friendly && animal.Sensor.IsOnHeard)
                patrolDestination = animal.Sensor.TargetLastPosition;
            else
                patrolDestination = animal.GetPatrolDestination();

            animal.SetAnimFloat(animal.HashMoveSpeed, 1f);
            entryTimer = 0f;
        }

        public override void ExitState(AIController animal)
        {
            animal.StopMoving();
            animal.SetAnimFloat(animal.HashMoveSpeed, 0f);
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // 플레이어를 시야에서 발견하면 추적
            if (!animal.Config.friendly && animal.Sensor.IsOnSight)
                return animal.StateMachine.TraceState;

            // 소리만 들었다면 해당 위치로 순찰
            if (!animal.Config.friendly && animal.Sensor.IsOnHeard)
                return animal.StateMachine.PatrolState;

            animal.MoveTo(patrolDestination);
            entryTimer += Time.deltaTime;

            // 목적지 도착 체크 (약간의 딜레이 후)
            if (entryTimer > DESTINATION_CHECK_DELAY && animal.ArrivedAtDestination)
                return animal.StateMachine.IdleState;

            return this;
        }
    }

    /// <summary>
    /// Trace: 추적 상태
    /// - 타겟을 향해 달려감
    /// - 공격 범위 내에 들어오면 Attack 상태로 전환
    /// </summary>
    public class Trace : BaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} entered Trace state");
            animal.SetAnimFloat(animal.HashMoveSpeed, 2f);
        }

        public override void ExitState(AIController animal)
        {
            // Attack 상태로 전환하는 경우가 아니라면 애니메이션 초기화
            if (animal.CurrentState != animal.StateMachine.AttackState)
                animal.SetAnimFloat(animal.HashMoveSpeed, 0f);
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // 타겟이 있으면 추적
            if (animal.Target)
                animal.MoveTo(animal.Target.transform.position);

            // 타겟을 놓치고 목적지에 도달했으면 Idle로 전환
            if (!animal.Sensor.IsOnSight && animal.ArrivedAtDestination)
                return animal.StateMachine.IdleState;

            // 공격 범위 내에 들어왔는지 체크
            if (animal.GetDistanceToTarget() <= animal.Config.attackRange)
            {
                if (animal.Config.friendly)
                    return animal.StateMachine.IdleState;
                else
                    return animal.StateMachine.AttackState;
            }

            return this;
        }
    }

    /// <summary>
    /// Attack: 공격 상태
    /// - 타겟을 바라보며 공격
    /// - 공격 타임아웃 후 Trace 상태로 복귀
    /// </summary>
    public class Attack : BaseState<AIController>
    {
        private float timer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} entered Attack state");

            animal.StopMoving();
            animal.SetAnimTrigger(animal.HashAttack1);
            LookAtTarget(animal);

            timer = 0f;
        }

        public override void ExitState(AIController animal)
        {
            animal.StopAllCoroutines();
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            animal.StopMoving();
            LookAtTarget(animal);

            timer += Time.deltaTime;

            // 공격 타임아웃이 지나면 다시 추적
            if (timer >= animal.Config.attackTimeout)
                return animal.StateMachine.TraceState;

            return this;
        }

        private void LookAtTarget(AIController animal)
        {
            if (animal.Target)
                animal.LookAt(animal.Target.transform.position);
        }
    }

    /// <summary>
    /// Interact: 상호작용 상태 (미구현)
    /// </summary>
    public class Interact : BaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            Debug.LogWarning($"{animal.name} Interact state is not implemented");
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // TODO: 상호작용 로직 구현
            return animal.StateMachine.IdleState;
        }
    }

    /// <summary>
    /// Hit: 피격 상태
    /// - 짧은 경직 후 Trace 상태로 복귀
    /// </summary>
    public class Hit : BaseState<AIController>
    {
        private const float HIT_STUN_DURATION = 0.5f;
        private const float SPEED_REDUCTION_FACTOR = 0.3f;

        private float timer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} got hit");

            timer = 0f;

            // NavMeshAgent 속도 감소
            if (animal.TryGetComponent<NavMeshAgent>(out var agent))
                agent.speed = animal.Config.runSpeed * SPEED_REDUCTION_FACTOR;
        }

        public override void ExitState(AIController animal)
        {
            // 속도 복구
            if (animal.TryGetComponent<NavMeshAgent>(out var agent))
                agent.speed = animal.Config.runSpeed;
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;

            // 경직 시간이 지나면 추적 재개
            if (timer >= HIT_STUN_DURATION)
                return animal.StateMachine.TraceState;

            return this;
        }
    }

    /// <summary>
    /// Die: 사망 상태
    /// - 컴포넌트 비활성화 및 정리 작업 수행
    /// </summary>
    public class Die : BaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} died");

            animal.StopMoving();
            animal.StopAllCoroutines();

            // 물리 충돌 비활성화
            if (animal.TryGetComponent<Collider>(out var collider))
                collider.enabled = false;

            // NavMeshAgent 비활성화
            if (animal.TryGetComponent<NavMeshAgent>(out var agent))
                agent.enabled = false;

            // 전리품 드롭 (옵션)
            if (animal.TryGetComponent<AnimalHealth>(out var health))
                health.SpawnLoot();
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // 사망 상태는 종료 상태
            return this;
        }
    }
}