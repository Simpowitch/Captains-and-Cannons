using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBook : MonoBehaviour
{
   
    [SerializeField] List<ItemDisplay> displayedItems = new List<ItemDisplay>();
    [SerializeField] List<Item> debugAddItems = null;
    [SerializeField] GameObject displayItemPrefab = null;

    Item selectedItem;
    public Text ItemNameText;
    public Text descriptionText;
    public delegate void DescriptionUpdater(Item item);
    DescriptionUpdater descriptionUpdater;

    void Awake()
    {
        UpdateItems();
        descriptionUpdater = new DescriptionUpdater(UpdateDescription);
        PlayerSession.OnSessionStatsChanged += UpdateItems;
    }
    private void OnDestroy()
    {
        PlayerSession.OnSessionStatsChanged -= UpdateItems;
    }

    public void UpdateItems()
    {
        if (PlayerSession.instance.itemInventory.Count > displayedItems.Count)
        {
            while (PlayerSession.instance.itemInventory.Count != displayedItems.Count)
            {
                displayedItems.Add(Instantiate(displayItemPrefab.gameObject, transform).GetComponent<ItemDisplay>());
            }
        }
        else if (PlayerSession.instance.itemInventory.Count < displayedItems.Count)
        {
            for (int i = 0; i < displayedItems.Count - PlayerSession.instance.itemInventory.Count; i++)
            {
                Destroy(displayedItems[0].gameObject);
                displayedItems.RemoveAt(0);
            }
        }
        for (int i = 0; i < PlayerSession.instance.itemInventory.Count; i++)
        {
            displayedItems[i].UpdateItem(PlayerSession.instance.itemInventory[i]);
        }
    }

    //debug method
    public void AddItem()
    {
        print("added");
        PlayerSession.instance.AddItem(debugAddItems[Random.Range(0, debugAddItems.Count)]);
        UpdateItems();
    }
    //debugmethod end

    private void UpdateDescription(Item item)
    {
        ItemNameText.text = item.itemName;
        descriptionText.text = item.description;
    }


    public void SelectItem(Item selectItem)
    {
        selectedItem = selectItem;
        descriptionUpdater(selectedItem);
    }
}
