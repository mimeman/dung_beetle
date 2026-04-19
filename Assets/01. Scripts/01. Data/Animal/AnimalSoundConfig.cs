using UnityEngine;

namespace Dung.Data
{
    [CreateAssetMenu(fileName = "AnimalSoundConfig", menuName = "DungBeetle/AI/SoundConfig", order = 2)]
    public class AnimalSoundConfig : ScriptableObject
    {
        [System.Serializable]
        public class StateSoundEntry
        {
            public AnimalStateID stateID;      // Idle, Eat, Fly 등
            public AudioClip[] clips;          // 랜덤 재생용 복수 클립
            [Range(0f, 1f)] public float volume = 1f;
            public float minInterval = 2f;     // 랜덤 재생 최소 간격 (OneShot인 경우)
            public float maxInterval = 5f;     // 랜덤 재생 최대 간격
            public bool loop;                  // 루프 여부 (지속적인 소리)
        }

        [Header("State별 사운드 설정")]
        public StateSoundEntry[] stateSounds;

        [Header("공통 효과음")]
        public AudioClip hitSound;
        public AudioClip deathSound;
        
        public StateSoundEntry GetEntry(AnimalStateID id)
        {
            foreach (var entry in stateSounds)
            {
                if (entry.stateID == id) return entry;
            }
            return null;
        }
    }
}
