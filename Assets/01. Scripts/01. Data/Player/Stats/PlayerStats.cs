using UnityEngine;

namespace Dung.Data
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "DungBeetle/Settings/Player Stats", order = 1)]
    public class PlayerStats : ScriptableObject
    {
        [Header("기본 움직임")]
        public MovementData movement;

        [Header("쇠똥 밀기 (상호작용)")]
        public PushData push;

        [Header("물리 및 환경")]
        public PhysicsData physics;

        [Header("감지 설정")]
        public DetectionData detection;



        [System.Serializable]
        public class MovementData
        {
            [Tooltip("걷기 최고 속도")]
            public float walkSpeed = 5.0f;

            [Tooltip("달리기 최고 속도")]
            public float runSpeed = 8.0f;

            [Tooltip("회전 속도 (클수록 빠릿하게 돔)")]
            public float rotationSpeed = 12.0f;

            [Space(10)]
            [Tooltip("가속도 (0 -> 최고속도 도달 시간 관여). 높을수록 반응 빠름.")]
            public float acceleration = 25.0f;

            [Tooltip("감속도 (이동 멈춤 -> 0 도달 시간 관여). 낮으면 빙판길.")]
            public float deceleration = 20.0f;
        }

        [System.Serializable]
        public class PushData
        {
            [Tooltip("쇠똥을 밀 때의 기본 최고 속도")]
            public float pushMaxSpeed = 3.0f;

            [Tooltip("밀기 시작할 때의 가속도 (무거운 느낌을 위해 낮게 설정)")]
            public float pushAcceleration = 8.0f;

            [Tooltip("밀 때 회전 속도 (탱크처럼 둔하게)")]
            public float pushRotationSpeed = 4.0f;

            [Tooltip("쇠똥 무게에 따른 속도 감소율 (1.0 = 정직하게 무거워짐)")]
            [Range(0f, 2f)] public float weightPenalty = 0.5f;
        }

        [System.Serializable]
        public class PhysicsData
        {
            [Tooltip("점프 힘")]
            public float jumpForce = 5.0f;

            [Tooltip("추가 중력 (점프 후 뚝 떨어지는 느낌)")]
            public float gravityMultiplier = 2.0f;

            [Tooltip("등판 각도 제한 (이 각도보다 가파르면 못 올라감)")]
            [Range(0f, 90f)] public float maxSlopeAngle = 45.0f;

            [Tooltip("경사로에서 미끄러지는 힘")]
            public float slideFriction = 0.2f;
        }

        [System.Serializable]
        public class DetectionData
        {
            [Tooltip("상호작용(잡기) 가능한 거리")]
            public float interactRange = 1.0f;

            [Tooltip("감지 범위 (반지름)")]
            public float detectRadius = 0.5f;

            [Tooltip("잡을 수 있는 물체 레이어")]
            public LayerMask interactLayer;

            [Tooltip("바닥으로 인식할 레이어")]
            public LayerMask groundLayer;
        }
    }
}