using UnityEngine;

[CreateAssetMenu(fileName = "New Item Effect", menuName = "Item/Item Effect")]
public class ItemEffect : ScriptableObject
{ 
    [SerializeField] int effectValue = 0;
    public void Effect(Character target)
    {
        target.CurrentHP = Mathf.Clamp(target.CurrentHP + effectValue, target.CurrentHP, target.MAXHP);
        Debug.Log("Healed " + effectValue);
    }
}