using UnityEngine;

namespace Dung.Data
{
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "DungBeetle/AI/AnimationConfig", order = 1)]
    public class AnimalAnimationConfig : ScriptableObject
    {
        [Header("Movement Parameters")]
        public string moveSpeedFloat = "Locomotion";
        public string idleTypeInt = "IdleType";

        [Header("State Parameters (bool)")]
        public string isWalkingBool = "Patrol";
        public string isRunningBool = "Trace";

        [Header("Action Parameters (trigger)")]
        public string actionTrigger1 = "Attack1";
        public string actionTrigger2 = "Attack2";
        public string actionTrigger3 = "Attack3";
        public string actionTrigger4 = "Attack4";
        [Space(10)]
        public string hitTrigger1 = "GotHit1";
        public string hitTrigger2 = "GotHit2";
        [Space(10)]
        public string dieTrigger1 = "Death1";
        public string dieTrigger2 = "Death2";
    }
}