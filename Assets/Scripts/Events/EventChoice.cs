using UnityEngine;
using System.Collections.Generic;

//Simon Voss
//A choice done in the event system in the map. These classes handles what happens when the player uses this choice

[System.Serializable]
public class EventChoice
{
    [Header("Before choice is made")]
    public string choicePreDescription = "";
    public string GetChoiceInformation(out bool canBeUsed)
    {
        //If any consequence is not possible
        foreach (var item in choiceConsequences)
        {
            if (!item.IsEffectPossible(out string explanation))
            {
                canBeUsed = false;
                return choicePreDescription + "\n" + explanation;
            }
        }
        canBeUsed = true;
        return choicePreDescription;
    }

    [Header("After choice is made")]
    [TextArea(1, 5)]
    public string choicePostDescription;
    public ChoiceConsequence[] choiceConsequences;

    public void UseChoice()
    {
        for (int i = 0; i < choiceConsequences.Length; i++)
        {
            choiceConsequences[i].DoEffect();
        }
        EventDisplay.instance.DisplayChoice(choicePostDescription);
    }
}

[System.Serializable]
public class ChoiceConsequence
{
    public enum Consequence
    {
        None,
        GiveXP,
        GiveHealth,
        RemoveHealth,
        GiveItem,
        RemoveItem,
        GiveGold,
        RemoveGold,
        GiveCharacter,
        GiveRandomCharacter
    }

    public enum Target
    {
        None,
        Captain,
        OnlyCrew,
        AllCharacters,
        Ship,
    }

    public Consequence typeOfConsequence;
    public Target target;

    public int xp;
    public int gold;
    public int hp;
    public Item item;
    public CharacterBlueprint character;

    public List<CharacterBlueprint> randomCharacterList = new List<CharacterBlueprint>();

    public void DoEffect()
    {
        switch (typeOfConsequence)
        {
            case Consequence.None:
                break;
            case Consequence.GiveXP:
                GiveExperience(target);
                break;
            case Consequence.GiveHealth:
                GiveHealth(target);
                break;
            case Consequence.RemoveHealth:
                RemoveHealth(target);
                break;
            case Consequence.GiveItem:
                GiveItem();
                break;
            case Consequence.RemoveItem:
                RemoveItem();
                break;
            case Consequence.GiveGold:
                GiveGold();
                break;
            case Consequence.RemoveGold:
                RemoveGold();
                break;
            case Consequence.GiveCharacter:
                GiveCharacter();
                break;
            case Consequence.GiveRandomCharacter:
                GiveRandomCharacter();
                break;
        }
    }

