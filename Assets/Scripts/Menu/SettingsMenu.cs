using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


//Simon Voss
//Controls settings in manu. Music and graphics, also contains quit method

public class SettingsMenu : MonoBehaviour
{
    #region Singleton
    public static SettingsMenu instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Another instance of : " + instance.ToString() + " was tried to be instanced, but was destroyed from gameobject: " + this.transform.name);
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject.transform.parent);
        }
    }
    #endregion

    [SerializeField] AudioMixer masterMixer = null;
    [SerializeField] UISFXSender sfxSender = null;

    [SerializeField] string sfxChannelName = "SFX Volume";
    [SerializeField] string musicChannelName = "Music Volume";
    [SerializeField] string ambienceChannelName = "Ambience Volume";
    [SerializeField] string masterChannelName = "Master Volume";
    [SerializeField] Slider sfxSlider = null;
    [SerializeField] Slider musicSlider = null;
    [SerializeField] Slider ambienceSlider = null;
    [SerializeField] Slider masterSlider = null;

    bool originalMasterVolumeSet = false;
    float originalMasterVolume = 0;
    const float audioMin = -80f;
    float fadeTransitionSteps = 0.5f;

    [SerializeField] SceneTransition toNewGame = null;

    [SerializeField] GameObject[] openObjects = null;
    [SerializeField] GameObject[] closeObjects = null;
    [SerializeField] MouseLock mouseLockControl = null;
    public bool menuIsOpen = false;

    //To open handbook instantly
    [SerializeField] GameObject handbookPanel = null;
    [SerializeField] GameObject mainMenuPanel = null;
    //

    private void Start()
    {
        masterMixer.GetFloat(sfxChannelName, out float sfx);
        sfxSlider.value = sfx;
        masterMixer.GetFloat(musicChannelName, out float music);
        musicSlider.value = music;
        masterMixer.GetFloat(ambienceChannelName, out float ambience);
        ambienceSlider.value = ambience;
        masterMixer.GetFloat(masterChannelName, out float master);
        masterSlider.value = master;
        originalMasterVolume = master;
        originalMasterVolumeSet = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuIsOpen)
            {
                instance.CloseMenu();
            }
            else
            {
                instance.OpenMenu();
            }
        }
    }
    public void OpenMenu()
    {
        menuIsOpen = true;
        foreach (GameObject menuItem in openObjects)
        {
            menuItem.SetActive(true);
        }
        mouseLockControl.LockToScreen(!menuIsOpen);
        sfxSender.PlayCustom();
    }
    public void CloseMenu()
    {
        menuIsOpen = false;
        foreach (GameObject menuItem in closeObjects)
        {
            menuItem.SetActive(false);
        }
        mouseLockControl.LockToScreen(!menuIsOpen);
        sfxSender.PlayBack();
    }

    public void OpenHandbook()
    {
        OpenMenu();
        handbookPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void SetSFXVolume(float volume)
    {
        masterMixer.SetFloat(sfxChannelName, volume);
    }

    public void SetMusicVolume(float volume)
    {
        masterMixer.SetFloat(musicChannelName, volume);
    }

    public void SetAmbienceVolume(float volume)
    {
        masterMixer.SetFloat(ambienceChannelName, volume);
    }

    public void SetMasterVolume(float volume)
    {
        masterMixer.SetFloat(masterChannelName, volume);
    }

    public void FadeInAudio(float transitionDuration)
    {
        StartCoroutine(FadeInAudioCo(transitionDuration));
    }
    IEnumerator FadeInAudioCo(float transitionDuration)
    {
        Debug.Log("Fading in audio");

        float newVolume = audioMin;
        if (!originalMasterVolumeSet)
        {
            yield return null;
        }
        while (newVolume < originalMasterVolume)
        {
            newVolume += fadeTransitionSteps;
            masterSlider.value = newVolume;
            yield return null;
        }
        masterSlider.value = originalMasterVolume;
    }

    public void FadeOutAudio(float transitionDuration)
    {
        StartCoroutine(FadeOutAudioCo(transitionDuration));
    }
    IEnumerator FadeOutAudioCo(float transitionDuration)
    {
        Debug.Log("Fading out audio");

        //Save original audio volume
        masterMixer.GetFloat(masterChannelName, out originalMasterVolume);

        float newVolume = originalMasterVolume;
        while (newVolume >= audioMin)
        {
            newVolume -= fadeTransitionSteps;
            masterSlider.value = newVolume;
            yield return null;
        }
        masterSlider.value = audioMin;
    }

    public void SetFullScreen(bool state)
    {
        Screen.fullScreen = state;
    }

    public void SetVSync(bool state)
    {
        QualitySettings.vSyncCount = state ? 1 : 0;
    }

    public void RestartGame()
    {
        PlayerSession.instance.EndSession(SessionStats.GameEnd.Restart);
        toNewGame.ChangeScene();
        CloseMenu();
    }

    public void Quitgame()
    {
        Application.Quit();
        Debug.Log("Exit Game");
    }

    public void OnApplicationQuit()
    {
        PlayerSession.instance.EndSession(SessionStats.GameEnd.Exit);
    }
}
