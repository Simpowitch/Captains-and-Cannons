using UnityEngine;

//Simon Voss
//An event where we get to buy items

[CreateAssetMenu(fileName = "New Shop Event", menuName = "Event/Shop")]
public class ShopEvent : ScriptableObject
{
    public Item[] availableItems = null;

    public void StartMyEvent()
    {
        ShopDisplay.instance.StartNewEvent(this);
    }
}