    public bool IsEffectPossible(out string explanation)
    {
        switch (typeOfConsequence)
        {
            case Consequence.None:
                explanation = "";
                return true;
            case Consequence.GiveXP:
                switch (target)
                {
                    case Target.None:
                    case Target.Ship:
                        Debug.LogWarning("Not correct target " + target.ToString());
                        explanation = "NOT SET UP CORRECTLY";
                        return false;
                    case Target.Captain:
                    case Target.AllCharacters:
                        explanation = "";
                        return true;
                    case Target.OnlyCrew:
                        //IF WE HAVE A CREW
                        explanation = PlayerSession.instance.GetNumberOfAliveCharacters() > 1 ? "" : "No crewmembers beside the captain";
                        return (PlayerSession.instance.GetNumberOfAliveCharacters() > 1);
                }
                break;
            case Consequence.GiveHealth:
                switch (target)
                {
                    case Target.None:
                        Debug.LogWarning("Not correct target " + target.ToString());
                        explanation = "NOT SET UP CORRECTLY";
                        return false;
                    case Target.Ship:
                    case Target.Captain:
                    case Target.AllCharacters:
                        explanation = "";
                        return true;
                    case Target.OnlyCrew:
                        //IF WE HAVE A CREW
                        explanation = PlayerSession.instance.GetNumberOfAliveCharacters() > 1 ? "" : "No crewmembers beside the captain";
                        return (PlayerSession.instance.GetNumberOfAliveCharacters() > 1);
                }
                break;
            case Consequence.RemoveHealth:
                switch (target)
                {
                    case Target.None:
                        Debug.LogWarning("Not correct target " + target.ToString());
                        explanation = "NOT SET UP CORRECTLY";
                        return false;
                    case Target.Ship:
                    case Target.Captain:
                    case Target.AllCharacters:
                        explanation = "";
                        return true;
                    case Target.OnlyCrew:
                        //IF WE HAVE A CREW
                        explanation = PlayerSession.instance.GetNumberOfAliveCharacters() > 1 ? "" : "No crewmembers beside the captain";
                        return PlayerSession.instance.GetNumberOfAliveCharacters() > 1;
                }
                break;
            case Consequence.GiveItem:
                switch (target)
                {
                    case Target.None:
                        explanation = PlayerSession.instance.GetNumberOfItemsInInventory() < PlayerSession.instance.inventoryLimit ? "" : "Inventory full";
                        return PlayerSession.instance.GetNumberOfItemsInInventory() < PlayerSession.instance.inventoryLimit; //If we have inventory space left
                    case Target.Ship:
                    case Target.Captain:
                    case Target.AllCharacters:
                    case Target.OnlyCrew:
                        Debug.LogWarning("Not correct target " + target.ToString());
                        explanation = "NOT SET UP CORRECTLY";
                        return false;
                }
                break;
            case Consequence.RemoveItem:
                switch (target)
                {
                    case Target.None:
                        explanation = PlayerSession.instance.ItemExistsInInventory(item) ? "" : "You do not have the right item for this";
                        return PlayerSession.instance.ItemExistsInInventory(item);
                    case Target.Ship:
                    case Target.Captain:
                    case Target.AllCharacters:
                    case Target.OnlyCrew:
                        Debug.LogWarning("Not correct target " + target.ToString());
                        explanation = "NOT SET UP CORRECTLY";
                        return false;
                }
                break;
            case Consequence.GiveGold:
                switch (target)
                {
                    case Target.None:
                        explanation = "";
                        return true;
                    case Target.Ship:
                    case Target.Captain:
                    case Target.AllCharacters:
                    case Target.OnlyCrew:
                        Debug.LogWarning("Not correct target " + target.ToString());
                        explanation = "NOT SET UP CORRECTLY";
                        return false;
                }
                break;
            case Consequence.RemoveGold:
                switch (target)
                {
                    case Target.None:
                        explanation = PlayerSession.instance.Dubloons >= gold ? "" : "Not enough gold";
                        return PlayerSession.instance.Dubloons >= gold; //If we have the gold needed
                    case Target.Ship:
                    case Target.Captain:
                    case Target.AllCharacters:
                    case Target.OnlyCrew:
                        Debug.LogWarning("Not correct target " + target.ToString());
                        explanation = "NOT SET UP CORRECTLY";
                        return false;
                }
                break;
            case Consequence.GiveCharacter:
                switch (target)
                {
                    case Target.None:
                        bool uniqueCharacterFound = false;
                        if (PlayerSession.instance.CheckIfCharacterIsNew(character.GetCharacterData()))
                        {
                            uniqueCharacterFound = true;
                        }
                        if (uniqueCharacterFound)
                        {
                            explanation = PlayerSession.instance.GetNumberOfAliveCharacters() < PlayerSession.instance.characterLimit ? "" : "Crew full";
                            return PlayerSession.instance.GetNumberOfAliveCharacters() < PlayerSession.instance.characterLimit;
                        }
                        else
                        {
                            explanation = "No character available";
                            return false;
                        }
                    case Target.Ship:
                    case Target.Captain:
                    case Target.AllCharacters:
                    case Target.OnlyCrew:
                        Debug.LogWarning("Not correct target " + target.ToString());
                        explanation = "NOT SET UP CORRECTLY";
                        return false;
                }
                break;
            case Consequence.GiveRandomCharacter:
                switch (target)
                {
                    case Target.None:
                        bool uniqueCharacterFound = false;
                        foreach (var item in randomCharacterList)
                        {
                            if (PlayerSession.instance.CheckIfCharacterIsNew(item.GetCharacterData()))
                            {
                                uniqueCharacterFound = true;
                                break;
                            }
                        }

                        if (uniqueCharacterFound)
                        {
                            explanation = PlayerSession.instance.GetNumberOfAliveCharacters() < PlayerSession.instance.characterLimit ? "" : "Crew full";
                            return PlayerSession.instance.GetNumberOfAliveCharacters() < PlayerSession.instance.characterLimit;
                        }
                        else
                        {
                            explanation = "No character available";
                            return false;
                        }
                    case Target.Ship:
                    case Target.Captain:
                    case Target.AllCharacters:
                    case Target.OnlyCrew:
                        Debug.LogWarning("Not correct target " + target.ToString());
                        explanation = "NOT SET UP CORRECTLY";
                        return false;
                }
                break;
            default:
                explanation = "Not set up";
                return false;
        }
        explanation = "Not set up";
        return false;
    }

