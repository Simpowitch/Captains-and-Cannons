using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Data-container of stats of captain

[System.Serializable]
public class CaptainData : CharacterData
{
    [Range(0, 1)] public float moraleDMGIncreaseModifier = 0.25f;
    public string MoraleDMGIncreaseModifierString
    {
        get => Mathf.RoundToInt(moraleDMGIncreaseModifier * 100).ToString() + "%";
    }

    [Range(0, 1)] public float moraleDMGDecreaseModifier = 0.25f;
    public string MoraleDMGDecreaseModifierString
    {
        get => Mathf.RoundToInt(moraleDMGDecreaseModifier * 100).ToString() + "%";

    }

    public const float damageAgainstCaptainModifier = 1;
    public static string DamageAgainstCaptainModifierString
    {
        get => Mathf.RoundToInt(damageAgainstCaptainModifier * 100).ToString() + "%";
    }

    public const float reductionPerAliveCharacterOnTeam = 0.25f;


    public CaptainData(CaptainData clone)
    {
        moraleDMGIncreaseModifier = clone.moraleDMGIncreaseModifier;
        moraleDMGDecreaseModifier = clone.moraleDMGDecreaseModifier;

        //Base
        characterName = clone.characterName;
        prefabObject = clone.prefabObject;
        defaultCharacterImage = clone.defaultCharacterImage;
        specialAbilityIcon = clone.specialAbilityIcon;
        recruitmentLine = clone.recruitmentLine;

        customWantedPoster = clone.customWantedPoster;

        backstory = clone.backstory;

        maxHP = clone.maxHP;
        currentHP = clone.currentHP;

        level = 1;
        levelUPXPReq = CharacterData.firstLevelUpReq;
        xp = 0;

        meleeMinDMG = clone.meleeMinDMG;
        meleeMaxDMG = clone.meleeMaxDMG;

        rangedMinDMG = clone.rangedMinDMG;
        rangedMaxDMG = clone.rangedMaxDMG;

        critChance = clone.critChance;
    }

    float moraleBoostLVLUp = 0.05f;
    float moraleDecreaseLVLUP = 0.05f;
    public override void LevelUp(LevelUpChoice choice)
    {
        base.LevelUp(choice);
        moraleDMGIncreaseModifier += moraleBoostLVLUp;
        moraleDMGDecreaseModifier += moraleDecreaseLVLUP;
    }

    public override string GetCharacterSpecifics()
    {
        string info = "Passive: Boosts friendly characters on tiles adjacent to this character. Their attacks do +" + MoraleDMGIncreaseModifierString + " damage, and they take -" + MoraleDMGDecreaseModifierString + " damage.";
        info += "\nPassive: If this captain attacks another captain the attack deals +" + DamageAgainstCaptainModifierString + " damage.";
        info += "\nSpecial Ability: Gives another matey an additional AP (action point)";
        return info;
    }

    public override int GetScore()
    {
        int baseScore = base.GetScore();

        //Multiply aura-buffs to base score
        float score = (1 + moraleDMGIncreaseModifier + moraleDMGDecreaseModifier) *baseScore;

        return Mathf.RoundToInt(score);
    }
}

