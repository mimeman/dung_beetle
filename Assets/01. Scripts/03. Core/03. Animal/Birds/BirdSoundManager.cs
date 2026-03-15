using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip flySound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayFlySound()
    {
        audioSource.PlayOneShot(flySound, 1f);
    }
}