    public bool IsEffectCorrectlySetUp(out string explanation)
    {
        switch (typeOfConsequence)
        {
            case Consequence.None:
            case Consequence.GiveItem:
            case Consequence.RemoveItem:
            case Consequence.GiveGold:
            case Consequence.RemoveGold:
            case Consequence.GiveCharacter:
            case Consequence.GiveRandomCharacter:
                switch (target)
                {
                    case Target.None:
                        break;
                    case Target.Ship:
                    case Target.Captain:
                    case Target.AllCharacters:
                    case Target.OnlyCrew:
                        explanation = "Invalid target";
                        return false;
                }
                break;
            case Consequence.GiveXP:
                switch (target)
                {
                    case Target.None:
                    case Target.Ship:
                        explanation = "Invalid target";
                        return false;
                    case Target.Captain:
                    case Target.AllCharacters:
                    case Target.OnlyCrew:
                        break;
                }
                break;
            case Consequence.GiveHealth:
            case Consequence.RemoveHealth:
                switch (target)
                {
                    case Target.None:
                        explanation = "Invalid target";
                        return false;
                    case Target.Ship:
                    case Target.Captain:
                    case Target.AllCharacters:
                    case Target.OnlyCrew:
                        break;
                }
                break;
        }

        switch (typeOfConsequence)
        {
            case Consequence.None:
                break;
            case Consequence.GiveXP:
                if (xp <= 0)
                {
                    explanation = "No number set for XP. Accepted values are above 0";
                    return false;
                }
                break;
            case Consequence.GiveHealth:
            case Consequence.RemoveHealth:
                if (hp <= 0)
                {
                    explanation = "No number set for HP. Accepted values are above 0";
                    return false;
                }
                break;
            case Consequence.GiveItem:
            case Consequence.RemoveItem:
                if (item == null)
                {
                    explanation = "No item set";
                    return false;
                }
                break;
            case Consequence.GiveGold:
            case Consequence.RemoveGold:
                if (gold <= 0)
                {
                    explanation = "No number set for Gold. Accepted values are above 0";
                    return false;
                }
                break;
            case Consequence.GiveCharacter:
                if (character == null)
                {
                    explanation = "No character set to be received";
                    return false;
                }
                break;
            case Consequence.GiveRandomCharacter:
                if (randomCharacterList == null || randomCharacterList.Count <= 0)
                {
                    explanation = "No random characters set to be received";
                    return false;
                }
                break;
        }

        //If reached this position without problems
        explanation = "";
        return true;
    }

    //Changes and effects

