using UnityEngine;

namespace Dung.Data
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "DungBeetle/Settings/Player Stats", order = 1)]
    public class PlayerStats : ScriptableObject
    {
        [Header("이동")]
        public MovementData movement;

        [Header("물리 (점프 & 중력)")]
        public PhysicsData physics;

        [Header( "비행")]
        public FlightData flight;

        [Header("쇠똥 밀기")]
        public PushData push;

        [Header("감지")]
        public DetectionData detection;


        [System.Serializable]
        public class MovementData
        {
            [Header("속도")]
            [Tooltip("걷기 최고 속도")]
            public float walkSpeed = 5.0f;

            [Tooltip("달리기 최고 속도")]
            public float runSpeed = 8.0f;

            [Tooltip("회전 속도 (높을수록 즉각 반응)")]
            public float rotationSpeed = 12.0f;

            [Header("반응성")]
            [Tooltip("가속도 (0→최고속 도달 속도)")]
            public float acceleration = 25.0f;

            [Tooltip("감속도 (입력 해제→정지 속도)")]
            public float deceleration = 20.0f;
        }

        [System.Serializable]
        public class PhysicsData
        {
            [Tooltip("점프 힘")]
            public float jumpForce = 5.0f;

            [Tooltip("점프 쿨타임")]
            public float jumpCooldown = 0.8f;

            [Tooltip("낙하 시 중력 배율 (1보다 크면 빠른 낙하)")]
            public float gravityMultiplier = 2.0f;

        }

        [System.Serializable]
        public class FlightData
        {
            [Header("비행 성능")]
            [Tooltip("상승 힘")]
            public float flyUpForce = 15.0f;

            [Tooltip("최대 상승 속도")]
            public float maxFlySpeed = 5.0f;

            [Header("제한")]
            [Tooltip("최대 비행 지속 시간 (초)")]
            public float maxDuration = 1.5f;

            [Tooltip("재사용 대기 시간 (초)")]
            public float cooldown = 10.0f;
        }

        [System.Serializable]
        public class PushData
        {
            [Tooltip("밀기 중 이동 속도 배율")]
            [Range(0f, 1f)] public float speedMultiplier = 0.7f;

            [Tooltip("밀기 중 회전 속도")]
            public float pushRotationSpeed = 4.0f;
        }

        [System.Serializable]
        public class DetectionData
        {
            [Tooltip("지면 감지 레이어")]
            public LayerMask groundLayer;

            [Tooltip("상호작용 가능 레이어")]
            public LayerMask interactLayer;
        }
    }
}