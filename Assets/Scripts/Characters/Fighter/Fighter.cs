using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss

//A character class specialized in close melee combat

public class Fighter : Character
{
    [HideInInspector]
    public FighterData characterData;
    public override CharacterData GetCharacterData()
    {
        return characterData;
    }
    public override void SetCharacterData(CharacterData newCharacterdata)
    {
        if (newCharacterdata is FighterData)
        {
            characterData = newCharacterdata as FighterData;
        }
        else
        {
            Debug.LogError("Wrong character type");
        }
    }

    protected override void Start()
    {
        base.Start();
        className = "Fighter";
    }

    int specialAbilityDuration = 2;
    int specialAbilityDurationTimer = 0;

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
                toolTip = $"Drink some rum. {characterData.characterName} will do "
            + characterData.ActiveOutDamageModifierString + " more damage and recieve " + characterData.ActiveInDamageModifierString + " more damage. \nCooldown: " +
                base.specialAbilityCooldown.ToString() + " turns.";
                toolTip += "\nInstant Action";
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
            case CombatAction.SpecialAbility:
                toolTip = "Drink rum to increase stats";
                tooltipColor = MouseTooltip.ColorText.FriendlyTarget;
                break;
            case CombatAction.RangedAttack:
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

    public override CombatAction PreparedAction
    {
        get => base.PreparedAction;
        set
        {
            if (value == CombatAction.SpecialAbility)
            {
                UseAction(value);
            }
            base.PreparedAction = value;
        }
    }

    public override void UseAction(CombatAction action)
    {
        switch (action)
        {
            case CombatAction.MeleeAttack:
                MeleeAttack(CombatManager.instance.TargetCharacter);
                break;
            case CombatAction.SpecialAbility:
                DrinkRum();
                break;
            case CombatAction.UseCannon:
                UseCannon();
                break;
            case CombatAction.RangedAttack:
            default:
                Debug.LogError(action.ToString() + " not se up in " + this.name);
                break;
        }
        PreparedAction = CombatAction.None;
    }

    private void DrinkRum()
    {
        if (specialAbilityCooldownTimer <= 0)
        {
            if (HasActionPoints(specialAbilityAPCost))
            {
                specialAbilityCooldownTimer = specialAbilityCooldown + 1;
                specialAbilityDurationTimer = specialAbilityDuration + 1;
                SpendActionPoint(specialAbilityAPCost);
                if (ActionPoints <= 0)
                {
                    CombatDelegates.instance.OnAnimationFinished += DeselectCharacterAfterAnimation; //Deselect after played animation
                }
                ActionAnimation.instance.PlayAnimation(this, this, 0, ActionAnimation.AnimationState.SpecialSelf, specialAbilityAudio, false);
            }
        }
    }

    public override void NewRound(CombatManager.State gameState)
    {
        if (IsMyTurn(gameState) && Alive)
        {
            specialAbilityDurationTimer--;
            specialAbilityDurationTimer = Mathf.Max(0, specialAbilityDurationTimer);
            base.NewRound(gameState);
            AddActionPoints(1); //Fighter has bonus actionpoint
        }
    }

    public override void GetDamageStats(out int minDMGMelee, out int maxDMGMelee, out int minDMGRanged, out int maxDMGRanged, out bool hasBuff)
    {
        hasBuff = GetCaptainBonus(MoralebonusType.DMGIncrease, out float captainBonus, out string captainBonusString);

        float specialAbilityModifier = 0;
        if (specialAbilityDurationTimer > 0)
        {
            specialAbilityModifier += characterData.activeOutDamageModifier;
            hasBuff = true;
        }

        minDMGMelee = Mathf.RoundToInt(GetCharacterData().meleeMinDMG * (1 + captainBonus + specialAbilityModifier));
        maxDMGMelee = Mathf.RoundToInt(GetCharacterData().meleeMaxDMG * (1 + captainBonus + specialAbilityModifier));
        minDMGRanged = 0;
        maxDMGRanged = 0;
    }

    protected override List<float> GetIncomingDamageModifiers(AttackType attackType, ref List<string> explanations)
    {
        List<float> modifiers = new List<float>();
        modifiers.AddRange(base.GetIncomingDamageModifiers(attackType, ref explanations));
        //Active special ability
        if (specialAbilityDurationTimer > 0)
        {
            modifiers.Add(characterData.activeInDamageModifier);
            explanations.Add("Special ability active: +" + characterData.ActiveInDamageModifierString);
        }
        return modifiers;
    }

    protected override List<float> GetOutgoingDamageModifiers(AttackType attackType, ref List<string> explanations, Character target)
    {
        List<float> modifiers = new List<float>();
        modifiers.AddRange(base.GetOutgoingDamageModifiers(attackType, ref explanations, target));
        //Active special ability
        if (specialAbilityDurationTimer > 0)
        {
            modifiers.Add(characterData.activeOutDamageModifier);
            explanations.Add("Special ability active: +" + characterData.ActiveOutDamageModifierString);
        }
        return modifiers;
    }
}
