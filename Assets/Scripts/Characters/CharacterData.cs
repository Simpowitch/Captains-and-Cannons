using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Abstract class of character stats in a data container

[System.Serializable]
public abstract class CharacterData
{
    public string characterName;
    public GameObject prefabObject;
    public Sprite defaultCharacterImage;
    public Sprite specialAbilityIcon;
    public AudioClip recruitmentLine;

    public Sprite customWantedPoster;

    [TextArea(1, 4)]
    public string backstory;

    public int maxHP;
    public int currentHP;

    public static int firstLevelUpReq = 100;
    protected int level = 1;
    public int Level
    {
        get => level;
    }
    protected int levelUPXPReq = 100;
    public int LevelUPXPReq
    {
        get => levelUPXPReq;
    }
    protected int xp = 0;
    public int XP
    {
        get => xp;
    }

    public int meleeMinDMG;
    public int meleeMaxDMG;

    public int rangedMinDMG;
    public int rangedMaxDMG;

    [Range(0, 1)] public float critChance;

    #region LevelUP
    public enum LevelUpChoice { HP, Melee, Ranged, Crit }
    float levelUpXPRequirementIncrease = 1.2f;

    int hpUP = 12;
    int meleeMinUp = 3;
    int meleeMaxUp = 4;

    int rangedMinUp = 2;
    int rangedMaxUp = 4;

    float critUp = 0.1f; // * 100 = percentage

    /// <summary>
    /// Returns true when character levels up
    /// </summary>
    /// <param name="change"></param>
    /// <returns></returns>
    public bool IncreaseXP(int xpToAdd)
    {
        xp += xpToAdd;

        return IsReadyForLevelUp;
    }

    public bool IsReadyForLevelUp
    {
        get
        {
            return xp >= levelUPXPReq;
        }
    }

    public string GetStatUpDescription(LevelUpChoice choiceOfInterest)
    {
        switch (choiceOfInterest)
        {
            case LevelUpChoice.HP:
                return "Increase MAX HP with: " + hpUP + " and restore health to full";
            case LevelUpChoice.Melee:
                return "Increase melee damage. Min: + " + meleeMinUp + ", Max: + " + meleeMaxUp;
            case LevelUpChoice.Ranged:
                return "Increase ranged damage. Min: + " + rangedMinUp + " Max: + " + rangedMaxUp;
            case LevelUpChoice.Crit:
                return "Increase critical strike chance with: " + Mathf.RoundToInt(critUp * 100) + "%";
            default:
                Debug.LogWarning("Not supported");
                return "";
        }
    }

    public virtual void LevelUp(LevelUpChoice choice)
    {
        if (!IsReadyForLevelUp)
        {
            Debug.Log("Not ready for level up");
            return;
        }
        switch (choice)
        {
            case LevelUpChoice.HP:
                maxHP += hpUP;
                currentHP = maxHP;
                break;
            case LevelUpChoice.Melee:
                meleeMinDMG += meleeMinUp;
                meleeMaxDMG += meleeMaxUp;
                break;
            case LevelUpChoice.Ranged:
                rangedMinDMG += rangedMinUp;
                rangedMaxDMG += rangedMaxUp;
                break;
            case LevelUpChoice.Crit:
                critChance += critUp;
                break;
        }

        level++;
        xp -= levelUPXPReq;
        levelUPXPReq = (int)(levelUPXPReq * levelUpXPRequirementIncrease);
    }
    #endregion

    public abstract string GetCharacterSpecifics();

    /// <summary>
    /// Used directly from blueprints to score battle events
    /// </summary>
    public virtual int GetScore()
    {
        float score = ((float)maxHP / 2); //HP only gives half points to avoid scoring to favor high HP too much
        if (currentHP !=  maxHP) //If wrongfully set up. (blueprints should always have the same currentHP as maxHP in blueprints)
        {
            score -= 1000;
        }
        score += (float) (meleeMinDMG + meleeMaxDMG) / 2;
        score += (float) (rangedMinDMG + rangedMaxDMG) / 2;
        score *= (1f + critChance);
        return Mathf.RoundToInt(score);
    }
}

