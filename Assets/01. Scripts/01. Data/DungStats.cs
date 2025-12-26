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
        }

        [System.Serializable]
        public class PhysicsData
        {
            [Header("저항력 (0.0 ~ 1.0)")]
            public float weatherResistance = 0.2f; // 비 저항
            public float impactResistance = 0.5f;  // 충돌 저항

            [Header("물리 특성")]
            public float bounciness = 0.1f;
            public float friction = 0.6f;
        }

        [System.Serializable]
        public class VisualData
        {
            public Material skinMaterial;    // 3D 모델 표면
            public GameObject crumbleEffect; // 파괴 이펙트
            public AudioClip rollSound;      // 구르는 소리
        }
    }
}