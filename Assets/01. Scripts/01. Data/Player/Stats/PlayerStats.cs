using UnityEngine;

namespace Dung.Data
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "DungBeetle/Settings/Player Stats", order = 1)]
    public class PlayerStats : ScriptableObject
    {
        [Header("1. 기본 움직임")]
        public MovementData movement;

        [Header("2. 상호작용 (쇠똥 밀기)")]
        public PushData push;

        [Header("3. 물리 설정 (점프 & 중력)")]
        public PhysicsData physics;

        [Header("4. 비행 설정 (추가됨)")]
        public FlightData flight;

        [Header("5. 감지 설정 (바닥 & 물체)")]
        public DetectionData detection; 


        [System.Serializable]
        public class MovementData
        {
            [Tooltip("걷기 최고 속도")]
            public float walkSpeed = 5.0f;

            [Tooltip("달리기 최고 속도")]
            public float runSpeed = 8.0f;

            [Tooltip("회전 속도")]
            public float rotationSpeed = 12.0f;

            [Space(10)]
            [Tooltip("가속도 (0 -> 최고속도 도달 빠름 정도)")]
            public float acceleration = 25.0f;

            [Tooltip("감속도 (이동 멈춤 -> 정지 빠름 정도)")]
            public float deceleration = 20.0f;

            [Tooltip("방향 전환 부드러움 (SmoothDamp용)")]
            public float speedSmoothTime = 0.1f;
            public float rotationSmoothTime = 0.1f;
        }

        [System.Serializable]
        public class FlightData
        {
            [Tooltip("비행 상승 힘")]
            public float flyUpForce = 15.0f;

            [Tooltip("최고 상승 속도 제한")]
            public float maxFlySpeed = 5.0f;

            [Tooltip("최대 비행 지속 시간")]
            public float maxDuration = 1.5f;

            [Tooltip("비행 후 재사용 대기시간")]
            public float cooldown = 10.0f;
        }

        [System.Serializable]
        public class PushData
        {
            [Tooltip("밀기 속도 배율 (기본속도 * 0.7)")]
            [Range(0f, 1f)] public float speedMultiplier = 0.7f;

            [Tooltip("밀기 회전 속도")]
            public float pushRotationSpeed = 4.0f;
        }

        [System.Serializable]
        public class PhysicsData
        {
            [Tooltip("점프 힘")]
            public float jumpForce = 5.0f;

            [Tooltip("낙하 시 중력 배율 (점프 후 뚝 떨어지는 느낌)")]
            public float gravityMultiplier = 2.0f;

            // 비행 관련 데이터도 필요하다면 여기에 추가
            [Tooltip("비행 상승 힘")]
            public float flyUpForce = 10.0f;
        }

        [System.Serializable]
        public class DetectionData
        {
            [Tooltip("바닥 감지 레이어")]
            public LayerMask groundLayer;

            [Tooltip("상호작용 감지 레이어")]
            public LayerMask interactLayer;
        }
    }
}