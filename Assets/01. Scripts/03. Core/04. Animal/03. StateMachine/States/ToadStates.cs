using UnityEngine;
using UnityEngine.AI;

namespace ToadStates
{// ==================================================================================
    // 1. Idle (대기/위장)
    // - 위장 상태 유지, 플레이어 감지 시 Aiming 전환
    // ==================================================================================
    public class Idle : BaseState<AIController>
    {
        ToadController toad;
        public override void EnterState(AIController animal)
        {
            if (!toad)
                toad = (ToadController)animal;

            toad.Camouflage.SetCamouflage(true); // 위장 켜기
            toad.SetAnimTrigger(toad.HashIdle);  // 대기 애니메이션
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            ToadStateMachine stateMachine = (ToadStateMachine)animal.StateMachine;

            // 플레이어 감지 시 Aiming 상태로 전환
            if (animal.Sensor.IsOnSight)
            {
                return stateMachine.AimingState;
            }
            return this;
        }
    }

    // ==================================================================================
    // 2. Aiming (조준)
    // - 위장 해제, 타겟 조준, 일정 시간 후 발사(Snap)
    // ==================================================================================
    public class Aiming : BaseState<AIController>
    {
        ToadController toad;
        private float timer;

        public override void EnterState(AIController animal)
        {
            if (!toad)
                toad = (ToadController)animal;

            timer = 0f;
            toad.Camouflage.SetCamouflage(false); // 위장 해제 시작
            animal.SetAnimTrigger(toad.HashAim);    // 조준 애니메이션 (입 벌리기)
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;

            // 타겟을 향해 회전
            toad.RotateTowardsTarget();

            // 플레이어가 시야 밖으로 사라지면 다시 Idle
            if (!animal.Sensor.IsOnSight)
            {
                return toad.StateMachine.IdleState;
            }

            // 조준 시간 완료 -> 발사
            if (timer >= toad.ToadConfig.aimingTime)
            {
                return toad.StateMachine.SnapState;
            }

            return this;
        }
    }

    // ==================================================================================
    // 3. Snap (혀 발사)
    // - 혀 발사 명령, 충돌 결과 대기 (이벤트/콜백으로 상태 전환)
    // ==================================================================================
    public class Snap : BaseState<AIController>
    {
        ToadController toad;

        public override void EnterState(AIController animal)
        {
            if (!toad)
                toad = (ToadController)animal;

            animal.SetAnimTrigger(toad.HashSnap); // 혀 발사 애니메이션
            toad.Tongue.FireTongue();               // 물리적인 혀 발사
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // TongueController에서 충돌 결과에 따라 상태를 직접 바꿔주거나
            // 여기서 플래그를 체크할 수 있습니다. 
            // *설계상 TongueController.OnHit()에서 animal.StateMachine.ChangeState()를 호출하는 구조라면
            // 여기서는 아무것도 안 해도 됩니다.

            return this;
        }
    }

    // ==================================================================================
    // 4. Pull (끌어당기기)
    // - 플레이어를 입 앞으로 당김, 완료 시 Bite 전환
    // ==================================================================================
    public class Pull : BaseState<AIController>
    {
        ToadController toad;
        public override void EnterState(AIController animal)
        {
            if (!toad)
                toad = (ToadController)animal;

            animal.SetAnimTrigger(toad.HashPull); // 당기는 애니메이션
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            // 끌어당기기 로직 수행 (완료 시 true 반환)
            if (toad.PullSystem.PullTarget())
            {
                return toad.StateMachine.BiteState;
            }
            return this;
        }
    }

    // ==================================================================================
    // 5. Bite (섭식)
    // - 데미지 적용, 섭식 애니메이션, 완료 후 Cooldown
    // ==================================================================================
    public class Bite : BaseState<AIController>
    {
        ToadController toad;
        private float timer;
        private float biteDuration = 1.0f; // 씹는 시간 (애니메이션 길이)

        public override void EnterState(AIController animal)
        {
            timer = 0f;
            toad.SetAnimTrigger(toad.HashBite); // 씹는 애니메이션

            // 데미지 적용
            if (animal.Target != null)
            {
                var health = animal.Target.GetComponent<IDamageable>();
                if (health != null) health.TakeDamage(toad.ToadConfig.biteDamage);
            }
        }

        public override void ExitState(AIController animal)
        {
            toad.Tongue.ResetTongue(); // 혀 제거/초기화
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;

            // 섭식 애니메이션 끝나면 쿨다운으로
            if (timer >= biteDuration)
            {
                return toad.StateMachine.CooldownState;
            }
            return this;
        }
    }

    // ==================================================================================
    // 6. Stuck (스턴 - 쇠똥 공 충돌 시)
    // - 일정 시간 마비, 피격 가능 상태
    // ==================================================================================
    public class Stuck : BaseState<AIController>
    {
        ToadController toad;
        private float timer;

        public override void EnterState(AIController animal)
        {
            if (!toad)
                toad = (ToadController)animal;

            timer = 0f;
            animal.SetAnimTrigger(toad.HashStuck); // 당황/기절 애니메이션
            // 약점 노출 효과 등 추가 가능
        }

        public override void ExitState(AIController animal)
        {
            toad.Tongue.ResetTongue();
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;

            if (timer >= toad.ToadConfig.stunDuration)
            {
                return toad.StateMachine.RecoverState;
            }
            return this;
        }
    }

    // ==================================================================================
    // 7. Recover (회복)
    // - 스턴 후 자세 정비
    // ==================================================================================
    public class Recover : BaseState<AIController>
    {
        ToadController toad;
        private float timer;
        private float recoverDuration = 1.0f;

        public override void EnterState(AIController animal)
        {
            timer = 0f;
            animal.SetAnimTrigger(toad.HashRecover);
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;
            if (timer >= recoverDuration)
            {
                return toad.StateMachine.CooldownState;
            }
            return this;
        }
    }

    // ==================================================================================
    // 8. Cooldown (쿨다운)
    // - 공격 후 잠시 대기하다가 다시 Idle(위장)로 복귀
    // ==================================================================================
    public class Cooldown : BaseState<AIController>
    {
        ToadController toad;
        private float timer;

        public override void EnterState(AIController animal)
        {
            if (!toad)
                toad = (ToadController)animal;

            timer = 0f;
            // 경계 태세 유지 (Idle 애니메이션과 다를 수 있음)
            animal.SetAnimTrigger(toad.HashIdle);
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            timer += Time.deltaTime;
            if (timer >= toad.ToadConfig.cooldownTime)
            {
                return animal.StateMachine.IdleState;
            }
            return this;
        }
    }
}