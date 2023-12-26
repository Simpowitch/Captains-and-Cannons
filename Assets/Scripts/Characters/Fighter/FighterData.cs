using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Data-container of stats of fighter


[System.Serializable]
public class FighterData : CharacterData
{
    [Range(0, 1)] public float activeOutDamageModifier = 0.5f;
    public string ActiveOutDamageModifierString
    {
        get => Mathf.RoundToInt(activeOutDamageModifier * 100).ToString() + "%";
    }

    [Range(0, 1)] public float activeInDamageModifier = 0.25f;
    public string ActiveInDamageModifierString
    {
        get => Mathf.RoundToInt(activeInDamageModifier * 100).ToString() + "%";
    }


    public FighterData(FighterData clone)
    {
        activeOutDamageModifier = clone.activeOutDamageModifier;
        activeInDamageModifier = clone.activeInDamageModifier;

        //Base
        characterName = clone.characterName;
        prefabObject = clone.prefabObject;
        defaultCharacterImage = clone.defaultCharacterImage;
        specialAbilityIcon = clone.specialAbilityIcon;
        backstory = clone.backstory;
        recruitmentLine = clone.recruitmentLine;

        customWantedPoster = clone.customWantedPoster;

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

    float debuffDecrese = 0.02f;
    public override void LevelUp(LevelUpChoice choice)
    {
        base.LevelUp(choice);
        activeInDamageModifier -= debuffDecrese;
        activeInDamageModifier = Mathf.Max(0, activeInDamageModifier); //Never goes below 0 %
    }

    public override string GetCharacterSpecifics()
    {
        string info;
        info = $"Special Ability: Drink some rum. {characterName} will do "
            + ActiveOutDamageModifierString + " more damage and recieve " + ActiveInDamageModifierString + " less damage.";
        return info;
    }

    public override int GetScore()
    {
        int baseScore = base.GetScore();

        //Add Fighter special buff to base score
        float score = (1 + activeOutDamageModifier - activeInDamageModifier) * baseScore;

        return Mathf.RoundToInt(score);
    }
}

