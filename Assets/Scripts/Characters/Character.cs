using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Simon Voss

//Base class for all character-classes in the game
//Contains methods for using actions, calculating damage, taking damage and moving

public enum Team { Player, AI }
public enum AttackType { Melee, Ranged }
public enum CombatAction { Move, MeleeAttack, RangedAttack, SpecialAbility, UseCannon, None }
public enum DamageRandomModifier { Min, Max, Random }

[RequireComponent(typeof(BoxCollider2D))]
public abstract class Character : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public abstract CharacterData GetCharacterData();
    public abstract void SetCharacterData(CharacterData newCharacterdata);

    protected string className = "";
    public string ClassName
    {
        get => className;
    }

    #region Stats
    //HP & ALIVE
    public int MAXHP
    {
        set
        {
            GetCharacterData().maxHP = value;
            overHeadInfo.SetMaxValue(value);
        }
        get => GetCharacterData().maxHP;
    }

    public int CurrentHP
    {
        set
        {
            if (value > MAXHP)
            {
                GetCharacterData().currentHP = MAXHP;
            }
            else
            {
                GetCharacterData().currentHP = value;
            }
            Debug.Log(GetCharacterData().characterName + " is now at " + CurrentHP.ToString() + " hitpoints");
            if (value <= 0)
            {
                GetCharacterData().currentHP = 0;
                Die();
            }
            overHeadInfo.SetCurrentValue(CurrentHP);
        }
        get => GetCharacterData().currentHP;
    }

    protected bool alive = true;
    public bool Alive
    {
        get => alive;
    }

    //Damage
    const float critModifier = 1f;

    //ABILITIES & ACTIONPOINTS
    [SerializeField] protected int specialAbilityCooldown; public int SpecialAbilityCooldown { get => specialAbilityCooldown; }
    protected int specialAbilityCooldownTimer = 0; public int SpecialAbilityCooldownTimer { get => specialAbilityCooldownTimer; }
    [SerializeField] protected int specialAbilityAPCost = 2;
    readonly int roundStartActionPoints = 2;
    const int maxActionPoints = 4;
    private int actionPoints;
    public int ActionPoints
    {
        get => actionPoints;
        set
        {
            actionPoints = value;
            if (actionPoints > maxActionPoints)
            {
                Debug.LogError("Tried to set actionpoints out of range. Accepted values are within 0-) " + maxActionPoints);
            }
            actionPoints = Mathf.Min(ActionPoints, maxActionPoints);
            overHeadInfo.SetActionPoints(ActionPoints, MyTeam == Team.Player);
        }
    }
    public void AddActionPoints(int pointsToAdd)
    {
        ActionPoints += pointsToAdd;
    }
    #endregion

    #region Logic
    [Header("Logic")]
    ShipTile tile;
    public ShipTile Tile
    {
        set
        {
            tile = value;
            transform.position = Tile.transform.position; //Move character visually
            transform.position += new Vector3(0, 0, -1); //Position in front of all other objects to enable easier clicking
        }
        get => tile;
    }
    [SerializeField] Team team = Team.Player;
    public Team MyTeam
    {
        get => team;
        set => team = value;
    }

    BoxCollider2D collisionBox;
    public void SetColliderStatus(bool input)
    {
        if (collisionBox == null) //If not yet set (for instance at beginning of combat)
        {
            collisionBox = GetComponent<BoxCollider2D>();
        }
        collisionBox.enabled = input;
    }

    public void StartBattle()
    {
        CombatDelegates.instance.OnTurnStatusChanged += this.NewRound;
    }

    #endregion

    #region Graphics
    [Header("Graphics")]
    [SerializeField] SpriteHolder spriteHolder = new SpriteHolder();
    public SpriteHolder SpriteHolder
    {
        get => spriteHolder;
    }

    Animator animator;
    [SerializeField] protected OverHeadInfo overHeadInfo = null;

    //Used to change material to the whole character. For instance when showing the character as selected
    public void ApplyMaterial(Material material)
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().material = material;
        transform.GetChild(1).GetComponent<SpriteRenderer>().material = material;
    }

    #endregion

    #region Audio
    [Header("Audio")]
    [SerializeField] CombatAudio.ActionAudio meleeAttackAudio = CombatAudio.ActionAudio.Bludgeoning;
    [SerializeField] protected CombatAudio.ActionAudio rangedAttackAudio = CombatAudio.ActionAudio.Gun;
    [SerializeField] protected CombatAudio.ActionAudio specialAbilityAudio = CombatAudio.ActionAudio.Heal;
    [SerializeField] CombatAudio.TakeDamageAudio takeDamageAudio = CombatAudio.TakeDamageAudio.HumanMale;
    [SerializeField] CombatAudio.DeathAudio deathAudio = CombatAudio.DeathAudio.HumanMale;
    #endregion

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        collisionBox = GetComponent<BoxCollider2D>();
        overHeadInfo.SetMaxValue(MAXHP);
        overHeadInfo.SetCurrentValue(CurrentHP);
    }

    #region Mouse Inputs
    //Click handler
    public void OnPointerClick(PointerEventData eventData)
    {
        if (CombatManager.instance.GameState == CombatManager.State.PlayerTurn)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    CombatManager.instance.SelectedCharacter = this;
                    CombatAudio.instance.PlayAudio(CombatAudio.ActionAudio.Selection);
                    break;
                case PointerEventData.InputButton.Right:
                    if (CombatManager.instance.SelectedItem) //Have item selected
                    {
                        if (IsValidTarget(CombatManager.instance.SelectedItem, out string explanation))
                        {
                            CombatManager.instance.TargetCharacter = this;
                        }
                        CombatAudio.instance.PlayAudio(CombatAudio.ActionAudio.Heal);
                    }
                    if (CombatManager.instance.SelectedCharacter) //Have another character selected
                    {
                        if (IsValidTarget(CombatManager.instance.SelectedCharacter.preparedAction, out string explanation))
                        {
                            CombatManager.instance.TargetCharacter = this;
                        }
                        CombatAudio.instance.PlayAudio(CombatAudio.ActionAudio.Selection);
                    }
                    break;
                case PointerEventData.InputButton.Middle:
                    break;
            }
        }
        else
        {
            Debug.Log("Not player turn");
        }
    }

    //Display a heads up of what will happen if the player clicks this character
    public void OnPointerEnter(PointerEventData eventData)
    {
        Character selectedCharacter = CombatManager.instance.SelectedCharacter;

        if (CombatManager.instance.GameState == CombatManager.State.PlayerTurn && CombatManager.instance.SelectedItem != null)
        {
            if (IsValidTarget(CombatManager.instance.SelectedItem, out string explanation))
            {
                //Display tooltip UI
                string message = CombatManager.instance.SelectedItem.description;
                if (message != "")
                {
                    MouseTooltip.ColorText textColor;
                    switch (CombatManager.instance.SelectedItem.myTargetType)
                    {
                        case Item.TargetType.SingleFriendly:
                            textColor = MouseTooltip.ColorText.FriendlyTarget;
                            break;
                        case Item.TargetType.SingleEnemy:
                            textColor = MouseTooltip.ColorText.EnemyTarget;
                            break;
                        case Item.TargetType.SingleAny:
                            textColor = MouseTooltip.ColorText.AnyTarget;
                            break;
                        default:
                            textColor = MouseTooltip.ColorText.Default;
                            break;
                    }

                    MouseTooltip.SetUpToolTip(textColor, message);
                }

                //Display targetoutline material
                ApplyMaterial(CombatManager.instance.outlineTargetSpriteMat);
            }
            else
            {
                //Display tooltip UI
                string message = CombatManager.instance.SelectedItem.description;
                if (message != "")
                {
                    MouseTooltip.SetUpToolTip(MouseTooltip.ColorText.InvalidTarget, message);
                }
            }
        }

        if (CombatManager.instance.GameState == CombatManager.State.PlayerTurn && selectedCharacter != null)
        {

            if (IsValidTarget(selectedCharacter.preparedAction, out string explanation))
            {
                //Display tooltip UI
                string message = selectedCharacter.GetCharacterHooverActionTooltip(selectedCharacter.preparedAction, out MouseTooltip.ColorText textColor, this);
                if (message != "")
                {
                    MouseTooltip.SetUpToolTip(textColor, message);
                }

                //Display targetoutline material
                ApplyMaterial(CombatManager.instance.outlineTargetSpriteMat);
            }
            else
            {
                //Display tooltip UI
                if (explanation != "")
                {
                    MouseTooltip.SetUpToolTip(MouseTooltip.ColorText.InvalidTarget, explanation);
                }
            }
        }
    }

    //Hide UI shown when the mouse is over this character
    public void OnPointerExit(PointerEventData eventData)
    {
        //DISABLE UI INFO
        MouseTooltip.HideTooltip();

        if (CombatManager.instance.SelectedCharacter != this)
        {
            //Apply normal material
            ApplyMaterial(CombatManager.instance.defaultSpriteMat);
        }
        else
        {
            //Apply selected character material
            ApplyMaterial(CombatManager.instance.outlineSelectionSpriteMat);
        }
    }
    #endregion

    #region Actions
    private CombatAction preparedAction = CombatAction.None;
    public virtual CombatAction PreparedAction
    {
        get => preparedAction;
        set
        {
            CombatDelegates.instance.OnPreparedActionChanged?.Invoke(this);
            switch (value)
            {
                case CombatAction.Move:
                    ShowAllAvailableTiles();
                    break;
                case CombatAction.MeleeAttack:
                    break;
                case CombatAction.RangedAttack:
                    break;
                case CombatAction.SpecialAbility:
                    break;
                case CombatAction.UseCannon:
                    UseCannon();
                    break;
                case CombatAction.None:
                    break;
            }
            preparedAction = value;
        }
    }

    public bool IsActionPossible(CombatAction action, out string explanation)
    {
        if (!IsMyTurn(CombatManager.instance.GameState))
        {
            explanation = "Not player turn";
            return false;
        }
        if (!Alive)
        {
            explanation = "Not alive";
            return false;
        }
        switch (action)
        {
            case CombatAction.Move:
                if (HasActionPoints(1))
                {
                    if (GetPossibleMoves().Count > 0)
                    {
                        explanation = "";
                        return true;
                    }
                    else
                    {
                        explanation = "No possible moves available";
                        return false;
                    }
                }
                else
                {
                    explanation = "No action points available";
                    return false;
                }
            case CombatAction.MeleeAttack:
                if (HasActionPoints(1))
                {
                    if (Tile.GetAdjacentEnemies(MyTeam).Count > 0)
                    {
                        explanation = "";
                        return true;
                    }
                    else
                    {
                        explanation = "No nearby enemies";
                        return false;
                    }
                }
                else
                {
                    explanation = "No action points available";
                    return false;
                }
            case CombatAction.RangedAttack:
                if (HasActionPoints(1))
                {
                    if (!(this is Fighter))
                    {
                        if (tile.GetAdjacentEnemies(MyTeam).Count != CombatManager.instance.GetAliveCharacters(Team.AI).Count)
                        {
                            explanation = "";
                            return true;
                        }
                        {
                            explanation = "No characters outside melee range";
                            return false;
                        }
                    }
                    else
                    {
                        explanation = "This character does not have ranged attacks";
                        return false;
                    }
                }
                else
                {
                    explanation = "No action points available";
                    return false;
                }
            case CombatAction.SpecialAbility:
                if (HasActionPoints(specialAbilityAPCost))
                {
                    if (specialAbilityCooldownTimer == 0)
                    {
                        explanation = "";
                        return true;
                    }
                    else
                    {
                        explanation = "Special ability is on cooldown. Action available again in " + specialAbilityCooldownTimer.ToString() + " turns";
                        return false;
                    }
                }
                else
                {
                    explanation = "Requires 2 action points";
                    return false;
                }
            case CombatAction.UseCannon:
                if (HasActionPoints(1))
                {
                    if (Tile.cannon != null)
                    {
                        if (tile.cannon.MyTeam == MyTeam)
                        {
                            explanation = "";
                            return true;
                        }
                        else
                        {
                            explanation = "Cannon is aimed at my ship";
                            return false;
                        }
                    }
                    else
                    {
                        explanation = "Tile does not have a cannon on it";
                        return false;
                    }
                }
                else
                {
                    explanation = "No action points available";
                    return false;
                }
            default:
                Debug.LogError(action.ToString() + " not se up in " + this.name);
                explanation = action.ToString() + " not se up in " + this.name;
                return false;
        }

    }

    public abstract void UseAction(CombatAction action);

    /// <summary>
    /// Returns true if there is ANY points left to spend. Then reduces remaining action points with a cost but never goes lower than 0
    /// </summary>
    protected void SpendActionPoint(int cost)
    {
        if (ActionPoints >= cost)
        {
            ActionPoints -= cost;
            ActionPoints = Mathf.Max(0, ActionPoints);
            Debug.Log(ActionPoints + " action points remaining for character " + GetCharacterData().characterName);
            CombatManager.instance.SetPendingAction(CombatAction.None);
            CombatDelegates.instance.OnActionPerformed?.Invoke(this);
        }
        else
        {
            Debug.Log("No action point available for action");
        }
    }

    protected void DeselectCharacterAfterAnimation()
    {
        CombatManager.instance.SelectedCharacter = null; //Deselect
        CombatDelegates.instance.OnAnimationFinished -= DeselectCharacterAfterAnimation;
    }

    public void MoveCharacter(ShipTile newTile)
    {
        if (newTile.IsAvailable())
        {
            List<TileMovement> movements = GetPossibleMoves();

            if (TileMovement.ExistsInList(movements, newTile))
            {
                int cost = TileMovement.GetTileMovementInList(movements, newTile).cost;
                if (HasActionPoints(cost))
                {
                    Tile.MoveCharacterAwayFromTile(this);
                    Tile = newTile;
                    Tile.MoveCharacterToTile(this);
                    CombatDelegates.instance.OnCharacterMoved?.Invoke(this);
                    SpendActionPoint(cost);
                    if (ActionPoints == 0)
                    {
                        CombatManager.instance.SelectedCharacter = null;
                    }
                    CombatAudio.instance.PlayAudio(CombatAudio.ActionAudio.Move);
                }
                else
                {
                    Debug.Log("Not enough action points to move here, cost is " + cost.ToString());
                }
            }
            else
            {
                Debug.Log("Tile cannot be moved to");
            }
        }
        else
        {
            Debug.Log("Tile not available");
        }
    }

    /// <summary>
    /// Returns all tiles a character can move to
    /// </summary>
    protected List<TileMovement> GetPossibleMoves()
    {
        List<TileMovement> availableTiles = new List<TileMovement>();
        TileMovement.SearchTileForPossibleMoves(ActionPoints, Tile, 1, ref availableTiles);
        return availableTiles;
    }



    protected void ShowAllAvailableTiles()
    {
        foreach (var movement in GetPossibleMoves())
        {
            movement.newTile.DisplayInteractable = true;
        }
    }

    //Melee attack
    protected void MeleeAttack(Character target)
    {
        if (target != null)
        {
            if (HasActionPoints(1))
            {
                int damageToDeal = GetDamageToDeal(GetCharacterData().meleeMinDMG, GetCharacterData().meleeMaxDMG, AttackType.Melee, true, target, out bool isCrit);
                target.QueueDamage(damageToDeal);
                ActionAnimation.instance.PlayAnimation(this, target, damageToDeal, ActionAnimation.AnimationState.Melee, meleeAttackAudio, isCrit);
                SpendAllActionPoints();
                CombatDelegates.instance.OnAnimationFinished += DeselectCharacterAfterAnimation;
            }
        }
        else
        {
            Debug.Log("No target");
        }
    }

    //Use ship cannon
    protected virtual void UseCannon()
    {
        if (HasActionPoints(1))
        {
            Tile.cannon.ShootCannon(1);
            SpendAllActionPoints();
            CombatManager.instance.SelectedCharacter = null; //Deselect
        }
    }
    #endregion

    #region Support Methods
    protected void SpendAllActionPoints()
    {
        SpendActionPoint(ActionPoints);
    }

    protected bool HasActionPoints(int points)
    {
        return ActionPoints >= points;
    }

    /// <summary>
    /// Called at the start of a round to replenish actionpoints and lower cooldown on abilities
    /// </summary>
    public virtual void NewRound(CombatManager.State gameState)
    {
        if (IsMyTurn(gameState))
        {
            if (Alive)
            {
                ActionPoints = roundStartActionPoints;
                specialAbilityCooldownTimer--;
                specialAbilityCooldownTimer = Mathf.Max(0, specialAbilityCooldownTimer);
            }
        }
    }

    public abstract string GetButtonTooltip(CombatAction action);
    public abstract string GetCharacterHooverActionTooltip(CombatAction action, out MouseTooltip.ColorText tooltipColor, Character mouseOverCharacter);

    /// <summary>
    /// Written from the perspective of the target, this (instance) is the target
    /// </summary>
    public bool IsValidTarget(CombatAction action, out string explanation)
    {
        if (!alive)
        {
            explanation = "";
            return false;
        }

        Character originCharacter = CombatManager.instance.SelectedCharacter;
        bool characterIsAdjacent = false;
        if (originCharacter.Tile.GetAdjacentEnemies(originCharacter.MyTeam).Contains(this))
        {
            characterIsAdjacent = true;
        }

        switch (action)
        {
            case CombatAction.MeleeAttack:
                if (originCharacter.MyTeam != MyTeam)
                {
                    if (characterIsAdjacent)
                    {
                        explanation = "";
                        return true;
                    }
                    else
                    {
                        explanation = "Not possible: Not within melee range";
                        return false;
                    }
                }
                else
                {
                    explanation = "Not possible: Characters are on the same team";
                    return false;
                }
            case CombatAction.RangedAttack:
                if (originCharacter.MyTeam != MyTeam)
                {
                    if (!characterIsAdjacent)
                    {
                        explanation = "";
                        return true;
                    }
                    else
                    {
                        explanation = "Not possible: Can't use ranged attack on nearby enemy";
                        return false;
                    }
                }
                else
                {
                    explanation = "Not possible: Characters are on the same team";
                    return false;
                }
            case CombatAction.SpecialAbility:
                if (originCharacter is Fighter)
                {
                    if (originCharacter == this)
                    {
                        explanation = "";
                        return true;
                    }
                    else
                    {
                        explanation = "Not possible: Can only target itself";
                        return false;
                    }
                }
                else if (originCharacter is Gunner)
                {
                    if (originCharacter.MyTeam != MyTeam)
                    {
                        if (!characterIsAdjacent)
                        {
                            explanation = "";
                            return true;
                        }
                        else
                        {
                            explanation = "Not possible: Can't use ranged attack on nearby enemy";
                            return false;
                        }
                    }
                    else
                    {
                        explanation = "Not possible: Characters are on the same team";
                        return false;
                    }
                }
                else if (originCharacter is Chef)
                {
                    if (originCharacter.MyTeam == MyTeam)
                    {
                        explanation = "";
                        return true;
                    }
                    else
                    {
                        explanation = "Not possible: Characters are not on the same team";
                        return false;
                    }
                }
                else if (originCharacter is Captain)
                {
                    if (originCharacter.MyTeam == MyTeam)
                    {
                        if (originCharacter != this)
                        {
                            explanation = "";
                            return true;
                        }
                        else
                        {
                            explanation = "Not possible: Can't target itself";
                            return false;
                        }
                    }
                    else
                    {
                        explanation = "Not possible: Characters are not on the same team";
                        return false;
                    }
                }
                break;
            default:
                explanation = "";
                return false;
        }
        explanation = "";
        return false;
    }

    public bool IsValidTarget(Item item, out string explanation)
    {
        switch (item.myTargetType)
        {
            case Item.TargetType.SingleFriendly:
                if (MyTeam == Team.Player)
                {
                    explanation = "";
                    return true;
                }
                else
                {
                    explanation = "Not possible: Not a friendly target";
                    return false;
                }
            case Item.TargetType.SingleEnemy:
                if (MyTeam == Team.AI)
                {
                    explanation = "";
                    return true;
                }
                else
                {
                    explanation = "Not possible: Not an enemy target";
                    return false;
                }
            case Item.TargetType.SingleAny:
                explanation = "";
                return true;
            default:
                explanation = "Not setup";
                return false;
        }
    }

    public bool IsMyTurn(CombatManager.State gameState)
    {
        return ((MyTeam == Team.Player && gameState == CombatManager.State.PlayerTurn) || MyTeam == Team.AI && gameState == CombatManager.State.AITurn);
    }

    public void Heal(int heal)
    {
        queuedHeal = heal;
        CombatDelegates.instance.OnAnimationFinished += ReceiveQueuedHeal;
        Debug.Log(GetCharacterData().characterName + " has heal Queued to receive after animation is played. Heal is " + heal.ToString());
    }

    int queuedHeal;
    //Receive actual healing after animation
    private void ReceiveQueuedHeal()
    {
        CurrentHP += queuedHeal;
        CurrentHP = Mathf.Min(MAXHP, CurrentHP);
        Debug.Log(GetCharacterData().characterName + " healed for " + queuedHeal.ToString() + " points");
        CombatDelegates.instance.OnAnimationFinished -= ReceiveQueuedHeal;

    }

    //Damage to take after animation is played
    private int queuedDamage = 0;
    protected AttackType queuedDamageType = AttackType.Melee;
    private void ReceiveQueuedDamage()
    {
        CurrentHP -= queuedDamage;
        Debug.Log(GetCharacterData().characterName + " took " + queuedDamage.ToString() + " points of damage");
        CombatDelegates.instance.OnAnimationFinished -= ReceiveQueuedDamage;

        if (Alive) //Only play damage audio if alive
        {
            CombatAudio.instance.PlayAudio(takeDamageAudio);
        }
    }

    public void QueueDamage(int damageToTake)
    {
        queuedDamage = damageToTake;
        CombatDelegates.instance.OnAnimationFinished += ReceiveQueuedDamage;
        Debug.Log(GetCharacterData().characterName + " has damage Queued to receive after animation is played. Damage is " + damageToTake.ToString());
    }

    protected virtual void Die()
    {
        alive = false;
        Debug.Log(GetCharacterData().characterName + " died");

        //Death animation
        animator.SetTrigger("Death");

        //Remove from list of character for player
        if (MyTeam == Team.Player)
        {
            PlayerSession.instance.KillCharacter(this.GetCharacterData());
        }

        CombatDelegates.instance.OnCharacterDied(this);

        CombatAudio.instance.PlayAudio(deathAudio);
    }
    #endregion

    #region Damage Calculations
    /// <summary>
    /// Returns the damage dependent on the attacktype and other modifiers. Outs explanations of the modified result
    /// </summary>
    protected int GetDamageToDeal(int startMinDamage, int startMaxDamage, AttackType attackType, bool useCriticalHitChance, Character target, out bool isCrit)
    {
        int[] damageRange = GetDamageRange(startMinDamage, startMaxDamage, attackType, target, out List<string> modifierExplanations);

        float damageToDeal = Random.Range(damageRange[0], damageRange[1]);

        if (useCriticalHitChance && IsCriticalHit())
        {
            damageToDeal += damageToDeal * critModifier;
            isCrit = true;
        }
        else
        {
            isCrit = false;
        }

        return (Mathf.RoundToInt(damageToDeal));
    }

    /// <summary>
    /// Get the range of damage after calculations. Pos 0 = Min, Pos 1 = Max
    /// </summary>
    public int[] GetDamageRange(int startMinDamage, int startMaxDamage, AttackType attackType, Character target, out List<string> modifierExplanations)
    {
        //Pos 0 = Min, Pos 1 = Max
        int[] damageRange = new int[] { startMinDamage, startMaxDamage };
        modifierExplanations = new List<string>();

        modifierExplanations.Add("Base damage: " + startMinDamage.ToString() + "-" + startMaxDamage.ToString());

        //Damage output
        List<float> modifiers = new List<float>();
        modifiers.AddRange(GetOutgoingDamageModifiers(attackType, ref modifierExplanations, target));

        //Input modifications of output damage
        modifiers.AddRange(target.GetIncomingDamageModifiers(attackType, ref modifierExplanations));

        float totalModifier = 1; //Start with 1 for 100%
        for (int i = 0; i < modifiers.Count; i++)
        {
            totalModifier += modifiers[i];
            totalModifier = Mathf.Max(0, totalModifier); //Dont go below 0% damage
        }


        damageRange[0] = Mathf.Max(0, Mathf.RoundToInt(totalModifier * damageRange[0])); //Never go below 0, min damage
        damageRange[1] = Mathf.Max(0, Mathf.RoundToInt(totalModifier * damageRange[1])); //never go below 0 max damage

        if (modifierExplanations.Count > 1) //If more than base damage explanation
        {
            string plusMinus = totalModifier < 1 ? "" : "+";
            string addOrRemove = totalModifier < 1 ? "Removing: " : "Adding: ";
            if (totalModifier == 1)
            {
                addOrRemove = "";
            }
            modifierExplanations.Add("Modifier sum = " + plusMinus + Mathf.RoundToInt((totalModifier - 1) * 100).ToString() + "% (" + addOrRemove + Mathf.Abs(damageRange[0] - startMinDamage) + "-" + Mathf.Abs(damageRange[1] - startMaxDamage) + ")");
        }

        return damageRange;
    }

    /// <summary>
    /// Calculates and returns the damage to receive after modifiers
    /// </summary>
    protected virtual List<float> GetIncomingDamageModifiers(AttackType attackType, ref List<string> explanations)
    {
        List<float> modifiers = new List<float>();
        //Captain dmg reduction
        if (GetCaptainBonus(MoralebonusType.DMGReduction, out float captainBonus, out string captainModifierString))
        {
            modifiers.Add(-captainBonus);
            explanations.Add("Captain nearby: -" + captainModifierString);
        }
        return modifiers;
    }

    /// <summary>
    /// Calculates and return the damage to deal after modifiers
    /// </summary>
    protected virtual List<float> GetOutgoingDamageModifiers(AttackType attackType, ref List<string> explanations, Character target)
    {
        List<float> modifiers = new List<float>();
        //Captain dmg reduction
        if (GetCaptainBonus(MoralebonusType.DMGIncrease, out float captainBonus, out string captainModifierString))
        {
            modifiers.Add(captainBonus);
            explanations.Add("Captain nearby: +" + captainModifierString);
        }
        return modifiers;
    }

    public virtual void GetDamageStats(out int minDMGMelee, out int maxDMGMelee, out int minDMGRanged, out int maxDMGRanged, out bool hasBuff)
    {
        hasBuff = GetCaptainBonus(MoralebonusType.DMGIncrease, out float captainBonus, out string captainBonusString);

        minDMGMelee = Mathf.RoundToInt(GetCharacterData().meleeMinDMG * (1 + captainBonus));
        maxDMGMelee = Mathf.RoundToInt(GetCharacterData().meleeMaxDMG * (1 + captainBonus));
        minDMGRanged = Mathf.RoundToInt(GetCharacterData().rangedMinDMG * (1 + captainBonus));
        maxDMGRanged = Mathf.RoundToInt(GetCharacterData().rangedMaxDMG * (1 + captainBonus));
    }

    private bool IsCriticalHit()
    {
        if (Random.Range(0f, 1f) <= GetCharacterData().critChance)
        {
            Debug.Log("CRIT!");
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Calculates if there is any bonus provided by a nearby captain and returns that captains bonus-value
    /// </summary>
    protected bool GetCaptainBonus(MoralebonusType moralebonusType, out float percentage, out string percentageString)
    {
        if (Tile.IsCaptainOnAdjacentTiles(MyTeam, out Captain captain))
        {
            switch (moralebonusType)
            {
                case MoralebonusType.DMGIncrease:
                    percentage = captain.characterData.moraleDMGIncreaseModifier;
                    percentageString = captain.characterData.MoraleDMGIncreaseModifierString;
                    return true;
                case MoralebonusType.DMGReduction:
                    percentage = captain.characterData.moraleDMGDecreaseModifier;
                    percentageString = captain.characterData.MoraleDMGDecreaseModifierString;
                    return true;
                default:
                    Debug.LogWarning("Not implemented");
                    percentage = 0;
                    percentageString = "";
                    return false;
            }
        }
        else
        {
            percentage = 0;
            percentageString = "";
            return false;
        }
    }
    #endregion
}