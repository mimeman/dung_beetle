using UnityEngine;

namespace BirdStates
{
    // ==========================================================
    // 1. FlyIdle (대기)
    // ==========================================================
    public class FlyIdle : BaseState<AIController>
    {
        private float idleTime;
        private float timer;

        public override void EnterState(AIController animal)
        {
            animal.StopMoving();
            animal.SetAnimBool(animal.HashIsWalking, true);
            idleTime = Random.Range(animal.Config.idleMinTime, animal.Config.idleMaxTime);
            timer = 0f;
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;

            if (!animal.Config.friendly && animal.Sensor.IsOnSight)
                return ((BirdStateMachine)animal.StateMachine).StalkingState;

            if (timer >= idleTime)
                return ((BirdStateMachine)animal.StateMachine).PatrolState;

            return this;
        }
    }

    // ==========================================================
    // 2. FlyPatrol (Perlin Noise + Boids 정찰)
    // ==========================================================
    public class FlyPatrol : BaseState<AIController>
    {
        private Vector3 targetPos;
        private float noiseTimer;

        public override void EnterState(AIController animal)
        {
            animal.SetAnimBool(animal.HashIsWalking, true);
            noiseTimer = ((BirdController)animal).NoiseSeed;
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            var bird = (BirdController)animal;
            var config = bird.FlightConfig;

            if (!animal.Config.friendly && animal.Sensor.IsOnSight)
                return bird.StateMachine.StalkingState;

            // 1. Perlin Noise 기반 경로 계산
            noiseTimer += Time.deltaTime * (config?.noiseFrequency ?? 0.3f);
            float nx = (Mathf.PerlinNoise(noiseTimer, 0) - 0.5f) * (config?.noiseAmplitude ?? 15f);
            float ny = (Mathf.PerlinNoise(0, noiseTimer) - 0.5f) * (config?.noiseAmplitude ?? 15f);
            float nz = (Mathf.PerlinNoise(noiseTimer, noiseTimer) - 0.5f) * (config?.noiseAmplitude ?? 15f);

            Vector3 noiseOffset = new Vector3(nx, ny, nz);
            Vector3 desiredDir = (bird.transform.forward * 5f + noiseOffset).normalized;

            // 2. Boids (군집 행동) 반영
            Vector3 flockVelocity = bird.CalculateFlockingVelocity();
            Vector3 finalDir = (desiredDir + flockVelocity).normalized;

            // 3. 고도 제한 (Terrain 감지)
            if (Physics.Raycast(bird.transform.position, Vector3.down, out RaycastHit hit, bird.Config.minHeight))
            {
                finalDir.y += 1f; // 너무 낮으면 상승
            }
            else if (bird.transform.position.y > bird.Config.maxHeight)
            {
                finalDir.y -= 1f; // 너무 높으면 하강
            }

            // 4. 이동 및 회전
            bird.RotateTowards(bird.transform.position + finalDir, bird.Config.rotateSpeed);
            bird.MoveForward(bird.Config.walkSpeed);

            // 가끔 착지 시도
            if (Random.value < 0.001f) return bird.StateMachine.PerchState;

            return this;
        }
    }

    // ==========================================================
    // 3. FlyStalking (나선형 정찰 + 페이크 다이브)
    // ==========================================================
    public class FlyStalking : BaseState<AIController>
    {
        private float timer;
        private float currentAngle;
        private Transform target;
        private float spiralRadius;

