using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public int itemValue;
    public Sprite itemSprite;
    public string description;
    public enum TargetType { SingleFriendly, SingleEnemy, GroupFriendly, GroupEnemy, GroupAll, SingleAny }
    public TargetType myTargetType;
    [SerializeField] ItemEffect myEffect = null;


    public void SelectItem()
    {
        Debug.Log($"Used {name}");
        CombatManager.instance.SelectedItem = this;
    }
    public void ApplyEffect(Character target)
    {
        myEffect.Effect(target);
    }

    public void RemoveItemFromPlayer()
    {
        PlayerSession.instance.RemoveItem(this);
    }
}