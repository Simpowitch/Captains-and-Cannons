using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Simon Voss
//This script will increase stats of a player session active character

public class LevelUpButton : MonoBehaviour
{
    [SerializeField]
    Text levelUpDescription = null;

    [SerializeField] int characterIndex = 0;

    [SerializeField] CharacterData.LevelUpChoice choice = CharacterData.LevelUpChoice.HP;

    CharacterData myCharacter;


    private void OnEnable()
    {
        myCharacter = PlayerSession.instance.GetCharacter(characterIndex);
        HideLevelUpDescription();
    }

    public void ShowLevelUpDescription()
    {
        levelUpDescription.text = myCharacter.GetStatUpDescription(choice);
    }

    public void HideLevelUpDescription()
    {
        levelUpDescription.text = "Choose a skill above to level up";
    }

    public void LevelUp()
    {
        myCharacter.LevelUp(choice);
        CharacterInformationDisplay.OnCharacterStatChange(myCharacter, characterIndex);
        CharacterInformationDisplay.OnLevelUp();
    }
}
