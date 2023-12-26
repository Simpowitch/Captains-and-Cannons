using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

//Simon Voss
//Simple scene-transition, with prepared possibility for animations

public class SceneTransition : MonoBehaviour
{
    [SerializeField] Animator sceneTransitionAnimator = null;
    [SerializeField] bool dontDestroyOnLoad = false;

    [SerializeField] int buildIndex = 0;

    [SerializeField] float timeBeforeTransitionStart = 0f;
    [SerializeField] float transitionDuration = 0.5f;

    [SerializeField] bool fadeInAudioAtStart = true;

    private void Start()
    {
        if (fadeInAudioAtStart)
        {
            if (SettingsMenu.instance)
            {
                SettingsMenu.instance.FadeInAudio(transitionDuration);
            }
        }
    }
    public void ChangeScene()
    {
        if (sceneTransitionAnimator != null)
        {
            StartCoroutine(PlaySceneTransition());
        }
        else
        {
            SceneManager.LoadScene(buildIndex);
        }
    }

    IEnumerator PlaySceneTransition()
    {
        yield return new WaitForSeconds(timeBeforeTransitionStart);

        //Play animation (fade out)
        if (sceneTransitionAnimator != null)
        {
            sceneTransitionAnimator.SetTrigger("Play");
            if (SettingsMenu.instance)
            {
                SettingsMenu.instance.FadeOutAudio(transitionDuration);
            }
        }


        yield return new WaitForSeconds(transitionDuration);

        //Play animation (fade in)
        if (sceneTransitionAnimator != null && dontDestroyOnLoad)
        {
            sceneTransitionAnimator.SetTrigger("Reset");
            if (fadeInAudioAtStart)
            {
                if (SettingsMenu.instance)
                {
                    SettingsMenu.instance.FadeInAudio(transitionDuration);
                }
            }
        }

        SceneManager.LoadScene(buildIndex);
    }
}
