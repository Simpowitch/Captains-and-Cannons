using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedAudioSource : MonoBehaviour
{
    public enum WhenToPlay { Awake, Enable}
    [SerializeField] WhenToPlay whenToPlay = WhenToPlay.Enable;
    [SerializeField] AudioSource audioSource = null;

    [SerializeField] AudioClip[] clips = null;

    private void Awake()
    {
        if (whenToPlay == WhenToPlay.Awake)
        {
            PlayAudio();
        }
    }

    private void OnEnable()
    {
        if (whenToPlay == WhenToPlay.Enable)
        {
            PlayAudio();
        }
    }

    private void PlayAudio()
    {
        audioSource.PlayOneShot(Utility.ReturnRandom(clips));
        Debug.Log("Playing audio");
    }
}
