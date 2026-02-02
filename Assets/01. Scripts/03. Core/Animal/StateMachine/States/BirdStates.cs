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
            animal.SetAnimBool(animal.HashIsWalking, true);
            idleTime = Random.Range(animal.Config.idleMinTime, animal.Config.idleMaxTime);
            timer = 0f;
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            ResetRotation(animal);
            timer += Time.deltaTime;

            // 우호적이지 않고 플레이어를 발견했거나 소리를 들었다면
            if (!animal.Config.friendly && animal.Sensor.IsOnSight)
                return ((BirdStateMachine)animal.StateMachine).StalkingState;  // 시야에 들어왔다면 바로 쫓아가서 싸움
            if (timer >= idleTime)
                return ((BirdStateMachine)animal.StateMachine).PatrolState;
            return this;
        }

        private void ResetRotation(AIController animal)
        {
            Transform t = animal.transform;

            // 현재 바라보는 방향(Y)은 유지하고, X와 Z(기울기)만 0으로 설정
            Quaternion targetRotation = Quaternion.Euler(0, t.eulerAngles.y, 0);

            // 부드럽게 회전 (Slerp 사용)
            // 2.0f는 복구 속도입니다. 수치가 높으면 더 빨리 수평이 됩니다.
            t.rotation = Quaternion.Slerp(t.rotation, targetRotation, Time.deltaTime * 2.0f);
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
            animal.SetAnimBool(animal.HashIsWalking, true);
            SetNewDestination(animal);
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // 우호적이지 않고 플레이어를 발견했거나 소리를 들었다면
            if (!animal.Config.friendly && animal.Sensor.IsOnSight)
                return ((BirdStateMachine)animal.StateMachine).StalkingState;  // 시야에 들어왔다면 바로 쫓아가서 싸움
            else if (!animal.Config.friendly && animal.Sensor.IsOnHeard)
                return ((BirdStateMachine)animal.StateMachine).PatrolState; // 소리만 들었다면 해당 좌표로 순찰

            // 만약 현재 높이가 MaxHeight를 넘었다면?
            var bird = (BirdController)animal;
            if (bird.transform.position.y > bird.Config.maxHeight)
            {
                // 강제로 목표 높이를 낮춰서 내려오게 유도
                targetPos.y = bird.Config.maxHeight - 2f;
            }

            // 이동
            ((BirdController)animal).RotateTowards(targetPos, animal.Config.rotateSpeed);
            ((BirdController)animal).MoveForward(animal.Config.walkSpeed);

            patrolTimer += Time.deltaTime;
            if (patrolTimer > 0.1f && animal.ArrivedAtDestination)
            {
                return animal.StateMachine.IdleState;
            }
            return this;
        }

        void SetNewDestination(AIController animal)
        {
            var birdData = animal.Config;

            Vector3 randomPos = Random.insideUnitSphere * birdData.maxHeight;

            targetPos = animal.transform.position + randomPos;
            float yClamp = Mathf.Clamp(targetPos.y, birdData.minHeight, birdData.maxHeight);
            targetPos = new Vector3(targetPos.x, yClamp, targetPos.z);

            animal.SetDestination(targetPos);
            // Debug.Log($"{animal.transform.position} -> {randomPos} Distance : {Vector3.Distance(animal.transform.position, randomPos)}");
        }
    }

    /// <summary>
    /// 플레이어를 발견하고 플레이어 주위에서 빙글빙글 도는 상태
    /// </summary>
    public class FlyStalking : BaseState<AIController>
    {
        float timer;

        float heightAbovePlayer = 25f;
        float angleSpeed = 2.0f;

        float currentAngle;
        Transform target;

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Fly Stalking State Entered");

            timer = 0f;

            // 1. 타겟 플레이어 가져오기 (Sensor나 Manager에서 가져옴)
            // 예시로 sensor에 Target이 있다고 가정하거나 태그로 찾습니다.
            if (animal.Sensor.Target != null)
                target = animal.Sensor.Target.transform;
            else
                target = GameObject.FindGameObjectWithTag("Player").transform;

            // 2. 초기 각도 계산 (부드러운 진입을 위해)
            // 갑자기 0도부터 시작하면 순간이동하듯 튈 수 있으므로, 현재 내 위치가 플레이어 기준 몇 도인지 계산해서 시작합니다.
            Vector3 dirToMe = animal.transform.position - target.position;
            currentAngle = Mathf.Atan2(dirToMe.z, dirToMe.x);
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // 예외 처리: 플레이어가 사라지면 다시 순찰
            if (target == null)
                return ((BirdStateMachine)animal.StateMachine).PatrolState;

            float stalkingDuration = Random.Range(5.0f, 10.0f);

            // 1. 타이머 체크 (5초 지나면 DiveState로 전환)
            timer += Time.deltaTime;
            if (timer >= stalkingDuration)
            {
                return ((BirdStateMachine)animal.StateMachine).DiveState;
            }

            // 2. 원형 궤도 목표 지점 계산 (핵심 로직)
            currentAngle += angleSpeed * Time.deltaTime; // 시간 흐름에 따라 각도 증가

            // 플레이어 위치 + (Cos, Sin으로 원형 오프셋) + 높이
            float x = Mathf.Cos(currentAngle) * animal.Config.fovRange / 2;
            float z = Mathf.Sin(currentAngle) * animal.Config.fovRange / 2;

            Vector3 circlePos = target.position + new Vector3(x, heightAbovePlayer, z);

            // 3. 이동 및 회전
            // BirdController로 형변환하여 이동 함수 호출
            var bird = (BirdController)animal;

            // 목표 지점을 향해 부드럽게 회전
            bird.RotateTowards(circlePos, bird.Config.rotateSpeed);

            // 앞으로 이동 (계속 목표점이 도망가므로 빙글빙글 돌게 됨)
            bird.MoveForward(bird.Config.walkSpeed); // 혹은 stalkingSpeed 등 별도 속도 사용

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
        private bool isPullingUp = false; // 급상승 중인지 체크

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Fly Dive State Entered");
            animal.SetAnimBool(animal.HashIsFlying, false);
            animal.SetAnimBool(animal.HashIsLanding, true);
            hasHit = false;

            var bird = (BirdController)animal;
            Transform target = animal.Sensor.Target.transform;

            if (target != null)
            {
                // 1. 목표 지점 설정: 플레이어 위치
                // 바닥을 뚫지 않게 하기 위해 약간 위(0.5f)를 목표로 잡음
                diveTarget = target.position + Vector3.up * 0.5f;
            }
            else
            {
                // 타겟 없으면 현재 보는 방향 아래로
                diveTarget = animal.transform.position + (animal.transform.forward + Vector3.down) * 10f;
            }

            // 공격 판정 켜기 (Collider)
            // bird.EnableAttackCollider(true); 
            isPullingUp = false;
        }

        public override void ExitState(AIController animal)
        {
            // animal.Attack.EnableHitbox(false);
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            var bird = (BirdController)animal;

            // ★ 1. 바닥 충돌 방지 (Raycast) - 핵심 로직
            // 진행 방향 앞쪽을 미리 감지합니다.
            // 감지 거리: 현재 속도에 비례해서 길게 잡아야 뚫지 않습니다 (최소 2~3m)
            float detectionDistance = 3.0f;

            // Ground 레이어만 감지하도록 LayerMask 설정 (없으면 ~0 으로 모든 레이어)
            // 여기서는 "Ground"라는 레이어가 있다고 가정하거나, 환경 레이어를 지정하세요.
            int groundLayerMask = LayerMask.GetMask("Ground", "Default", "Terrain");

            if (Physics.Raycast(bird.transform.position, bird.transform.forward, out RaycastHit hit, detectionDistance, groundLayerMask))
            {
                Debug.LogWarning("바닥 감지! 급상승 전환");
                return ((BirdStateMachine)animal.StateMachine).AscentState;
            }


            // 2. 이동 로직 (MoveTowards)
            // 단순 Translate보다 MoveTowards가 정확한 지점에 멈추기 좋습니다.
            float step = animal.Config.runSpeed * Time.deltaTime;
            bird.transform.position = Vector3.MoveTowards(bird.transform.position, diveTarget, step);

            // 목표를 향해 회전
            bird.RotateTowards(diveTarget, bird.Config.rotateSpeed * 2f); // 회전도 빠르게


            // 3. 목표 도달 확인 (공격 실패 혹은 완료)
            // 거리가 아주 가까워졌다면 (바닥에 닿기 직전 목표점 도달)
            if (Vector3.Distance(bird.transform.position, diveTarget) < 1.0f)
            {
                return ((BirdStateMachine)animal.StateMachine).AscentState;
            }

            // 4. (안전장치) 혹시라도 Y축이 너무 낮아지면 강제 상승
            // 맵의 바닥 높이가 0이라면 0.5f 정도에서 컷
            if (bird.transform.position.y < 0.5f)
            {
                return ((BirdStateMachine)animal.StateMachine).AscentState;
            }

            return this;
        }
    }

    /// <summary>
    /// Player를 향해 내려찍기를 수행한 다음 다시 고도를 높여 올라가는 상태
    /// </summary>
    public class FlyAscent : BaseState<AIController>
    {
        private float turnSpeed = 5.0f;    // 고개 드는 속도

        public override void EnterState(AIController animal)
        {
            Debug.Log($"{animal.name} Fly Ascent State Entered");
            // 현재 위치에서 수직 + 전방 방향으로 상승 목표 설정

            animal.SetAnimBool(animal.HashIsFlying, true);
            animal.SetAnimBool(animal.HashIsLanding, false);
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            var bird = (BirdController)animal;

            // 1. 이동 방향 계산 (★ 핵심 변경 사항)
            // "내 몸이 보는 방향"이 아니라, "무조건 위쪽 + 내가 가던 수평 방향"으로 강제 설정합니다.

            // 현재 까마귀의 수평 방향(Horizontal Forward) 구하기
            Vector3 horizontalForward = bird.transform.forward;
            horizontalForward.y = 0; // Y축 제거해서 완전 수평으로 만듦
            horizontalForward.Normalize();

            // 상승 벡터: 수평 방향(1) + 수직 방향(2) 정도의 비율로 섞음 (수직 힘을 강하게)
            Vector3 moveDirection = (horizontalForward * 1.0f) + (Vector3.up * 2.0f);
            moveDirection.Normalize();

            // 2. 이동 (Translate 사용 - World 기준)
            // MoveForward 대신 직접 좌표를 옮깁니다. 
            // 이렇게 하면 고개를 아직 처박고 있어도 몸체는 위로 뜹니다.
            bird.transform.Translate(moveDirection * animal.Config.runSpeed * Time.deltaTime, Space.World);


            // 3. 회전 (시각적 연출)
            // 이동은 위 코드로 이미 올라가고 있고, 눈에 보이는 회전은 부드럽게 따라오게 합니다.
            // 목표 방향: 방금 계산한 상승 벡터(moveDirection) 쪽을 바라보게 함
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                bird.transform.rotation = Quaternion.Slerp(bird.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }


            // 4. 상태 전환 체크 (목표 높이 도달 시)
            // 거의 다 올라왔다면 (오차 1m 이내)
            if (bird.transform.position.y >= animal.Config.maxHeight - 1.0f)
            {
                // 다시 순찰(Patrol) 또는 선회(Stalking)로 복귀
                if (animal.Sensor.IsOnHeard || animal.Sensor.IsOnSight)
                    return ((BirdStateMachine)animal.StateMachine).StalkingState;
                else
                    return ((BirdStateMachine)animal.StateMachine).PatrolState;
            }

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
            animal.Sensor.enabled = false;  // 최적화: 센서 끄기
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            if (null == animal.Target) return animal.StateMachine.IdleState;

            Vector3 awayDir = (animal.transform.position - animal.Target.transform.position).normalized;
            awayDir.y = 0.2f;

            BirdController bird = (BirdController)animal;
            bird.RotateTowards(animal.transform.position + awayDir, 2f);
            bird.MoveForward(animal.Config.walkSpeed * 1.0f);

            float distance = Vector3.Distance(animal.transform.position, animal.Target.transform.position);
            if (distance > 50f)
                animal.Despawn();
            return this;
        }
    }
}