using System.Collections.Generic;
using UnityEngine;

//Simon Voss

//The ship which battles takes place on

public class Ship : MonoBehaviour
{
    public SpriteRenderer spriteRenderer = null;
    public SpriteRenderer railingRenderer = null;
    [SerializeField] OverHeadInfo healthBar = null;
    [SerializeField] List<ShipTile> myTiles = new List<ShipTile>();
    public List<ShipTile> MyTiles { get => myTiles; }

    [SerializeField] Team team = Team.Player;
    public Team Team { get => team; }

    [SerializeField]
    int maxHP = 100;
    public int MaxHp
    {
        get => maxHP;
        set
        {
            maxHP = value;
            healthBar.SetMaxValue(value);
        }
    }

    [SerializeField]
    int currentHP = 100;
    public int CurrentHp
    {
        get => currentHP;
        set
        {
            currentHP = value;
            healthBar.SetCurrentValue(value);
        }
    }

    private void Start()
    {
        healthBar.SetMaxValue(MaxHp);
        healthBar.SetCurrentValue(CurrentHp);
    }

    //Increases the ship HP
    public void Repair(int repairHp)
    {
        CurrentHp += repairHp;
        CurrentHp += Mathf.Min(MaxHp, CurrentHp);
    }

    //Reduce the HP of this ship
    public void TakeDamage(int damageToTake)
    {
        CurrentHp -= Mathf.Max(0, damageToTake);
        if (CurrentHp <= 0)
        {
            SinkShip();
        }
    }

    //Call this if the shall sink/be destroyed
    private void SinkShip()
    {
        Debug.Log("Ship destroyed.");
        CombatManager.instance.EndBattleEvent(null, this);
    }
}
