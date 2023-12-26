using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Simon & Gabriel
//Displays the character or entity health visually

public class OverHeadInfo : MonoBehaviour
{
    [SerializeField] Slider slider = null;
    [SerializeField] Animator textAnimator = null;
    [SerializeField] Text changeText = null;
    [SerializeField] Text currentValueText = null;

    [SerializeField] Image[] actionIcons = new Image[3];
    [SerializeField] List<Image> shieldIcons = new List<Image>();

    [SerializeField] bool hideAt0Value = true;
    [SerializeField] bool alwaysAnimateChangeFromBottomUp = false;

    [SerializeField] float animationTime = 1f;

    [SerializeField] bool playSoundAtMax = false;
    [SerializeField] UISFXSender levelUpSound = null;

    //Use this method to set the max value of the slider
    public void SetMaxValue(int value)
    {
        if (value > 0)
        {
            slider.maxValue = value;
        }
        else
        {
            Debug.LogWarning(value + " is not over 0!");
        }
    }
    public void SetActionPoints(int actionPoints, bool playerOwner)
    {
        for (int i = 0; i < actionIcons.Length; i++)
        {
            actionIcons[i].gameObject.SetActive(false);
        }
        if (actionPoints != 0 && playerOwner) //Only show actionpoints for player controlled characters
        {
            for (int i = 0; i < actionPoints; i++)
            {
                if (i < actionIcons.Length)
                {
                    actionIcons[i].gameObject.SetActive(true);
                }
            }
        }
    }

    public void ShowCharacterShields(int numberOfOtherCharacters)
    {
        for (int i = 0; i < shieldIcons.Count; i++)
        {
            shieldIcons[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < numberOfOtherCharacters; i++)
        {
            if (i < shieldIcons.Count)
            {
                shieldIcons[i].gameObject.SetActive(true);
            }
        }
    }

    //Use this one to set the fill of the bar 
    public void SetCurrentValue(int newValue)
    {
        if (this.gameObject.activeInHierarchy)
        {

            if (changeText && textAnimator != null)
            {
                changeText.color = newValue >= slider.value ? Color.green : Color.red;
                changeText.text = "";
                changeText.text += newValue >= slider.value ? "+ " : "- ";
                if (newValue == slider.value)
                {
                    changeText.text = "";
                    changeText.color = Color.black;
                }
                changeText.text += Mathf.RoundToInt(Mathf.Abs(slider.value - newValue));
                if (textAnimator.gameObject.activeInHierarchy)
                {
                    textAnimator.SetTrigger("Play");
                }
            }
            StartCoroutine(ChangeSliderValueOverTime(slider, newValue));
        }
        else
        {
            Debug.LogWarning("Trying to call on an disabled overhead info panel");

            Debug.LogWarning("This was called on" + gameObject.transform + "/" + gameObject.transform.parent + "/" + gameObject.transform.parent.parent);
        }
    }

    IEnumerator ChangeSliderValueOverTime(Slider slider, int targetValue)
    {
        float timer = 0;
        float t = 0;
        int sliderStartValue = alwaysAnimateChangeFromBottomUp ? 0 : Mathf.RoundToInt(slider.value);

        while (timer < this.animationTime)
        {
            timer += Time.deltaTime;
            t = timer / animationTime;
            slider.value = Mathf.Lerp(sliderStartValue, targetValue, t);
            ShowValueInText(slider.value);
            yield return null;
        }
        slider.value = targetValue;
        if (targetValue <= 0 && hideAt0Value)
        {
            transform.parent.gameObject.SetActive(false);
        }
        if (targetValue >= slider.maxValue && playSoundAtMax && levelUpSound != null)
        {
            levelUpSound.PlayCustom();
            Debug.Log("Playing level up sound");
        }

        ShowValueInText(slider.value);
    }

    private void ShowValueInText(float currentValue)
    {
        if (currentValueText != null)
        {
            currentValueText.text = Mathf.RoundToInt(currentValue).ToString() + " / " + Mathf.RoundToInt(slider.maxValue).ToString();
        }
    }
}
