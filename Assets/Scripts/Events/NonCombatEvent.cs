using UnityEngine;

//Simon Voss
//An event on the map that does not result in a combat. Will prompt the user with options of which he/she will have to make a choice

[CreateAssetMenu(fileName = "New Noncombat Event", menuName = "Event/Non Combat Event")]
public class NonCombatEvent : ScriptableObject
{
    public string title;
    [TextArea(1, 5)]
    public string eventDescription;
    public Sprite image;
    public EventChoice[] eventChoices = null;

    public void SendMyEvent()
    {
        for (int i = 0; i < eventChoices.Length; i++)
        {
            EventDisplay.OnResponseGiven[i] = eventChoices[i].UseChoice;
        }
        EventDisplay.instance.StartNewEvent(this);
    }
}