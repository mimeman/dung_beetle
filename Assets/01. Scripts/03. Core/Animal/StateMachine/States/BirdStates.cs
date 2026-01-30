using UnityEngine;

namespace BirdStates
{
    /// <summary>
    /// 공중에서 가만히 날개짓 하며 있는 상태.
    /// </summary>
    public class FlyIdle : BaseState<AIController>
    {
        private float idleTime;
        private float timer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Fly Idle State Entered");
            animal.StopMoving();
            animal.SetAnimBool(animal.hashIsWalking, true);
            idleTime = Random.Range(animal.Config.idleMinTime, animal.Config.idleMaxTime);
            timer = 0f;
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            BirdController bird = (BirdController)animal;

            if (!animal.Config.friendly && animal.sensor.IsOnSight)
            {   // Target 발견시 Trace상태로 변경
                if (bird)
                    return ((BirdStateMachine)animal.stateMachine).StalkingState;
                return animal.stateMachine.TraceState;
            }
            timer += Time.deltaTime;
            if (timer >= idleTime)
            {
                if (bird)
                    return ((BirdStateMachine)animal.stateMachine).PatrolState;
                return animal.stateMachine.PatrolState;
            }
            return this;
        }
    }
    /// <summary>
    /// 유유자적하며 정찰하며 날아다니는 상태
    /// </summary>
    public class FlyPatrol : BaseState<AIController>
    {
        private Vector3 targetPos;
        private float patrolTimer;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Fly Patrol State Entered");
            animal.SetAnimBool(animal.hashIsWalking, true);
            SetNewDestination(animal);
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // 우호적이지 않고 플레이어를 발견했거나 소리를 들었다면
            if (!animal.Config.friendly && animal.sensor.IsOnSight)
                return ((BirdStateMachine)animal.stateMachine).StalkingState;  // 시야에 들어왔다면 바로 쫓아가서 싸움
            else if (!animal.Config.friendly && animal.sensor.IsOnHeard)
                return ((BirdStateMachine)animal.stateMachine).PatrolState; // 소리만 들었다면 해당 좌표로 순찰

            // 이동
            ((BirdController)animal).RotateTowards(targetPos, animal.Config.rotateSpeed);
            ((BirdController)animal).MoveForward(animal.Config.walkSpeed);

            patrolTimer += Time.deltaTime;
            if (patrolTimer > 0.1f && animal.arrivedAtDestination)
            {
                return animal.stateMachine.IdleState;
            }
            return this;
        }

        void SetNewDestination(AIController animal)
        {
            Vector3 randomPos = Random.insideUnitSphere * 15f;
            randomPos.y = Mathf.Clamp(randomPos.y, 5f, 15f);
            targetPos = animal.transform.position + randomPos;
            animal.SetDestination(targetPos);
            Debug.Log($"{animal.transform.position} -> {randomPos} Distance : {Vector3.Distance(animal.transform.position, randomPos)}");
        }
    }

    /// <summary>
    /// 플레이어를 발견하고 플레이어 주위에서 빙글빙글 도는 상태
    /// </summary>
    public class FlyStalking : BaseState<AIController>
    {
        float timer;
        float angle;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Fly Stalking State Entered");
            timer = 0;
            angle = 0;
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;
            Transform player = animal.target.transform;

            if (player == null) return ((BirdStateMachine)animal.stateMachine).PatrolState;

            // 원형 궤도 위치 계산 = PlayerPos + (Angle * radius) + height
            angle += animal.Config.runSpeed * Time.deltaTime;

            float x = Mathf.Cos(angle) * animal.Config.fovRange;
            float z = Mathf.Sin(angle) * animal.Config.fovRange;

            Vector3 orbitPos = player.position + new Vector3(x, 8f, z);

            // 해당 궤도 위치로 부드럽게 이동
            Vector3 moveDir = (orbitPos - animal.transform.position).normalized;
            animal.transform.position += moveDir * animal.Config.runSpeed * Time.deltaTime;

            // 시선은 플레이어로 (TODO: 이상한경우 조정)
            animal.transform.LookAt(player);

            if (timer >= 3f)
                return ((BirdStateMachine)animal.stateMachine).DiveState;

            return this;
        }
    }

    /// <summary>
    /// 플레이어를 향해 내려찍기 공격을 수행하는 상태
    /// </summary>
    public class FlyDive : BaseState<AIController>
    {
        Vector3 diveTarget;
        bool hasHit;    // 중복 충돌 방지

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Fly Dive State Entered");
            hasHit = false;

            diveTarget = animal.target.transform.position;
            diveTarget.y = animal.target.transform.position.y + 0.5f;

            // 새 공격 충돌박스 켜기
            // animal.Attack.EnableHitbox(true);


        }

        public override void ExitState(AIController animal)
        {
            // animal.Attack.EnableHitbox(false);
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            float step = animal.Config.runSpeed * Time.deltaTime;
            animal.transform.position = Vector3.MoveTowards(animal.transform.position, diveTarget, step);
            animal.transform.LookAt(diveTarget);

            // 목표 지점에 도달 했는가
            if (Vector3.Distance(animal.transform.position, diveTarget) < 0.5f)
                return ((BirdStateMachine)animal.stateMachine).AscentState; // 상승 전환
            return this;
        }
    }

    /// <summary>
    /// Player를 향해 내려찍기를 수행한 다음 다시 고도를 높여 올라가는 상태
    /// </summary>
    public class FlyAscent : BaseState<AIController>
    {
        float targetHeight = 10f;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Fly Ascent State Entered");
            // 현재 위치에서 수직 + 전방 방향으로 상승 목표 설정
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // 상승 로직 : Y축을 높이는 방향으로 이동
            // 단순히 위로만 가면 어색하니까 가던 방향을 유지하며 상승
            Vector3 ascentDir = (animal.transform.forward + Vector3.up * .2f).normalized;

            animal.transform.Translate(ascentDir * animal.Config.runSpeed * Time.deltaTime, Space.World);

            // 회전도 위쪽을 바라보도록
            Quaternion targetRot = Quaternion.LookRotation(ascentDir);
            animal.transform.rotation = Quaternion.Slerp(animal.transform.rotation, targetRot, Time.deltaTime * .5f);
            // 목표치까지 상승했는지 확인
            if (animal.transform.position.y >= targetHeight)
                return ((BirdStateMachine)animal.stateMachine).StalkingState;
            return this;
        }
    }

    /// <summary>
    /// 플레이어로 부터 멀어지며 Despawn하는 상태
    /// </summary>
    public class Retreat : BaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Retreat State Entered");
            animal.sensor.enabled = false;  // 최적화: 센서 끄기
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            if (null == animal.target) return animal.stateMachine.IdleState;

            Vector3 awayDir = (animal.transform.position - animal.target.transform.position).normalized;
            awayDir.y = 0.2f;

            BirdController bird = (BirdController)animal;
            bird.RotateTowards(animal.transform.position + awayDir, 2f);
            bird.MoveForward(animal.Config.walkSpeed * 1.0f);

            float distance = Vector3.Distance(animal.transform.position, animal.target.transform.position);
            if (distance > 50f)
                animal.Despawn();
            return this;
        }
    }
}