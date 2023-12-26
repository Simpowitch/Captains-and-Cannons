using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Data-container of stats of chef

[System.Serializable]
public class ChefData : CharacterData
{
    public int healing;

    public ChefData(ChefData clone)
    {
        healing = clone.healing;

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

    int healUp = 2;
    public override void LevelUp(LevelUpChoice choice)
    {
        base.LevelUp(choice);
        healing += healUp;
    }

    public override string GetCharacterSpecifics()
    {
        string info = "Special Ability: Heal another character for " + healing.ToString() + " points.";
        return info;
    }

    public override int GetScore()
    {
        int baseScore = base.GetScore();

        //Add healing to base score (healing is worth 2 points per heal)
        return ( healing * 2 ) + baseScore;
    }
}
