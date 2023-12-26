using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsCombatDisplay : MonoBehaviour
{

    [SerializeField] Animator myAnimator = null;
    bool isDisplayOpen;

    const float ITEMOFFSET = -144f, CELLSIZE = 90;

    List<ItemDisplay> displayedItems = new List<ItemDisplay>();
    [SerializeField] GameObject displayItemPrefab = null;



    void Start()
    {
        isDisplayOpen = false;
        PlayerSession.OnSessionStatsChanged += UpdateItems;
        foreach (Transform child in transform)
        {
            displayedItems.Add(child.GetComponent<ItemDisplay>());
        }
        UpdateItems();
    }
    private void OnDestroy()
    {
        PlayerSession.OnSessionStatsChanged -= UpdateItems;
    }
    public void UpdateItems()
    {
        if (PlayerSession.instance.itemInventory.Count > displayedItems.Count)
        {
            while(PlayerSession.instance.itemInventory.Count != displayedItems.Count)
            {
                displayedItems.Add(Instantiate(displayItemPrefab.gameObject, transform).GetComponent<ItemDisplay>());
            }
        }
        else if(PlayerSession.instance.itemInventory.Count < displayedItems.Count)
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


    #region Scroll Items
    

    public void DisplayRight()
    {
        SetDisplayPosition(-CELLSIZE);
    }
    public void DisplayLeft()
    {
       
        SetDisplayPosition(CELLSIZE);
    }

    public void SetDisplayPosition(float newXPos)
    {
        transform.localPosition += new Vector3(newXPos, 0, 0);
        transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -CELLSIZE * (displayedItems.Count - 4) + ITEMOFFSET, ITEMOFFSET),0,0);
    }
    #endregion
    #region Animation
    public void ToggleItemDisplay()
    {
        isDisplayOpen = !isDisplayOpen;
        myAnimator.SetBool("Open", isDisplayOpen);
    }
    #endregion
}
