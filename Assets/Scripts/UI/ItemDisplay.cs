using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    public enum DisplayType { ItemBook, Shop, Combat, Show}
    public DisplayType myDisplayType;

    [SerializeField] Item myItem;
    [SerializeField] Image myImage = null;
    ItemBook myBook;
    public Text myPriceTag;

    private void Awake()
    {
        if (myDisplayType == DisplayType.ItemBook)
        {
            myBook = transform.parent.GetComponent<ItemBook>();
        }
    }
    public void UpdateItem(Item item)
    {
        myItem = item;
        myImage.sprite = item.itemSprite;
        if (myDisplayType == DisplayType.Shop)
        {
            myPriceTag.text = item.itemValue.ToString();
        }
    }
    public void DisableDisplay()
    {
        myImage.enabled = false;
        if (myDisplayType == DisplayType.Shop)
        {
            myPriceTag.text = "";
        }
    }
    public void ClickItem()
    {
        switch (myDisplayType)
        {
            case DisplayType.ItemBook:
                myBook.SelectItem(myItem);
                break;
            case DisplayType.Shop:
                ShopDisplay.instance.DisplaySelectedItem(myItem);
                break;
            case DisplayType.Combat:
                switch (myItem.myTargetType)
                {
                    case Item.TargetType.SingleFriendly:
                        myItem.SelectItem();
                        break;
                    case Item.TargetType.SingleEnemy:
                        myItem.SelectItem();
                        break;
                    case Item.TargetType.GroupFriendly:
                        foreach (Character character in CombatManager.instance.GetAliveCharacters(Team.Player))
                        {
                            myItem.ApplyEffect(character);
                        }
                        PlayerSession.instance.RemoveItem(myItem);
                        break;
                    case Item.TargetType.GroupEnemy:
                        foreach (Character character in CombatManager.instance.GetAliveCharacters(Team.AI))
                        {
                            myItem.ApplyEffect(character);
                        }
                        PlayerSession.instance.RemoveItem(myItem);
                        break;
                    case Item.TargetType.GroupAll:
                        foreach (Character character in CombatManager.instance.GetAliveCharacters(Team.AI))
                        {
                            myItem.ApplyEffect(character);
                        }
                        foreach (Character character in CombatManager.instance.GetAliveCharacters(Team.Player))
                        {
                            myItem.ApplyEffect(character);

                        }
                        PlayerSession.instance.RemoveItem(myItem);
                        break;
                    case Item.TargetType.SingleAny:
                        myItem.SelectItem();
                        break;
                    default:
                        break;
                }
                
              
                break;
            case DisplayType.Show:
                break;
            default:
                break;
        }
    }
    public void ShowToolTip()
    {
        MouseTooltip.SetUpToolTip(MouseTooltip.ColorText.Default, myItem.description);
    }

    public void HideToolTip()
    {
        MouseTooltip.HideTooltip();
    }
}
