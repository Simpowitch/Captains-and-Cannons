using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SessionRestart : MonoBehaviour
{
    [SerializeField] GameObject tutorial = null;
    const string tutorialPlayerPrefs = "tutorial done";

    [SerializeField] bool restartTutorialInEditor = true;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartSessionWithDelay());

#if UNITY_EDITOR
        if (restartTutorialInEditor)
        {
            PlayerPrefs.DeleteKey(tutorialPlayerPrefs); //Reset playerprefs of tutorial
        }
#endif
        //Show tutorial if no playerprefs has been set, or if the last playerprefs of the tutorial was set in a different version than the current
        tutorial.SetActive(!PlayerPrefs.HasKey(tutorialPlayerPrefs) && PlayerPrefs.GetString(tutorialPlayerPrefs) != Application.version);
    }

    IEnumerator StartSessionWithDelay()
    {
        yield return null;
        PlayerSession.instance.StartNewSession();
    }

    //Mark the tutorial as read so it does not pop up ever again on the machine
    public void TutorialRead()
    {
        PlayerPrefs.SetString(tutorialPlayerPrefs, Application.version);
    }
}
