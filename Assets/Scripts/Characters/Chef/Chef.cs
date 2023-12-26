using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss

//A character class specialized in healing other characters


public class Chef : Character
{
    [HideInInspector]
    public ChefData characterData;
    public override CharacterData GetCharacterData()
    {
        return characterData;
    }
    public override void SetCharacterData(CharacterData newCharacterdata)
    {
        if (newCharacterdata is ChefData)
        {
            characterData = newCharacterdata as ChefData;
        }
        else
        {
            Debug.LogError("Wrong character type");
        }
    }

    protected override void Start()
    {
        base.Start();
        className = "Chef";
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
                toolTip = "Give a hearty some food to regain " + characterData.healing + " health. \nCooldown: "
                + base.specialAbilityCooldown.ToString() + " turns.";
                toolTip += "\nCOST: " + specialAbilityAPCost + " AP(s)";
                break;
            case CombatAction.UseCannon:
                toolTip = "Shoot the cannon to damage the other ship.";
                if (Tile.cannon != null)
                {
                    toolTip += "Expected damage: " + Tile.cannon.GetDamageToDeal(1, DamageRandomModifier.Min) + "-" + Tile.cannon.GetDamageToDeal(1, DamageRandomModifier.Max);
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
                toolTip = "Heal character with " + characterData.healing.ToString() + " points of HP";
                tooltipColor = MouseTooltip.ColorText.FriendlyTarget;
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
                HealOtherCharacter(CombatManager.instance.TargetCharacter);
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


    public void HealOtherCharacter(Character target)
    {
        if (HasActionPoints(specialAbilityAPCost))
        {
            if (specialAbilityCooldownTimer <= 0)
            {
                target.Heal(characterData.healing);
                specialAbilityCooldownTimer = specialAbilityCooldown + 1;
                SpendActionPoint(specialAbilityAPCost);
                CombatDelegates.instance.OnAnimationFinished += DeselectCharacterAfterAnimation; //Deselect after played animation
                ActionAnimation.instance.PlayAnimation(this, target, characterData.healing, ActionAnimation.AnimationState.SpecialFriendly, specialAbilityAudio, false);
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
}
