using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Max & Simon
//This script handles user interface on events

public class EventDisplay : MonoBehaviour
{
    #region Singleton
    public static EventDisplay instance;
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

    [SerializeField] GameObject eventCanvas = null;

    [SerializeField] Text eventName = null;
    [SerializeField] Text descriptionText = null;
    [SerializeField] Image eventImage = null;

    [SerializeField] GameObject[] choiceButtonObjects = new GameObject[4];
    [SerializeField] Text[] buttonTexts = new Text[4];
    [SerializeField] Button[] choiceButtons = new Button[4];

    [SerializeField] Button closeButton = null;

    //Holding the event choices start method
    public delegate void ResponseMethod();
    public static ResponseMethod[] OnResponseGiven = new ResponseMethod[4];

    //Starts an event
    public void StartNewEvent(NonCombatEvent newEvent)
    {
        if (closeButton != null)
        {
            closeButton.interactable = false;
        }
        eventCanvas.SetActive(true);
        for (int i = 0; i < OnResponseGiven.Length; i++)
        {
            if (i < choiceButtonObjects.Length)
            {
                choiceButtonObjects[i].SetActive(OnResponseGiven[i] != null);
                if (choiceButtonObjects[i].GetComponent<Button>() != null)
                {
                    choiceButtonObjects[i].GetComponent<Button>().interactable = true; //Resetting when a new event is starting
                }
                if (OnResponseGiven[i] != null)
                {
                    if (buttonTexts[i] != null)
                    {
                        buttonTexts[i].text = newEvent.eventChoices[i].GetChoiceInformation(out bool interactable);
                        choiceButtons[i].interactable = interactable;
                    }
                }
            }
        }

        if (eventName)
        {
            eventName.text = newEvent.title;
        }
        if (descriptionText)
        {
            descriptionText.text = newEvent.eventDescription;
        }

        if (eventImage != null)
        {
            eventImage.sprite = newEvent.image;
        }
    }

    //Send from a button with options on the event
    public void OnPress(int index)
    {
        OnResponseGiven[index]?.Invoke();
        for (int i = 0; i < OnResponseGiven.Length; i++)
        {
            OnResponseGiven[i] = null;
        }
        for (int i = 0; i < choiceButtonObjects.Length; i++)
        {
            if (choiceButtonObjects[i].GetComponent<Button>() != null)
            {
                choiceButtonObjects[i].GetComponent<Button>().interactable = false;
            }
        }

        if (closeButton != null)
        {
            closeButton.interactable = true;
        }
    }

    public void DisplayChoice(string postChoiceText)
    {
        descriptionText.text = postChoiceText;
    }

    //Closes the windows of this eventhandler
    public void EndEvent()
    {
        //Close the window
        Debug.Log("Event ended");
        eventCanvas.SetActive(false);

        for (int i = 0; i < OnResponseGiven.Length; i++)
        {
            OnResponseGiven[i] = null;
        }
    }
}
