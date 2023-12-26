using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//A manager we can reach from all buttons in the game to play Audio

public class UISFXManager : MonoBehaviour
{
    public enum SFX { Select, Back, Hoover}

    [SerializeField] AudioSource audioSource = null;

    [SerializeField] AudioClip select = null;
    [SerializeField] AudioClip back = null;
    [SerializeField] AudioClip hoover = null;


    #region Singleton
    public static UISFXManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Another instance of : " + instance.ToString() + " was tried to be instanced, but was destroyed from gameobject: " + this.transform.name);
            GameObject.Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    #endregion

    public static void PlayUISFX(SFX type)
    {
        instance.PlayAudio(type);
    }

    public static void PlayUISFX(AudioClip clip)
    {
        instance.PlayAudio(clip);
    }

    private void PlayAudio(SFX type)
    {
        AudioClip audio = null;

        switch (type)
        {
            case SFX.Select:
                audio = select;
                break;
            case SFX.Back:
                audio = back;
                break;
            case SFX.Hoover:
                audio = hoover;
                break;
        }

        audioSource.PlayOneShot(audio);
    }

    private void PlayAudio(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