        public override void EnterState(AIController animal)
        {
            timer = 0f;
            target = animal.Sensor.Target?.transform;
            if (target == null) target = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (target != null)
            {
                Vector3 dirToMe = animal.transform.position - target.position;
                currentAngle = Mathf.Atan2(dirToMe.z, dirToMe.x);
                spiralRadius = ((BirdController)animal).FlightConfig?.baseSpiralRadius ?? 12f;
            }
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            if (target == null) return ((BirdStateMachine)animal.StateMachine).PatrolState;

            var bird = (BirdController)animal;
            var config = bird.FlightConfig;

            timer += Time.deltaTime;
            
            // 나선형 궤도: 반지름이 점점 좁아짐
            spiralRadius = Mathf.Lerp(spiralRadius, bird.Config.attackRange + 2f, Time.deltaTime * (config?.spiralTightenRate ?? 0.5f));
            currentAngle += bird.Config.walkSpeed * 0.5f * Time.deltaTime;

            float x = Mathf.Cos(currentAngle) * spiralRadius;
            float z = Mathf.Sin(currentAngle) * spiralRadius;
            float yBob = Mathf.Sin(timer * (config?.bobFrequency ?? 0.8f)) * (config?.bobAmplitude ?? 1.5f);

            Vector3 targetPoint = target.position + new Vector3(x, (bird.Config.maxHeight + bird.Config.minHeight) * 0.5f + yBob, z);

            bird.RotateTowards(targetPoint, bird.Config.rotateSpeed);
            bird.MoveForward(bird.Config.walkSpeed * 1.2f);

            // 공격 조건 체크
            if (timer > 5f)
            {
                // 페이크 다이브 확률 체크
                if (Random.value < (config?.fakeDiveProbability ?? 0.3f))
                {
                    Debug.Log("[CrowAI] Fake Dive!");
                    // 페이크 다이브 연출 후 다시 Stalking 유지하는 로직은 Dive State 내부에서 처리
                }
                return bird.StateMachine.DiveState;
            }

            return this;
        }
    }

    // ==========================================================
    // 4. FlyDive (포물선 급강하 공격)
    // ==========================================================
    public class FlyDive : BaseState<AIController>
    {
        private Vector3 startPos;
        private Vector3 targetPos;
        private float progress;
        private bool isFake;

        public override void EnterState(AIController animal)
        {
            var bird = (BirdController)animal;
            startPos = bird.transform.position;
            targetPos = bird.Sensor.Target?.transform.position ?? bird.transform.position + bird.transform.forward * 10f;
            progress = 0f;
            
            isFake = Random.value < (bird.FlightConfig?.fakeDiveProbability ?? 0.3f);
            
            animal.SetAnimBool(animal.HashIsFlying, false);
            animal.SetAnimBool(animal.HashIsLanding, true);
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            var bird = (BirdController)animal;
            var config = bird.FlightConfig;

            progress += Time.deltaTime * bird.Config.runSpeed * 0.1f;
            float speedMult = config?.diveSpeedCurve.Evaluate(progress) ?? 1f;

            // 포물선 이동: 직선 이동 + AnimationCurve 기반 높이 보정
            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, progress);
            // 실제 바닥에 꽂히기 전에 상승하도록 페이크 처리
            if (isFake && progress > 0.6f) return bird.StateMachine.AscentState;

            bird.transform.position = Vector3.MoveTowards(bird.transform.position, linearPos, Time.deltaTime * bird.Config.runSpeed * speedMult);
            bird.RotateTowards(targetPos, bird.Config.rotateSpeed * 2f);

            // 공격 판정 및 충돌 회피
            if (progress >= 0.9f || Vector3.Distance(bird.transform.position, targetPos) < 1.5f)
            {
                bird.ApplyDamageToTarget();
                return bird.StateMachine.AscentState;
            }

            // 바닥 감지 안전 장치
            if (Physics.Raycast(bird.transform.position, bird.transform.forward, 2f, LayerMask.GetMask("Ground", "Default")))
            {
                return bird.StateMachine.AscentState;
            }

