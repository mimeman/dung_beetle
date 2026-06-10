using UnityEngine;

namespace Dung.Data
{
    [CreateAssetMenu(fileName = "CrowFlightConfig", menuName = "DungBeetle/AI/CrowFlightConfig", order = 3)]
    public class CrowFlightConfig : ScriptableObject
    {
        [Header("Perlin Noise 비행")]
        public float noiseFrequency = 0.3f;
        public float noiseAmplitude = 15f;
        
        [Header("선회(Stalking) 설정")]
        public float baseSpiralRadius = 12f;
        public float spiralTightenRate = 0.5f;
        public float bobFrequency = 0.8f;
        public float bobAmplitude = 1.5f;
        
        [Header("급강하(Dive)")]
        public AnimationCurve diveSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 3);
        public float diveMaxCorrection = 30f;   // 경로 보정 최대 각도
        public float fakeDiveProbability = 0.3f; // 페이크 다이브 확률
        
        [Header("군집(Boids) 설정")]
        public float separationWeight = 1.5f;
        public float alignmentWeight = 1.0f;
        public float cohesionWeight = 1.0f;
        public float flockRange = 10f;
        
        [Header("착지(Perch)")]
        public float perchMinTime = 10f;
        public float perchMaxTime = 30f;
        public float fleeDistance = 8f;         // 이 거리 이내 진입 시 날아감
        public float perchSearchRadius = 20f;
        public float minPerchHeight = 5f;
        
        [Header("흩어지기(Scatter)")]
        public float scatterSpeedMultiplier = 2f;
        public float scatterDuration = 3f;
    }
}
