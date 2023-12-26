using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Scriptable object for creating a combat encounter from inside unity. Tool for designers

 
[CreateAssetMenu(fileName = "New Combat Encounter", menuName = "Event/Combat Encounter")]
public class CombatEvent : ScriptableObject
{
    public BossIntroBlueprint bossIntro;
    public CharacterBlueprint[] characterBlueprints;
    public Sprite shipSprite;
    public Sprite shipRailing;
    public int shipHP = 100;

    const float xpOfScore = 1.8f;
    const float dubloonsOfScore = 0.4f;

    public int Score
    {
        get
        {
            int score = 0;
            foreach (var item in characterBlueprints)
            {
                score += item.GetBlueprintData().GetScore();
            }
            return score;
        }
    }

    public int GetXPPerCharacter(int numOfCharacters)
    {
        return Mathf.RoundToInt(Score * xpOfScore / numOfCharacters);
    }

    public int Dubloons
    {
        get => Mathf.RoundToInt(Score * dubloonsOfScore);
    }

    public bool isFinalBoss = false;
}
