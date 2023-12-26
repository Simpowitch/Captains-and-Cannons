using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Simon Voss
//Controls the cutscene and provides ability to skip it, also loads the next scene and readies it for the player whenever the player chooses to change

public class CutScene : MonoBehaviour
{
    [SerializeField] Image timerCircle = null;
    [SerializeField] float timeToHoldButton = 2;
    float holdTimer = 0;
    [SerializeField] AudioSource soundPlayer = null;

    [SerializeField] SceneTransition sceneTransition = null;

    private void Start()
    {
        timerCircle.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            soundPlayer.Play();
        }

        if (Input.anyKey)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= timeToHoldButton)
            {
                holdTimer = float.MinValue;
                ChangeScene();
            }
        }
        else
        {
            holdTimer = 0;
        }

        UpdateHoldVisual(holdTimer);
    }

    private void UpdateHoldVisual(float value)
    {
        timerCircle.fillAmount = value / timeToHoldButton;
    }

    private void ChangeScene()
    {
        sceneTransition.ChangeScene();
    }
}
