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
            public float weatherResistance = 0.2f;
            public float impactResistance = 0.5f;

            [Header("물리 특성")]
            public float bounciness = 0.1f;
            public float friction = 0.6f;
        }

        [System.Serializable]
        public class VisualData
        {
            [Header("Visuals")]
            public Material skinMaterial;
            public GameObject crumbleEffect;

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