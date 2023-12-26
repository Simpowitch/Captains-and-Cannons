using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//Simon Voss

    //A basic cannon that can fire upon the enemy ship

public class Cannon : MonoBehaviour
{
    [SerializeField] SpriteRenderer mySpriteRenderer = null;
    [SerializeField] Sprite shootSprite = null;
    [SerializeField] Sprite idleSprite = null;
    [SerializeField] Animator myAnimator = null;
    [SerializeField] Text damageText = null;

    [SerializeField] Team team = Team.Player;
    public Team MyTeam
    {
        get => team;
    }

    [SerializeField]
    int cannonDamage = 10;

    float damageRandomModifier = 0.25f;

    public void ShootCannon(float damageModifier)
    {
        myAnimator.SetTrigger("Shoot");
        StartCoroutine(CannonSpriteChange());
        Team enemy = MyTeam == Team.Player ? Team.AI : Team.Player;
        int damageToDeal = GetDamageToDeal(damageModifier, DamageRandomModifier.Random);
        Debug.Log("Cannon dealt " + damageToDeal.ToString() + " points of damage to the opposing team ship");
        CombatManager.instance.ships[(int)enemy].TakeDamage(damageToDeal);
        damageText.text = damageToDeal.ToString();
    }


    public int GetDamageToDeal(float damageModifier, DamageRandomModifier damageRandomState)
    {
        float damageToDeal = cannonDamage;
        damageToDeal = Mathf.RoundToInt(cannonDamage * damageModifier);

        switch (damageRandomState)
        {
            case DamageRandomModifier.Min:
                damageToDeal *= (1 - damageRandomModifier);
                break;
            case DamageRandomModifier.Max:
                damageToDeal *= (1 + damageRandomModifier);
                break;
            case DamageRandomModifier.Random:
                damageToDeal *= Random.Range((1 - damageRandomModifier), (1 + damageRandomModifier));
                break;
        }
        return Mathf.RoundToInt(damageToDeal);
    }

    public IEnumerator CannonSpriteChange()
    {
        mySpriteRenderer.sprite = shootSprite;
        CombatAudio.instance.PlayAudio(CombatAudio.ActionAudio.Cannon);
        yield return new WaitForSeconds(0.5f);
        CombatAudio.instance.PlayAudio(CombatAudio.ActionAudio.ShipImpact);
        mySpriteRenderer.sprite = idleSprite;
    }
}