    private void GiveExperience(Target target)
    {
        List<CharacterData> affectedCharacters = new List<CharacterData>();
        switch (target)
        {
            case Target.None:
                Debug.LogWarning("No target set in event");
                break;
            case Target.Captain:
                affectedCharacters.Add(PlayerSession.instance.GetCharacterDatas(true)[0]);
                break;
            case Target.OnlyCrew:
                for (int i = 1; i < PlayerSession.instance.GetCharacterDatas(true).Count; i++)
                {
                    affectedCharacters.Add(PlayerSession.instance.GetCharacter(i));
                }
                break;
            case Target.AllCharacters:
                for (int i = 0; i < PlayerSession.instance.GetCharacterDatas(true).Count; i++)
                {
                    affectedCharacters.Add(PlayerSession.instance.GetCharacter(i));
                }
                break;
            case Target.Ship:
                Debug.LogWarning("Cannot give XP to ship");
                break;
        }
        foreach (var item in affectedCharacters)
        {
            item.IncreaseXP(xp);
        }
    }

    private void GiveHealth(Target target)
    {
        List<CharacterData> affectedCharacters = new List<CharacterData>();
        switch (target)
        {
            case Target.None:
                Debug.LogWarning("No target set in event");
                break;
            case Target.Captain:
                affectedCharacters.Add(PlayerSession.instance.GetCharacterDatas(true)[0]);
                break;
            case Target.OnlyCrew:
                for (int i = 1; i < PlayerSession.instance.GetCharacterDatas(true).Count; i++)
                {
                    affectedCharacters.Add(PlayerSession.instance.GetCharacter(i));
                }
                break;
            case Target.AllCharacters:
                for (int i = 0; i < PlayerSession.instance.GetCharacterDatas(true).Count; i++)
                {
                    affectedCharacters.Add(PlayerSession.instance.GetCharacter(i));
                }
                break;
            case Target.Ship:
                PlayerSession.instance.ShipCurrentHP += hp;
                break;
        }
        foreach (var item in affectedCharacters)
        {
            item.currentHP += hp;
            item.currentHP = Mathf.Min(item.currentHP, item.maxHP);
        }
    }

    private void RemoveHealth(Target target)
    {
        List<CharacterData> affectedCharacters = new List<CharacterData>();
        switch (target)
        {
            case Target.None:
                Debug.LogWarning("Not implemented");
                break;
            case Target.Captain:
                affectedCharacters.Add(PlayerSession.instance.GetCharacterDatas(true)[0]);
                break;
            case Target.OnlyCrew:
                for (int i = 1; i < PlayerSession.instance.GetCharacterDatas(true).Count; i++)
                {
                    affectedCharacters.Add(PlayerSession.instance.GetCharacter(i));
                }
                break;
            case Target.AllCharacters:
                for (int i = 0; i < PlayerSession.instance.GetCharacterDatas(true).Count; i++)
                {
                    affectedCharacters.Add(PlayerSession.instance.GetCharacter(i));
                }
                break;
            case Target.Ship:
                PlayerSession.instance.ShipCurrentHP -= hp;
                break;
        }
        foreach (var item in affectedCharacters)
        {
            item.currentHP -= hp;
            if (item.currentHP < 1) //Do not kill characters, but set them to 1 hp
            {
                item.currentHP = 1;
            }
        }
    }

    private void GiveItem()
    {
        PlayerSession.instance.AddItem(item);
    }
    private void RemoveItem()
    {
        PlayerSession.instance.RemoveItem(item);
    }

    private void GiveGold()
    {
        PlayerSession.instance.GainDubloons(gold);
    }

    private void RemoveGold()
    {
        PlayerSession.instance.SpendDubloons(gold);
    }

    private void GiveCharacter()
    {
        PlayerSession.instance.AddNewCharacter(character.GetCharacterData());
    }

    private void GiveRandomCharacter()
    {
        List<CharacterData> shuffledPool = new List<CharacterData>();
        foreach (var item in randomCharacterList)
        {
            shuffledPool.Add(item.GetCharacterData());
        }
        shuffledPool = Utility.ShuffleList<CharacterData>(shuffledPool);

        for (int i = 0; i < shuffledPool.Count; i++)
        {
            //Dont allow characters already owned
            if (PlayerSession.instance.CheckIfCharacterIsNew(shuffledPool[i]))
            {
                PlayerSession.instance.AddNewCharacter(shuffledPool[i]);
                break;
            }
        }
    }
}