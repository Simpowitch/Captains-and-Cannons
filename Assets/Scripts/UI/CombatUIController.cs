using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Viktor & Simon
//Controls all UI

public class CombatUIController : MonoBehaviour
{
    #region SelectedCharacter
    [Header("Selected character panel")]
    public Text characterNameText;
    public Text hPtext;
    public Text meleeDMGtext;
    public Text rangedDMGtext;
    public Text critText;
    public Text characterClassText;
    public Image characterImage;

    [Header("Damage text color (selected character)")]
    public Color buffedColor = Color.blue;
    public Color defaultColor = Color.black;
    #endregion

    [Header("Buttons")]
    [SerializeField]
    ActionButton[] actionButtons = null;
    [SerializeField] Button endTurnButton = null;

    [SerializeField] Color endTurnDefaultColor = Color.white;
    [SerializeField] Color endTurnDoneColor = Color.green;
    [SerializeField] Color endTurnAIColor = Color.red;

    [SerializeField] Image changeTurnButtonImage = null;


    [Header("Post Combat")]
    [SerializeField] GameObject playerWonScreen = null;
    [SerializeField] Text dubloonWinText = null;
    [SerializeField] Text[] xpIncreaseTexts = null;

    [SerializeField] GameObject gameOverScreen = null;

    [Header("Animators")]
    [SerializeField] Animator selectedCharacterAnim = null;
    [SerializeField] Animator turnPanelAnim = null;
    [SerializeField] Animator turnTextAnim = null;
    [SerializeField] Animator changeTurnAnim = null;
    [SerializeField] Animator itemDisplayAnim = null;


    private void Start()
    {
        CombatDelegates.instance.OnSelectedCharacter += UpdateSelectedCharacterInfo;
        CombatDelegates.instance.OnSelectedCharacter += UpdateActionButtonStatuses;
        CombatDelegates.instance.OnTurnStatusChanged += ChangeTurnUI;
        CombatDelegates.instance.OnCharacterMoved += UpdateActionButtonStatuses;
        CombatDelegates.instance.OnCharacterMoved += UpdateSelectedCharacterInfo;
        CombatDelegates.instance.OnActionPerformed += UpdateSelectedCharacterInfo;
        CombatDelegates.instance.OnActionPerformed += UpdateActionButtonStatuses;
        CombatDelegates.instance.OnActionPerformed += ShowNoMoreActionsAvailable;

        CombatDelegates.instance.OnPlayerWon += ShowPlayerWonUI;
        CombatDelegates.instance.OnPlayerLost += ShowPlayerLostUI;
        CombatDelegates.instance.OnAnimationBegin += HideAllUI;
        CombatDelegates.instance.OnAnimationFinished += ShowDefaultUI;

        ShowDefaultUI();
        ChangeTurnUI(CombatManager.instance.GameState);
    }

    /// <summary>
    /// Updates info about the selected character, displays stats and images in the panel for selected characters
    /// </summary>
    public void UpdateSelectedCharacterInfo(Character character)
    {
        SelectedCharacterUIStatus(character != null);
        if (character != null && CombatManager.instance.GameState == CombatManager.State.PlayerTurn)
        {
            character.GetDamageStats(out int minMeleeDamage, out int maxMeleeDamage, out int minRangedDamage, out int maxRangedDamage, out bool hasBuff);

            Color colorText = hasBuff ? buffedColor : defaultColor;
            meleeDMGtext.color = colorText;
            rangedDMGtext.color = colorText;


            characterNameText.text = character.GetCharacterData().characterName;
            hPtext.text = character.CurrentHP.ToString() + "/" + character.MAXHP.ToString();
            meleeDMGtext.text = minMeleeDamage.ToString() + "-" + maxMeleeDamage.ToString();
            rangedDMGtext.text = minRangedDamage.ToString() + "-" + maxRangedDamage.ToString();
            critText.text = Mathf.RoundToInt(character.GetCharacterData().critChance * 100).ToString() + "%";
            characterClassText.text = character.ClassName;



            characterImage.sprite = character.SpriteHolder.standing;

            //Flip the image if the character is not friendly (inverted)
            characterImage.transform.localScale = new Vector3(character.MyTeam == Team.Player ? 1 : -1, 1, 1);
        }
    }


    private void UpdateActionButtonStatuses(Character character)
    {
        if (character != null && CombatManager.instance.GameState == CombatManager.State.PlayerTurn && character.MyTeam == Team.Player)
        {
            foreach (var item in actionButtons)
            {
                item.CheckIfInteractable();
            }
        }
        else
        {
            foreach (var item in actionButtons)
            {
                item.SetNotInteractable();
            }
        }
    }

    public void SelectedCharacterUIStatus(bool newStatus)
    {
        if (CombatManager.instance.GameState == CombatManager.State.PlayerTurn)
        {
            selectedCharacterAnim.SetBool("Show", newStatus);
        }
        else
        {
            selectedCharacterAnim.SetBool("Show", false);
        }
    }

    private void ChangeTurnUI(CombatManager.State state)
    {
        if (state == CombatManager.State.AITurn)
        {
            endTurnButton.interactable = false;
            turnTextAnim.SetBool("PlayerTurn", false);
            changeTurnAnim.SetBool("PlayerTurn", false);
            changeTurnButtonImage.color = endTurnAIColor;

        }
        else if (state == CombatManager.State.PlayerTurn)
        {
            endTurnButton.interactable = true;
            turnTextAnim.SetBool("PlayerTurn", true);
            changeTurnAnim.SetBool("PlayerTurn", true);
            changeTurnButtonImage.color = endTurnDefaultColor;
        }
    }

    private void ShowPlayerWonUI()
    {
        playerWonScreen.SetActive(true);
        dubloonWinText.text = "+" + PlayerSession.instance.nextEncounter.Dubloons.ToString();
        foreach (var item in xpIncreaseTexts)
        {
            item.text = "+" + PlayerSession.instance.nextEncounter.GetXPPerCharacter(PlayerSession.instance.GetNumberOfAliveCharacters()).ToString() + " XP";
        }
    }

    private void ShowPlayerLostUI()
    {
        gameOverScreen.SetActive(true);
    }

    private void ShowNoMoreActionsAvailable(Character character)
    {
        if (CombatManager.instance.GameState == CombatManager.State.PlayerTurn)
        {
            bool characterWithActionPoint = false;
            List<Character> characters = CombatManager.instance.GetAliveCharacters(Team.Player);
            foreach (var item in characters)
            {
                if (item.ActionPoints > 0)
                {
                    characterWithActionPoint = true;
                    break;
                }
            }

            changeTurnButtonImage.color = characterWithActionPoint ? endTurnDefaultColor : endTurnDoneColor;
        }
    }

    void ShowDefaultUI()
    {
        turnPanelAnim.SetBool("Show", true);
        UpdateSelectedCharacterInfo(CombatManager.instance.SelectedCharacter);
    }

    void HideAllUI()
    {
        turnPanelAnim.SetBool("Show", false);
        selectedCharacterAnim.SetBool("Show", false);
        itemDisplayAnim.SetBool("Open", false);
    }
}
