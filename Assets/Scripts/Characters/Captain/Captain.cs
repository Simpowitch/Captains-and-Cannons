using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//The most important character-class. Versitile in both ranged and melee combat. This character can aid other characters by boosting them in different ways

public enum MoralebonusType { DMGIncrease, DMGReduction }

public class Captain : Character
{
    [HideInInspector]
    public CaptainData characterData;
    public override CharacterData GetCharacterData()
    {
        return characterData;
    }
    public override void SetCharacterData(CharacterData newCharacterdata)
    {
        if (newCharacterdata is CaptainData)
        {
            characterData = newCharacterdata as CaptainData;
        }
        else
        {
            Debug.LogError("Wrong character type");
        }
    }

    protected override void Start()
    {
        base.Start();
        className = "Captain";

        UpdateOverHeadShields(null);

        CombatDelegates.instance.OnCharacterDied += UpdateOverHeadShields;
    }

    private void UpdateOverHeadShields(Character character)
    {
        int charactersOnMyTeam = CombatManager.instance.GetAliveCharacters(MyTeam).Count - 1; //Number of characters - 1
        overHeadInfo.ShowCharacterShields(charactersOnMyTeam);
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
                toolTip = "Give a matey another ACTION POINT. \nCooldown: "
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
                toolTip = "Give 1 actionpoint to this character";
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
                GiveActionPointToOther(CombatManager.instance.TargetCharacter);
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


    public void GiveActionPointToOther(Character target)
    {
        if (specialAbilityCooldownTimer <= 0)
        {
            if (HasActionPoints(specialAbilityAPCost))
            {
                target.AddActionPoints(1);
                specialAbilityCooldownTimer = specialAbilityCooldown + 1;
                SpendActionPoint(specialAbilityAPCost);
                CombatDelegates.instance.OnAnimationFinished += DeselectCharacterAfterAnimation; //Deselect after played animation
                ActionAnimation.instance.PlayAnimation(this, target, 1, ActionAnimation.AnimationState.SpecialFriendly, specialAbilityAudio, false);
            }
        }
    }

    private void RangedAttack(Character target)
    {
        if (HasActionPoints(1))
        {
            int damageToDeal = GetDamageToDeal(characterData.rangedMinDMG, characterData.rangedMaxDMG, AttackType.Ranged, true, target, out bool iscrit);
            target.QueueDamage(damageToDeal);
            SpendAllActionPoints();
            CombatDelegates.instance.OnAnimationFinished += DeselectCharacterAfterAnimation; //Deselect after played animation
            ActionAnimation.instance.PlayAnimation(this, target, damageToDeal, ActionAnimation.AnimationState.Ranged, rangedAttackAudio, iscrit);
        }
    }


    protected override List<float> GetIncomingDamageModifiers(AttackType attackType, ref List<string> explanations)
    {
        List<float> modifiers = new List<float>();
        modifiers.AddRange(base.GetIncomingDamageModifiers(attackType, ref explanations));

        int charactersOnMyTeam = CombatManager.instance.GetAliveCharacters(MyTeam).Count - 1; //Number of characters except one (this captain)
        float damageNegateMultiplier = CaptainData.reductionPerAliveCharacterOnTeam * charactersOnMyTeam;
        Mathf.Min(1, damageNegateMultiplier); //Don't go above 100%

        if (charactersOnMyTeam > 0)
        {
            modifiers.Add(-damageNegateMultiplier);
            explanations.Add("Other alive crew members: " + charactersOnMyTeam + " = -" + (damageNegateMultiplier * 100) + "% ");
        }
        return modifiers;
    }

    protected override List<float> GetOutgoingDamageModifiers(AttackType attackType, ref List<string> explanations, Character target)
    {
        List<float> modifiers = new List<float>();
        modifiers.AddRange(base.GetOutgoingDamageModifiers(attackType, ref explanations, target));

        //Captain VS Captain Damage boost
        if (target is Captain)
        {
            modifiers.Add(CaptainData.damageAgainstCaptainModifier);
            explanations.Add("Captain VS Captain bonus: +" + CaptainData.DamageAgainstCaptainModifierString);
        }
        return modifiers;
    }

    protected override void Die()
    {
        base.Die();
        CombatManager.instance.EndBattleEvent(this, null);
    }
}
