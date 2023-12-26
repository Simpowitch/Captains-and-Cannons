using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss

//A character class specialized in ranged combat and cannon usage
public class Gunner : Character
{
    [HideInInspector]
    public GunnerData characterData;
    public override CharacterData GetCharacterData()
    {
        return characterData;
    }
    public override void SetCharacterData(CharacterData newCharacterdata)
    {
        if (newCharacterdata is GunnerData)
        {
            characterData = newCharacterdata as GunnerData;
        }
        else
        {
            Debug.LogError("Wrong character type");
        }
    }

    protected override void Start()
    {
        base.Start();
        className = "Gunner";
    }

    public override string GetButtonTooltip(CombatAction action)
    {
        string toolTip = "";
        switch (action)
        {
            case CombatAction.Move:
                toolTip = "Move character to another tile.";
                toolTip += "\nCOST: 1 AP/tile";
                break;
            case CombatAction.MeleeAttack:
                toolTip = "Perform a melee attack on an adjacent enemy target.";
                toolTip += "\nCOST: all remaining APs";
                break;
            case CombatAction.RangedAttack:
                toolTip = "Perform a ranged attack on an enemy target.";
                toolTip += "\nCOST: all remaining APs";
                break;
            case CombatAction.SpecialAbility:
                toolTip = "Load the gun with scraps and deal extra damage. \nCooldown: "
                + base.specialAbilityCooldown.ToString() + " turns.";
                toolTip += "\nCOST: " + specialAbilityAPCost + " AP(s)";
                break;
            case CombatAction.UseCannon:
                toolTip = "Shoot the cannon to damage the other ship.";
                if (Tile.cannon != null)
                {
                    toolTip += "Expected damage: " + Tile.cannon.GetDamageToDeal(1 + GunnerData.cannonDamageIncrease, DamageRandomModifier.Min) + "-" + Tile.cannon.GetDamageToDeal(1 + GunnerData.cannonDamageIncrease, DamageRandomModifier.Max);
                    toolTip += "\nDamage increased by +" + GunnerData.CannonDamageIncreaseString + " from character passive ability.";
                }
                toolTip += "\nInstant Action";
                toolTip += "\nCOST: all remaining APs";
                break;
            case CombatAction.None:
            default:
                toolTip = "NOT SET UP";
                break;
        }

        if (!IsActionPossible(action, out string explanation))
        {
            toolTip += "\n<color=red>Action not available: " + explanation + "</color>";
        }
        return toolTip;
    }

    public override string GetCharacterHooverActionTooltip(CombatAction action, out MouseTooltip.ColorText tooltipColor, Character mouseOverCharacter)
    {
        string toolTip = "";
        tooltipColor = MouseTooltip.ColorText.Default;
        List<string> modifierExplanations = new List<string>();
        int[] damageRange;
        switch (action)
        {
            case CombatAction.MeleeAttack:
                damageRange = GetDamageRange(characterData.meleeMinDMG, characterData.meleeMaxDMG, AttackType.Melee, mouseOverCharacter, out modifierExplanations);
                toolTip = "Expected damage is: " + damageRange[0].ToString() + "-" + damageRange[1].ToString();
                tooltipColor = MouseTooltip.ColorText.EnemyTarget;
                break;
            case CombatAction.RangedAttack:
                damageRange = GetDamageRange(characterData.rangedMinDMG, characterData.rangedMaxDMG, AttackType.Ranged, mouseOverCharacter, out modifierExplanations);
                toolTip = "Expected damage is: " + damageRange[0].ToString() + "-" + damageRange[1].ToString();
                tooltipColor = MouseTooltip.ColorText.EnemyTarget;
                break;
            case CombatAction.SpecialAbility:
                damageRange = GetDamageRange(characterData.specialAbilityMinDamage, characterData.specialAbilityMaxDamage, AttackType.Ranged, mouseOverCharacter, out modifierExplanations);
                toolTip = "Expected damage is: " + damageRange[0].ToString() + "-" + damageRange[1].ToString();
                tooltipColor = MouseTooltip.ColorText.EnemyTarget;
                break;
            case CombatAction.Move:
            case CombatAction.UseCannon:
            case CombatAction.None:
            default:
                break;
        }
        foreach (var item in modifierExplanations)
        {
            toolTip += "\n";
            toolTip += item;
        }
        return toolTip;
    }

    public override void UseAction(CombatAction action)
    {
        switch (action)
        {
            case CombatAction.MeleeAttack:
                MeleeAttack(CombatManager.instance.TargetCharacter);
                break;
            case CombatAction.RangedAttack:
                RangedAttack(CombatManager.instance.TargetCharacter);
                break;
            case CombatAction.SpecialAbility:
                ShotgunBlast(CombatManager.instance.TargetCharacter);
                break;
            case CombatAction.UseCannon:
                UseCannon();
                break;
            default:
                Debug.LogError(action.ToString() + " not se up in " + this.name);
                break;
        }
        PreparedAction = CombatAction.None;

    }

    public void ShotgunBlast(Character target)
    {
        if (specialAbilityCooldownTimer <= 0)
        {
            if (HasActionPoints(specialAbilityAPCost))
            {
                int damageToDeal = GetDamageToDeal(characterData.specialAbilityMinDamage, characterData.specialAbilityMaxDamage, AttackType.Ranged, true, target, out bool isCrit);
                target.QueueDamage(damageToDeal);
                specialAbilityCooldownTimer = specialAbilityCooldown + 1;
                SpendActionPoint(specialAbilityAPCost);
                CombatDelegates.instance.OnAnimationFinished += DeselectCharacterAfterAnimation; //Deselect after played animation
                ActionAnimation.instance.PlayAnimation(this, target, damageToDeal, ActionAnimation.AnimationState.SpecialEnemy, specialAbilityAudio, isCrit);
            }
        }
    }

    public void RangedAttack(Character target)
    {
        if (HasActionPoints(1))
        {
            int damageToDeal = GetDamageToDeal(characterData.rangedMinDMG, characterData.rangedMaxDMG, AttackType.Ranged, true, target, out bool isCrit);
            target.QueueDamage(damageToDeal);
            SpendAllActionPoints();
            CombatDelegates.instance.OnAnimationFinished += DeselectCharacterAfterAnimation; //Deselect after played animation
            ActionAnimation.instance.PlayAnimation(this, target, damageToDeal, ActionAnimation.AnimationState.Ranged, rangedAttackAudio, isCrit);
        }
    }

    protected override List<float> GetIncomingDamageModifiers(AttackType attackType, ref List<string> explanations)
    {
        List<float> modifiers = new List<float>();
        modifiers.AddRange(base.GetIncomingDamageModifiers(attackType, ref explanations));
        //Vulnerable to melee attacks
        if (attackType == AttackType.Melee)
        {
            modifiers.Add(GunnerData.meleeDamageVulnerability);
            explanations.Add("Vulnerable to melee attacks: +" + GunnerData.MeleeDamageVulnerabilityString);
        }
        return modifiers;
    }

    //Modified by gunners cannon damage modifier
    protected override void UseCannon()
    {
        if (HasActionPoints(1))
        {
            Tile.cannon.ShootCannon(1 + GunnerData.cannonDamageIncrease); //100% + increase
            //Display shooting animation
            SpendAllActionPoints();
        }
    }
}