            return this;
        }
    }

    // ==========================================================
    // 5. FlyAscent (급상승)
    // ==========================================================
    public class FlyAscent : BaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            animal.SetAnimBool(animal.HashIsFlying, true);
            animal.SetAnimBool(animal.HashIsLanding, false);
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            var bird = (BirdController)animal;
            
            Vector3 upDir = (bird.transform.forward + Vector3.up * 2f).normalized;
            bird.transform.Translate(upDir * bird.Config.runSpeed * Time.deltaTime, Space.World);
            bird.RotateTowards(bird.transform.position + upDir, bird.Config.rotateSpeed);

            if (bird.transform.position.y >= bird.Config.maxHeight * 0.8f)
            {
                return bird.StateMachine.PatrolState;
            }

            return this;
        }
    }

    // ==========================================================
    // 6. FlyPerch (높은 곳에 앉아 대기) — 신규
    // ==========================================================
    public class FlyPerch : BaseState<AIController>
    {
        private Vector3 perchPoint;
        private bool isArrived;
        private float stayTimer;

        public override void EnterState(AIController animal)
        {
            isArrived = false;
            stayTimer = 0f;
            
            // 자동 PerchPoint 탐색
            if (FindPerchPoint(animal, out perchPoint))
            {
                Debug.Log($"[CrowAI] Found perch point at {perchPoint}");
            }
            else
            {
                animal.ChangeState(((BirdStateMachine)animal.StateMachine).PatrolState);
            }
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            var bird = (BirdController)animal;
            
            if (!isArrived)
            {
                bird.RotateTowards(perchPoint, bird.Config.rotateSpeed);
                bird.transform.position = Vector3.MoveTowards(bird.transform.position, perchPoint, bird.Config.walkSpeed * Time.deltaTime);

                if (Vector3.Distance(bird.transform.position, perchPoint) < 0.2f)
                {
                    isArrived = true;
                    bird.StopMoving();
                    bird.SetAnimBool(bird.HashIsWalking, false);
                }
            }
            else
            {
                stayTimer += Time.deltaTime;
                
                // 위협 감지 시 비행
                if (bird.IsAnyPlayerNear) return bird.StateMachine.ScatterState;
                
                if (stayTimer > (bird.FlightConfig?.perchMaxTime ?? 20f))
                {
                    return bird.StateMachine.PatrolState;
                }
            }

            return this;
        }

        private bool FindPerchPoint(AIController animal, out Vector3 point)
        {
            point = Vector3.zero;
            var config = ((BirdController)animal).FlightConfig;
            float radius = config?.perchSearchRadius ?? 20f;
            
            // 주변 높은 오브젝트 탐색
            Collider[] cols = Physics.OverlapSphere(animal.transform.position, radius);
            foreach (var col in cols)
            {
                if (col.transform.position.y > animal.transform.position.y - 5f)
                {
                    // 오브젝트 상단 레이캐스트
                    Vector3 origin = col.bounds.center + Vector3.up * col.bounds.extents.y + Vector3.up * 2f;
                    if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 5f))
                    {
                        if (hit.point.y > (config?.minPerchHeight ?? 5f))
                        {
                            point = hit.point;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    // ==========================================================
    // 7. FlyScatter (위협 시 사방으로 흩어짐) — 신규
    // ==========================================================
    public class FlyScatter : BaseState<AIController>
    {
        private Vector3 scatterDir;
        private float timer;

        public override void EnterState(AIController animal)
        {
            timer = 0f;
            // 위협(주로 플레이어)의 반대 방향으로 흩어짐
            Vector3 threatPos = animal.Sensor.Target?.transform.position ?? animal.transform.position + Vector3.down;
            scatterDir = (animal.transform.position - threatPos).normalized;
            scatterDir.y += 0.5f; // 위쪽으로도 비행
            scatterDir.Normalize();
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            var bird = (BirdController)animal;
            timer += Time.deltaTime;

            bird.RotateTowards(bird.transform.position + scatterDir, bird.Config.rotateSpeed * 2f);
            bird.MoveForward(bird.Config.runSpeed * (bird.FlightConfig?.scatterSpeedMultiplier ?? 2f));

            if (timer > (bird.FlightConfig?.scatterDuration ?? 3f))
            {
                return bird.StateMachine.PatrolState;
            }

            return this;
        }
    }
}