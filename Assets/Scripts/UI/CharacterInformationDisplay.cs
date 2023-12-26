using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Simon Voss
//Controls an UI panel containing information about the characters in the active player session or from a set array of characters

public class CharacterInformationDisplay : MonoBehaviour
{
    [Header("Origin")]
    [SerializeField] CharacterBlueprint[] characterBlueprints = null;
    [SerializeField] bool usePlayerSession = true;

    [Header("All characters")]
    [SerializeField] GameObject[] characterBoxes = null;

    [SerializeField] Text[] names = null;
    [SerializeField] Text[] levels = null;
    [SerializeField] Image[] portraits = null;
    [SerializeField] Text[] HPs = null;
    [SerializeField] Text[] meleeDamages = null;
    [SerializeField] Text[] rangedDamages = null;
    [SerializeField] Text[] crits = null;
    [SerializeField] OverHeadInfo[] xpDisplays = null;
    [SerializeField] Image[] wantedPosters = null;

    [Header("Single choice")]
    [SerializeField] Text loreField = null;
    [SerializeField] Text specialAbilityDesciption = null;
    [SerializeField] Image specialAbilityIcon = null;
    [SerializeField] Button walkThePlankButton = null;
    [SerializeField] Text walkThePlankText = null;

    [Header("Level Up buttons and texts")]
    [SerializeField] Button winScreenContinueButton = null;
    [SerializeField] GameObject[] levelUpObjects = null;
    [SerializeField] GameObject[] rangedLevelUpButtons = null;
    [SerializeField] AudioSource voicelineAudioSource = null;
    [SerializeField] GameObject levelUpDescriptionPanel = null;

    public delegate void StatUpdate(CharacterData character, int index);
    public static StatUpdate OnCharacterStatChange;
    public delegate void StatsUpdate();
    public static StatsUpdate OnCharactersStatsChanged;
    public static StatsUpdate OnLevelUp;


    private void Start()
    {
        Subscribe(true);

        ResetDisplay();
    }

    private void OnEnable()
    {
        ResetDisplay();
    }

    private void ResetDisplay()
    {
        if (specialAbilityIcon)
        {
            specialAbilityIcon.enabled = false;
        }
        if (loreField != null)
        {
            loreField.text = "";
        }
        if (specialAbilityDesciption != null)
        {
            specialAbilityDesciption.text = "";
        }

        foreach (var item in characterBoxes)
        {
            item.SetActive(false);
        }

        ShowAllCharactersStats();
        CheckLevelUpDone();
        selectedCharacterForWalkThePlank = null;
        if (walkThePlankText != null)
        {
            walkThePlankText.text = "";
        }
        if (walkThePlankButton != null)
        {
            walkThePlankButton.enabled = false;
            walkThePlankButton.image.enabled = false;
        }
    }

    public void ShowAllCharactersStats()
    {
        List<CharacterData> characters;
        if (usePlayerSession)
        {
            characters = PlayerSession.instance.GetCharacterDatas(true);
        }
        else
        {
            characters = new List<CharacterData>();
            foreach (var item in characterBlueprints)
            {
                characters.Add(item.GetCharacterData());
            }
        }
        for (int i = 0; i < characters.Count; i++)
        {
            ShowCharacterStats(characters[i], i);
        }
    }

