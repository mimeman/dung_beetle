using UnityEngine;

namespace Dung.Data
{
    [CreateAssetMenu(fileName = "DungStats", menuName = "DungBeetle/Stats/Dung Physics Stats", order = 1)]
    public class DungStats : ScriptableObject
    {
        public GrowthData growth;
        public PhysicsData physics;
        public VisualData visual;

        [System.Serializable]
        public class GrowthData
        {
            [Header("질량 설정")]
            public float initialMass = 5.0f;
            public float maxMass = 1000.0f;

            [Tooltip("흡수 배율 (1.0 = 정상)")]
            public float absorptionRate = 1.0f;

            [Header("크기(반지름) 설정")]
            [Tooltip("시작할 때 똥의 반지름")]
            public float initialRadius = 0.5f;

            [Tooltip("최대로 커졌을 때의 반지름")]
            public float maxRadius = 3.0f;
        }

        [System.Serializable]
        public class PhysicsData
        {
            [Header("저항력 (0.0 ~ 1.0)")]
            public float weatherResistance = 0.2f;
            public float impactResistance = 0.5f;

            [Header("물리 특성")]
            public float bounciness = 0.1f;
            public float friction = 0.6f;

            [Tooltip("똥의 레이어 (감지용)")]
            public LayerMask dungLayer;

            [Tooltip("공이 구를 때 적용할 공기 저항")]
            public float linearDrag = 0.5f;

            [Tooltip("공이 회전할 때 적용할 회전 저항")]
            public float angularDrag = 0.1f;
        }

        [System.Serializable]
        public class VisualData
        {
            [Header("Visuals")]
            public Material skinMaterial;
            public GameObject crumbleEffect;

            [Header("IK Settings")]
            [Tooltip("똥 중심으로부터 왼손이 위치할 기본 거리")]
            public Vector3 leftHandOffset = new Vector3(-0.3f, 0, 0.5f);

            [Tooltip("똥 중심으로부터 오른손이 위치할 기본 거리")]
            public Vector3 rightHandOffset = new Vector3(0.3f, 0, 0.5f);

            [Header("Sounds")]
            public AudioClip rollSound;      
            public AudioClip hitHard;        
            public AudioClip hitSoft;        
            public AudioClip growSound;      
            public AudioClip breakSound;     
            public float maxPitch = 1.5f;    
        }
    }
}