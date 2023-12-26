using UnityEngine;
using UnityEngine.UI;

//Simon Voss
//Responsible for the Shop UI

public class ShopDisplay : MonoBehaviour
{
    #region Singleton
    public static ShopDisplay instance;
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

    [SerializeField] GameObject shopObject = null;
    [SerializeField] ItemDisplay[] itemDisplays = null;
    [SerializeField] GameObject[] itemObjects = null;
    [SerializeField] Text itemName = null;
    [SerializeField] Text description = null;
    [SerializeField] Text buyText = null;
    [SerializeField] Button buyButton = null;

    ShopEvent activeEvent;
    Item itemSelectedForPurchase;
    int selectedIndex;

    public void StartNewEvent(ShopEvent shopEvent)
    {
        activeEvent = shopEvent;

        shopObject.SetActive(true);

        DisplaySelectedItem(null);

        for (int i = 0; i < itemDisplays.Length; i++)
        {
            itemObjects[i].SetActive(false);
        }

        for (int i = 0; i < shopEvent.availableItems.Length; i++)
        {
            if (itemDisplays.Length > i)
            {
                itemObjects[i].SetActive(true);
                itemDisplays[i].UpdateItem(shopEvent.availableItems[i]);
            }
        }
    }

    public void SelectItem(int index)
    {
        selectedIndex = index;
    }

    public void DisplaySelectedItem(Item item)
    {
        if (item == null)
        {
            itemName.text = "";
            description.text = "";
            buyText.text = "No item selected";
            buyButton.interactable = false;
        }
        else
        {
            itemSelectedForPurchase = item;

            itemName.text = item.itemName;
            description.text = item.description;
            buyText.text = "Cost: " + item.itemValue.ToString();

            if (item.itemValue <= PlayerSession.instance.Dubloons)
            {
                if (PlayerSession.instance.GetNumberOfItemsInInventory() < PlayerSession.instance.inventoryLimit)
                {
                    buyButton.interactable = true;
                }
                else
                {
                    buyButton.interactable = false;
                    buyText.text += "\nInventory full";
                }
            }
            else
            {
                buyButton.interactable = false;
                buyText.text += "\nNot enough dubloons";
            }
        }
    }

    public void BuyItem()
    {
        if (itemSelectedForPurchase)
        {
            if (PlayerSession.instance.SpendDubloons(itemSelectedForPurchase.itemValue))
            {
                PlayerSession.instance.AddItem(itemSelectedForPurchase);
                itemObjects[selectedIndex].SetActive(false);
                DisplaySelectedItem(null);
                Debug.Log("Item bought");
            }
        }
        else
        {
            Debug.Log("No item selected");
        }
    }
}
