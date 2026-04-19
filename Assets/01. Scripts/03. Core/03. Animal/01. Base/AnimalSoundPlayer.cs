using UnityEngine;
using Dung.Data;
using System.Collections;

public class AnimalSoundPlayer : MonoBehaviour
{
    [SerializeField] private AnimalSoundConfig config;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioSource sfxSource;

    private AIController controller;
    private Coroutine randomPlayCoroutine;
    private AnimalSoundConfig.StateSoundEntry currentEntry;

    private void Awake()
    {
        controller = GetComponent<AIController>();
        
        if (loopSource == null)
        {
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.loop = true;
            loopSource.spatialBlend = 1f;
        }
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.spatialBlend = 1f;
        }
    }

    private void OnEnable()
    {
        if (controller != null)
        {
            controller.OnStateChanged += HandleStateChanged;
            
            // 초기 상태 처리
            if (controller.CurrentState != null)
            {
                HandleStateChanged(controller.CurrentState);
            }
        }
    }

    private void OnDisable()
    {
        if (controller != null)
        {
            controller.OnStateChanged -= HandleStateChanged;
        }
        StopAllCoroutines();
    }

    private void HandleStateChanged(BaseState<AIController> newState)
    {
        if (config == null) return;

        // 상태 이름으로 ID 유추 (현재 BaseState 구조상 직접 ID를 가져오기 어려움)
        // 설계상 BaseState에 StateID를 추가하거나, State 유형에 따라 매핑해야 함.
        // 여기서는 간단한 매핑 로직을 사용하거나, 각 State 클래스에서 직접 ID를 전달하도록 설계 확장 가능.
        
        AnimalStateID stateID = GetStateIDFromState(newState);
        UpdateSoundState(stateID);
    }

    private AnimalStateID GetStateIDFromState(BaseState<AIController> state)
    {
        string stateName = state.GetType().Name;
        
        // 클래스 이름 기반 매핑 (BirdStates.FlyIdle -> Idle, AIStates.Idle -> Idle 등)
        if (stateName.Contains("Idle")) return AnimalStateID.Idle;
        if (stateName.Contains("Patrol")) return AnimalStateID.Patrol;
        if (stateName.Contains("Trace")) return AnimalStateID.Trace;
        if (stateName.Contains("Attack")) return AnimalStateID.Attack;
        if (stateName.Contains("Hit")) return AnimalStateID.Hit;
        if (stateName.Contains("Die")) return AnimalStateID.Die;
        if (stateName.Contains("Eat")) return AnimalStateID.Eat;
        if (stateName.Contains("Poo")) return AnimalStateID.Poo;
        if (stateName.Contains("Sleep")) return AnimalStateID.Sleep;
        
        // 특정 비행 상태
        if (stateName.Contains("Fly")) return AnimalStateID.Fly;
        if (stateName.Contains("Stalking")) return AnimalStateID.Stalking;
        if (stateName.Contains("Dive")) return AnimalStateID.Dive;
        if (stateName.Contains("Ascent")) return AnimalStateID.Ascent;
        
        // 두꺼비 상태
        if (stateName.Contains("Aiming")) return AnimalStateID.Aiming;
        if (stateName.Contains("Snap")) return AnimalStateID.Snap;
        if (stateName.Contains("Pull")) return AnimalStateID.Pull;
        if (stateName.Contains("Bite")) return AnimalStateID.Bite;
        
        return AnimalStateID.Idle;
    }

    private void UpdateSoundState(AnimalStateID stateID)
    {
        if (randomPlayCoroutine != null)
        {
            StopCoroutine(randomPlayCoroutine);
            randomPlayCoroutine = null;
        }

        currentEntry = config.GetEntry(stateID);
        
        if (currentEntry == null || currentEntry.clips == null || currentEntry.clips.Length == 0)
        {
            loopSource.Stop();
            return;
        }

        if (currentEntry.loop)
        {
            loopSource.clip = currentEntry.clips[Random.Range(0, currentEntry.clips.Length)];
            loopSource.volume = currentEntry.volume;
            loopSource.Play();
        }
        else
        {
            loopSource.Stop();
            randomPlayCoroutine = StartCoroutine(RandomPlayRoutine());
        }
    }

    private IEnumerator RandomPlayRoutine()
    {
        while (true)
        {
            if (currentEntry != null && currentEntry.clips.Length > 0)
            {
                AudioClip clip = currentEntry.clips[Random.Range(0, currentEntry.clips.Length)];
                sfxSource.PlayOneShot(clip, currentEntry.volume);
            }
            
            float waitTime = Random.Range(currentEntry.minInterval, currentEntry.maxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    public void PlayHitSound()
    {
        if (config != null && config.hitSound != null)
            sfxSource.PlayOneShot(config.hitSound);
    }
    
    public void PlayDeathSound()
    {
        if (config != null && config.deathSound != null)
            sfxSource.PlayOneShot(config.deathSound);
    }
}
