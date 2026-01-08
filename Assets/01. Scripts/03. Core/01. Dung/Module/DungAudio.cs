using UnityEngine;
using Dung.Data; // DungStats 사용

[RequireComponent(typeof(AudioSource))]
public class DungAudio : MonoBehaviour
{
    private AudioSource _rollSource; // 구르는 소리 (Loop)
    private AudioSource _sfxSource;  // 효과음 (OneShot)

    private DungStats _stats;
    private Rigidbody _rb;
    private bool _isGrounded;

    public void Initialize(DungStats stats)
    {
        _stats = stats;
        _rb = GetComponent<Rigidbody>();

        // 1. 구르는 소리용 소스 설정
        _rollSource = GetComponent<AudioSource>();
        _rollSource.loop = true;
        _rollSource.playOnAwake = false;
        _rollSource.spatialBlend = 1.0f; // 3D 사운드
        _rollSource.volume = 0f;

        if (stats.visual.rollSound != null)
        {
            _rollSource.clip = stats.visual.rollSound;
            _rollSource.Play();
        }

        // 2. 효과음용 소스 추가
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.spatialBlend = 1.0f;
    }

    private void Update()
    {
        if (_stats == null || _rb == null) return;

        HandleRollSound();
    }

    // 구르는 속도에 맞춰 볼륨/피치 조절
    private void HandleRollSound()
    {
        if (!_isGrounded || _rb.velocity.sqrMagnitude < 0.1f)
        {
            _rollSource.volume = Mathf.Lerp(_rollSource.volume, 0f, Time.deltaTime * 5f);
            return;
        }

        float speed = _rb.velocity.magnitude;
        float targetVol = Mathf.Clamp01(speed / 10.0f);
        float targetPitch = Mathf.Lerp(0.8f, _stats.visual.maxPitch, speed / 10.0f);

        _rollSource.volume = Mathf.Lerp(_rollSource.volume, targetVol, Time.deltaTime * 5f);
        _rollSource.pitch = targetPitch;
    }

    // 바닥 체크 및 충돌 소리
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) _isGrounded = true;

        if (_stats == null) return;

        float impact = collision.relativeVelocity.magnitude;
        if (impact > 2.0f)
        {
            // 세게 박으면 Hard, 약하면 Soft
            AudioClip clip = (impact > 8.0f) ? _stats.visual.hitHard : _stats.visual.hitSoft;
            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip, Mathf.Clamp01(impact / 15.0f));
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) _isGrounded = false;
    }

    // 외부 호출용
    public void PlayGrowSound()
    {
        if (_stats != null && _stats.visual.growSound != null)
            _sfxSource.PlayOneShot(_stats.visual.growSound);
    }

    public void PlayBreakSound()
    {
        if (_stats != null && _stats.visual.breakSound != null)
            _sfxSource.PlayOneShot(_stats.visual.breakSound);
    }
}