    private void CheckLevelUpDone()
    {
        List<CharacterData> characters;
        if (usePlayerSession)
        {
            characters = PlayerSession.instance.GetCharacterDatas(true);
        }
        else
        {
            characters = new List<CharacterData>();
            foreach (var item in characterBlueprints)
            {
                characters.Add(item.GetCharacterData());
            }
        }
        int charactersReadyForLevelUp = 0;
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].IsReadyForLevelUp)
            {
                charactersReadyForLevelUp++;
            }
        }
        if (winScreenContinueButton != null)
        {
            winScreenContinueButton.interactable = charactersReadyForLevelUp == 0;
            levelUpDescriptionPanel.SetActive(charactersReadyForLevelUp != 0);
        }
    }

    private void ShowCharacterStats(CharacterData character, int index)
    {
        characterBoxes[index].SetActive(true);

        //Background/Wantedposter
        if (wantedPosters.Length > index && wantedPosters[index] != null)
        {
            if (character.customWantedPoster != null)
            {
                wantedPosters[index].sprite = character.customWantedPoster;
            }
        }

        if (names.Length > index && names[index] != null)
        {
            names[index].text = character.characterName;
        }
        if (levels.Length > index && levels[index] != null)
        {
            levels[index].text = character.Level.ToString();
        }
        if (portraits.Length > index && portraits[index] != null)
        {
            portraits[index].sprite = character.defaultCharacterImage;
        }
        if (HPs.Length > index && HPs[index] != null)
        {
            HPs[index].text = character.currentHP.ToString() + "/" + character.maxHP.ToString();
        }
        if (meleeDamages.Length > index && meleeDamages[index] != null)
        {
            meleeDamages[index].text = character.meleeMinDMG.ToString() + "-" + character.meleeMaxDMG.ToString();
        }
        if (rangedDamages.Length > index && rangedDamages[index] != null)
        {
            rangedDamages[index].text = character.rangedMinDMG.ToString() + "-" + character.rangedMaxDMG.ToString();
        }
        if (crits.Length >= index && crits[index] != null)
        {
            crits[index].text = Mathf.RoundToInt(character.critChance * 100).ToString() + "%";
        }


        //Level Up & XP
        if (levelUpObjects.Length != 0 && levelUpObjects.Length > index && levelUpObjects[index] != null)
        {
            levelUpObjects[index].SetActive(character.IsReadyForLevelUp);
        }


        //Dont show ranged levelup buttons for fighters (they have terrible aim at a distance...)
        if (rangedLevelUpButtons != null && rangedLevelUpButtons.Length > index && rangedLevelUpButtons[index] != null)
        {
            if (!(character is FighterData)) //Fighters can't level up ranged
            {
                rangedLevelUpButtons[index].SetActive(character.IsReadyForLevelUp);
            }
            else
            {
                rangedLevelUpButtons[index].SetActive(false);
            }
        }
        if (xpDisplays.Length > 0 && xpDisplays.Length > index && xpDisplays[index] != null)
        {
            xpDisplays[index].SetMaxValue(character.LevelUPXPReq);
            xpDisplays[index].SetCurrentValue(character.XP);
        }
    }

    public void ShowExtraInfo(int characterIndex)
    {
        ShowLoreFromSession(characterIndex);
        ShowSpecialAbility(characterIndex);
        ShowWalkThePlankButton(characterIndex);
        PlayVoiceline(characterIndex);
    }

    private void ShowLoreFromSession(int characterIndex)
    {
        CharacterData character = PlayerSession.instance.GetCharacter(characterIndex);
        loreField.text = character.backstory;
    }
    private void PlayVoiceline(int characterIndex)
    {
        voicelineAudioSource.clip = PlayerSession.instance.GetCharacter(characterIndex).recruitmentLine;
        voicelineAudioSource.Play();
    }

    private void ShowSpecialAbility(int characterIndex)
    {
        specialAbilityIcon.enabled = true;
        CharacterData character = PlayerSession.instance.GetCharacter(characterIndex);
        specialAbilityDesciption.text = character.GetCharacterSpecifics();
        specialAbilityIcon.sprite = character.specialAbilityIcon;
    }

    //Walk the plank (remove character from crew)
    CharacterData selectedCharacterForWalkThePlank;
    private void ShowWalkThePlankButton(int characterIndex)
    {
        if (walkThePlankButton != null && walkThePlankText != null)
        {
            CharacterData character = PlayerSession.instance.GetCharacter(characterIndex);
            walkThePlankButton.interactable = !(character is CaptainData);
            walkThePlankButton.enabled = true;
            walkThePlankButton.image.enabled = true;

            walkThePlankText.text = walkThePlankButton.interactable ? "Walk the plank!" : "Cannot perform mutiny on yourself!";

            selectedCharacterForWalkThePlank = character;
        }
    }
    public void WalkThePlank()
    {
        PlayerSession.instance.WalkThePlank(selectedCharacterForWalkThePlank);
        ResetDisplay();
    }

    private void Subscribe(bool status)
    {
        if (status)
        {
            OnCharacterStatChange += ShowCharacterStats;
            OnCharactersStatsChanged += ShowAllCharactersStats;
            OnLevelUp += CheckLevelUpDone;
        }
        else
        {
            OnCharacterStatChange -= ShowCharacterStats;
            OnCharactersStatsChanged -= ShowAllCharactersStats;
            OnLevelUp -= CheckLevelUpDone;
        }
    }

    private void OnDestroy()
    {
        Subscribe(false);
    }
}
