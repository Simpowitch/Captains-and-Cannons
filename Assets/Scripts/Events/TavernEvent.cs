using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Blueprint for a tavern event, possible characters there and their prices

[CreateAssetMenu(fileName = "New Tavern Event", menuName = "Event/Tavern")]
public class TavernEvent : ScriptableObject
{
    public CharacterPurchase[] characterPurchases = null;
    public int minNumberOfCharacters = 1;
    public int maxNumberOfCharacters = 3;

    public bool isShipyard = false;

    public List<CharacterData> RandomizeCharacters(out List<int> prices)
    {
        List<CharacterData> charactersInTavern = new List<CharacterData>();
        prices = new List<int>();

        //Add pool to list and shuffle it
        List<CharacterPurchase> shuffledPool = new List<CharacterPurchase>();
        foreach (var item in characterPurchases)
        {
            shuffledPool.Add(item);
        }

        //Number of characters we want to find
        int numberOfPossiblePurchases = Random.Range(minNumberOfCharacters, maxNumberOfCharacters + 1);
        numberOfPossiblePurchases = Mathf.Min(characterPurchases.Length, numberOfPossiblePurchases);

        for (int i = 0; i < numberOfPossiblePurchases; i++)
        {
            shuffledPool = Utility.ShuffleList<CharacterPurchase>(shuffledPool); //Shuffle order
            bool characterFound = false;
            float rng = Random.Range(0f, 1f);

            if (rng <= CharacterPurchase.legendaryProbability)
            {
                for (int j = 0; j < shuffledPool.Count; j++)
                {
                    if (shuffledPool[j].rarity == CharacterPurchase.Rarity.Legendary)
                    {
                        CharacterData character = shuffledPool[j].characterForSale.GetCharacterData();

                        //Dont allow characters which has been owned by the player session (dead or alive) to be bought again
                        if (PlayerSession.instance.CheckIfCharacterIsNew(character))
                        {
                            charactersInTavern.Add(shuffledPool[j].characterForSale.GetCharacterData());
                            prices.Add(shuffledPool[j].cost);
                            shuffledPool.RemoveAt(j);
                            characterFound = true;
                            break;
                        }
                    }
                }
            }

            if (characterFound)
            {
                continue;
            }

            if (rng <= CharacterPurchase.legendaryProbability + CharacterPurchase.rareProbability)
            {
                for (int j = 0; j < shuffledPool.Count; j++)
                {
                    if (shuffledPool[j].rarity == CharacterPurchase.Rarity.Rare)
                    {
                        CharacterData character = shuffledPool[j].characterForSale.GetCharacterData();

                        //Dont allow characters which has been owned by the player session (dead or alive) to be bought again
                        if (PlayerSession.instance.CheckIfCharacterIsNew(character))
                        {
                            charactersInTavern.Add(character);
                            prices.Add(shuffledPool[j].cost);
                            shuffledPool.RemoveAt(j);
                            characterFound = true;
                            break;
                        }
                    }
                }
            }

            if (characterFound)
            {
                continue;
            }

            if (rng <= CharacterPurchase.legendaryProbability + CharacterPurchase.rareProbability + CharacterPurchase.uncommonProbability)
            {
                for (int j = 0; j < shuffledPool.Count; j++)
                {
                    if (shuffledPool[j].rarity == CharacterPurchase.Rarity.Uncommon)
                    {
                        CharacterData character = shuffledPool[j].characterForSale.GetCharacterData();

                        //Dont allow characters which has been owned by the player session (dead or alive) to be bought again
                        if (PlayerSession.instance.CheckIfCharacterIsNew(character))
                        {
                            charactersInTavern.Add(character);
                            prices.Add(shuffledPool[j].cost);
                            shuffledPool.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
            else //If no character found before
            {
                for (int j = 0; j < shuffledPool.Count; j++)
                {
                    if (shuffledPool[j].rarity == CharacterPurchase.Rarity.Common)
                    {
                        CharacterData character = shuffledPool[j].characterForSale.GetCharacterData();

                        //Dont allow characters which has been owned by the player session (dead or alive) to be bought again
                        if (PlayerSession.instance.CheckIfCharacterIsNew(character))
                        {
                            charactersInTavern.Add(character);
                            prices.Add(shuffledPool[j].cost);
                            shuffledPool.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
        }

        return charactersInTavern;
    }
}

[System.Serializable]
public class CharacterPurchase
{
    public enum Rarity { Common, Uncommon, Rare, Legendary }
    public static readonly float uncommonProbability = 0.3f;
    public static readonly float rareProbability = 0.15f;
    public static readonly float legendaryProbability = 0.05f;

    public CharacterBlueprint characterForSale;
    public int cost;
    public Rarity rarity;
}
