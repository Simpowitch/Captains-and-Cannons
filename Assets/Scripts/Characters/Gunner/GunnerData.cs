using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Data-container of stats of gunner

[System.Serializable]
public class GunnerData : CharacterData
{
    public int specialAbilityMinDamage = 0;
    public int specialAbilityMaxDamage = 0;

    public const float cannonDamageIncrease = 0.5f;
    public static string CannonDamageIncreaseString
    {
        get => Mathf.RoundToInt(cannonDamageIncrease * 100).ToString() + "%";
    }

    public const float meleeDamageVulnerability = 0.3f;
    public static string MeleeDamageVulnerabilityString
    {
        get => Mathf.RoundToInt(meleeDamageVulnerability * 100).ToString() + "%";
    }

    public GunnerData(GunnerData clone)
    {
        specialAbilityMinDamage = clone.specialAbilityMinDamage;
        specialAbilityMaxDamage = clone.specialAbilityMaxDamage;

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

    int specialAbilityMinDamageUp = 3;
    int specialAbilityMaxDamageUp = 5;

    public override void LevelUp(LevelUpChoice choice)
    {
        base.LevelUp(choice);
        specialAbilityMinDamage += specialAbilityMinDamageUp;
        specialAbilityMaxDamage += specialAbilityMaxDamageUp;
    }

    public override string GetCharacterSpecifics()
    {
        string info = "Passive: Receives + " + MeleeDamageVulnerabilityString + " damage on melee range.";
        info += "\nPassive: Increases cannon damage by " + CannonDamageIncreaseString + " when fired by this character.";
        info += "\nSpecial Ability: Load the gun with scraps and deal extra damage. \nExpected damage: " + specialAbilityMinDamage.ToString() + "-" + specialAbilityMaxDamage.ToString();
        return info;
    }

    public override int GetScore()
    {
        int baseScore = base.GetScore();

        //Add Heavy attack to base score
        float score = baseScore + (specialAbilityMinDamage + specialAbilityMaxDamage) / 2;

        return Mathf.RoundToInt(score);
    }
}

