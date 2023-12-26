using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Simon Voss
//Button that activates selected character actions

[RequireComponent(typeof(Button))]
public class ActionButton : MonoBehaviour
{
    [SerializeField] CombatAction action = CombatAction.MeleeAttack;

    [Header("Helpfulness to user")]
    [SerializeField] KeyCode hotkey = KeyCode.Space;
    [SerializeField] string hotkeyExplanation = "";
    [SerializeField] Tooltip tooltip = null;

    Button button;
    Image iconRenderer;


    [Header("Special ability")]
    [SerializeField] Sprite captainSpecialAbilityIcon = null;
    [SerializeField] Sprite gunnerSpecialAbilityIcon = null;
    [SerializeField] Sprite fighterSpecialAbilityIcon = null;
    [SerializeField] Sprite chefSpecialAbilityIcon = null;

    [Header("Cooldown visuals")]
    [SerializeField] Image cdImage = null;
    [SerializeField] Text cdText = null;

    [Header("Cannon button")]
    [SerializeField] Animator cannonButtonAnimator = null;


    private void Awake()
    {
        button = GetComponent<Button>();
        iconRenderer = GetComponent<Image>();
    }

    private void Start()
    {
        if (action == CombatAction.SpecialAbility)
        {
            CombatDelegates.instance.OnSelectedCharacter += NewSelectedCharacter;
            NewSelectedCharacter(CombatManager.instance.SelectedCharacter); //Load a sprite instantly when object is made active
        }
        CombatDelegates.instance.OnCombatHotkeyPress += DetectHotkey;
    }

    private void DetectHotkey()
    {
        if (Input.GetKeyDown(hotkey) && button.IsInteractable())
        {
            DoButtonClick();
            button.Select();
        }
    }

    private void NewSelectedCharacter(Character character)
    {
        if (character is Captain)
        {
            iconRenderer.sprite = captainSpecialAbilityIcon;
        }
        else if (character is Gunner)
        {
            iconRenderer.sprite = gunnerSpecialAbilityIcon;
        }
        else if (character is Fighter)
        {
            iconRenderer.sprite = fighterSpecialAbilityIcon;
        }
        else if (character is Chef)
        {
            iconRenderer.sprite = chefSpecialAbilityIcon;
        }
        if (character != null)
        {
            CheckCooldown(character);
        }
    }

    private void CheckCooldown(Character character)
    {
        cdImage.enabled = character.SpecialAbilityCooldownTimer > 0;
        cdText.text = character.SpecialAbilityCooldownTimer > 0 ? character.SpecialAbilityCooldownTimer.ToString() : "";
        cdImage.fillAmount = (float) character.SpecialAbilityCooldownTimer / character.SpecialAbilityCooldown;
    }

    public void SetNotInteractable()
    {
        button.interactable = false;
        //Special animation for cannon-button
        if (action == CombatAction.UseCannon)
        {
            cannonButtonAnimator.SetBool("Show", false);
        }
    }

    public void CheckIfInteractable()
    {
        Character character = CombatManager.instance.SelectedCharacter;
        if (character != null)
        {
            button.interactable = (character.IsActionPossible(action, out string explanation));
        }
        else
        {
            button.interactable = false;
        }
        //Special animation for cannon-button
        if (action == CombatAction.UseCannon)
        {
            cannonButtonAnimator.SetBool("Show", button.IsInteractable());
        }
    }

    public void DoButtonClick()
    {
        CombatManager.instance.SetPendingAction(action);
    }

    public void ShowToolTip()
    {
        if (CombatManager.instance.SelectedCharacter != null)
        {
            string message = CombatManager.instance.SelectedCharacter.GetButtonTooltip(action);
            message += "\nHotkey: (" + hotkeyExplanation + ")";
            tooltip.SetUp(message);
        }
        else
        {
            tooltip.SetUp("No character selected (Click on a character on your turn to display available actions here)");
        }
    }

    public void HideToolTip()
    {
        tooltip.HideTooltip();
    }
}
