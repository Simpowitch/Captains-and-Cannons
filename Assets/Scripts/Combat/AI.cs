using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Simon Voss
//Turn based AI, calculates and performs moves when called upon

public class AI : MonoBehaviour
{
    List<Character> myCharacters = new List<Character>();

    Ship playerShip;

    private void Start()
    {
        playerShip = CombatManager.instance.ships[(int)Team.Player];
        CombatDelegates.instance.OnPlayerLost += PlayerLost;
    }


    [SerializeField] bool failSafeTimer = false; //For debugging
    [SerializeField] float safeTimer = 0f; //For debugging
    const float safeTimerLimit = 15f;
    private void Update()
    {
        if (failSafeTimer)
        {
            safeTimer += Time.deltaTime;

            if (safeTimer > safeTimerLimit)
            {
                ForceQuitTurn();
            }
        }
    }

    private void ForceQuitTurn()
    {
        failSafeTimer = false;
        StopAllCoroutines();
        CombatDelegates.instance.OnAnimationFinished?.Invoke();
        characterQueue.Clear();
        CombatManager.instance.ChangeTurn();
    }

    public List<Character> GetIngameCharacters()
    {
        return myCharacters;
    }

    public void AddCharacterToControl(Character spawnedCharacter)
    {
        myCharacters.Add(spawnedCharacter);
    }

    Queue<Character> characterQueue = new Queue<Character>();

    bool playerLost = false;
    private void PlayerLost()
    {
        playerLost = true;
    }


    [Header("Damage prio")]
    [Range(0, 1)]
    [SerializeField] float meleeDamagePrio = 1;
    [Range(0, 1)]
    [SerializeField] float rangedDamagePrio = 1;

    [Header("Damage target prio")]
    [Range(0, 1)]
    [SerializeField] float targetCaptainPrio = 1;
    [Range(0, 1)]
    [SerializeField] float targetNonCaptainPrio = 0.5f;

    [Header("Possible kill bonus")]
    [SerializeField] int killNonCaptainPoint = 10;
    [SerializeField] int killCaptainPoint = 1000;
    [SerializeField] int killShipPoint = 1000;

    [Header("Cannon damage prio")]
    [Range(0, 1)]
    [SerializeField] float nonGunnerCannonPrio = 0.25f;
    [Range(0, 1)]
    [SerializeField] float gunnerCanonPrio = 1;

    [Header("Move prio")]
    [Range(0, 1)]
    [SerializeField] float meleeChasemovePrio = 1;
    [Range(0, 1)]
    [SerializeField] float rangedAvertmovePrio = 1;


    [Header("Delays")]
    [SerializeField] float waitTimeBetweenCharacters = 1;
    [SerializeField] float waitTimeBetweenActions = 4;



    public void NewTurn()
    {
        ShuffleAndQueueCharacters(myCharacters);
        StartCoroutine(PerformActionOnCharacter(characterQueue.Dequeue()));
    }


    private void ShuffleAndQueueCharacters(List<Character> characters)
    {
        List<Character> shuffledList = new List<Character>();
        List<Character> clonedList = new List<Character>();

        foreach (var item in characters)
        {
            clonedList.Add(item);
        }

        while (clonedList.Count > 0)
        {
            int randomIndex = Random.Range(0, clonedList.Count);
            shuffledList.Add(clonedList[randomIndex]);
            clonedList.RemoveAt(randomIndex);
        }
        for (int i = 0; i < shuffledList.Count; i++)
        {
            characterQueue.Enqueue(shuffledList[i]);
        }
    }



    IEnumerator PerformActionOnCharacter(Character activeIngameCharacter)
    {
        if (activeIngameCharacter.Alive && !playerLost)
        {
            failSafeTimer = true;
            safeTimer = 0; //Reset timer

            CombatManager.instance.SelectedCharacter = activeIngameCharacter;
            CharacterData characterData = activeIngameCharacter.GetCharacterData();
            Debug.Log("Starting AI decision making for next character in queue: " + characterData.characterName);

            yield return new WaitForSeconds(waitTimeBetweenCharacters);


            //Find player character
            List<Character> possibleTargets = new List<Character>();

            foreach (var item in CombatManager.instance.GetAliveCharacters(Team.Player))
            {
                if (item.Alive)
                {
                    possibleTargets.Add(item);
                }
            }

            if (possibleTargets.Count <= 0)
            {
                Debug.Log("No alive player characters found");
            }
            else
            {
                PreferedDistance preferedDistance = PreferedDistance.Melee;

                if (characterData.rangedMaxDMG > characterData.meleeMaxDMG)
                {
                    preferedDistance = PreferedDistance.Ranged;
                }

                List<ActionGroup> possibleActions = new List<ActionGroup>();



                //Move
                foreach (var firstTile in activeIngameCharacter.Tile.AdjacentAndBoardingTiles)
                {
                    if (firstTile.IsAvailable())
                    {
                        //Move + Move
                        foreach (var secondTile in firstTile.AdjacentAndBoardingTiles)
                        {
                            if (secondTile.IsAvailable())
                            {
                                ActionGroup newActionGroup = new ActionGroup();
                                possibleActions.Add(newActionGroup);
                                newActionGroup.AddAction(ActionGroup.AIActionType.Move, firstTile);
                                newActionGroup.AddAction(ActionGroup.AIActionType.Move, secondTile);
                                newActionGroup.endTile = secondTile;
                            }
                        }

                        //Move + Attack melee
                        foreach (var enemy in firstTile.GetAdjacentEnemies(activeIngameCharacter.MyTeam))
                        {
                            if (enemy.Alive)
                            {
                                ActionGroup newActionGroup = new ActionGroup();
                                possibleActions.Add(newActionGroup);
                                newActionGroup.AddAction(ActionGroup.AIActionType.Move, firstTile);
                                newActionGroup.AddAction(ActionGroup.AIActionType.AttackMelee, enemy.Tile);
                                newActionGroup.endTile = firstTile;
                            }
                        }

                        if (characterData.rangedMaxDMG > 0)
                        {
                            //Move + Attack ranged

                            List<Character> possibleRangedTargets = new List<Character>();
                            possibleRangedTargets.AddRange(possibleTargets);
                            foreach (var enemy in firstTile.GetAdjacentEnemies(activeIngameCharacter.MyTeam)) //Check melee range targets
                            {
                                if (enemy.Alive && possibleRangedTargets.Contains(enemy))
                                {
                                    possibleRangedTargets.Remove(enemy); //Remove characters that are in meleerange
                                }
                            }

                            foreach (var enemy in possibleRangedTargets)
                            {
                                ActionGroup newActionGroup = new ActionGroup();
                                possibleActions.Add(newActionGroup);
                                newActionGroup.AddAction(ActionGroup.AIActionType.Move, firstTile);
                                newActionGroup.AddAction(ActionGroup.AIActionType.AttackRanged, enemy.Tile);
                                newActionGroup.endTile = firstTile;
                            }
                        }

                        //Move + Fire Cannon
                        if (firstTile.cannon != null && firstTile.cannon.MyTeam == Team.AI)
                        {
                            ActionGroup newActionGroup = new ActionGroup();
                            possibleActions.Add(newActionGroup);
                            newActionGroup.AddAction(ActionGroup.AIActionType.Move, firstTile);
                            newActionGroup.AddAction(ActionGroup.AIActionType.UseCannon, null);
                            newActionGroup.endTile = firstTile;
                        }
                    }
                }

                //Attack melee
                foreach (var enemy in activeIngameCharacter.Tile.GetAdjacentEnemies(activeIngameCharacter.MyTeam))
                {
                    if (enemy.Alive)
                    {
                        ActionGroup newActionGroup = new ActionGroup();
                        possibleActions.Add(newActionGroup);
                        newActionGroup.AddAction(ActionGroup.AIActionType.AttackMelee, enemy.Tile);
                        newActionGroup.endTile = activeIngameCharacter.Tile;
                    }
                }

                if (characterData.rangedMaxDMG > 0)
                {
                    //Attack ranged
                    List<Character> possibleRangedTargets = new List<Character>();
                    possibleRangedTargets.AddRange(possibleTargets);
                    foreach (var enemy in activeIngameCharacter.Tile.GetAdjacentEnemies(activeIngameCharacter.MyTeam)) //Check melee range targets
                    {
                        if (enemy.Alive && possibleRangedTargets.Contains(enemy))
                        {
                            possibleRangedTargets.Remove(enemy); //Remove characters that are in meleerange
                        }
                    }

                    foreach (var enemy in possibleRangedTargets)
                    {
                        ActionGroup newActionGroup = new ActionGroup();
                        possibleActions.Add(newActionGroup);
                        newActionGroup.AddAction(ActionGroup.AIActionType.AttackRanged, enemy.Tile);
                        newActionGroup.endTile = activeIngameCharacter.Tile;
                    }
                }

                //Fire Cannon
                if (activeIngameCharacter.Tile.cannon != null && activeIngameCharacter.Tile.cannon.MyTeam == Team.AI)
                {
                    ActionGroup newActionGroup = new ActionGroup();
                    possibleActions.Add(newActionGroup);
                    newActionGroup.AddAction(ActionGroup.AIActionType.UseCannon, null);
                    newActionGroup.endTile = activeIngameCharacter.Tile;
                }


                //Score
                foreach (var actionGroup in possibleActions)
                {
                    actionGroup.score = Score(activeIngameCharacter, preferedDistance, actionGroup, possibleTargets);
                }

                if (possibleActions.Count > 0) //If there is atleast 1 action to do
                {
                    //Pick best score
                    ActionGroup bestAction = PickActionGroup(possibleActions);

                    //Perform action(s)
                    for (int i = 0; i < bestAction.actions.Count; i++)
                    {
                        switch (bestAction.actions[i])
                        {
                            case ActionGroup.AIActionType.Move:
                                Debug.Log(activeIngameCharacter.GetCharacterData().characterName + " is moving");
                                activeIngameCharacter.MoveCharacter(bestAction.targetsForAction[i]);
                                break;
                            case ActionGroup.AIActionType.AttackMelee:
                                Character meleeTarget = bestAction.targetsForAction[i].GetCharacterOnTile();
                                if (meleeTarget == null)
                                {
                                    Debug.LogError("Tried to melee attack character on tile " + bestAction.targetsForAction[i].name + " but, Target is null");
                                    break;
                                }
                                Debug.Log(activeIngameCharacter.GetCharacterData().characterName + " is melee attacking " + meleeTarget.GetCharacterData().characterName);
                                CombatManager.instance.TargetCharacter = meleeTarget;
                                activeIngameCharacter.UseAction(CombatAction.MeleeAttack);
                                waitingForAnimation = true;
                                CombatDelegates.instance.OnAnimationFinished += ContinueAI;
                                break;
                            case ActionGroup.AIActionType.AttackRanged:
                                Character rangedTarget = bestAction.targetsForAction[i].GetCharacterOnTile();
                                if (rangedTarget == null)
                                {
                                    Debug.LogError("Tried to melee attack character on tile " + bestAction.targetsForAction[i].name + " but, Target is null");
                                    break;
                                }
                                Debug.Log(activeIngameCharacter.GetCharacterData().characterName + " is range attacking " + rangedTarget.GetCharacterData().characterName);
                                CombatManager.instance.TargetCharacter = rangedTarget;
                                activeIngameCharacter.UseAction(CombatAction.RangedAttack);
                                waitingForAnimation = true;
                                CombatDelegates.instance.OnAnimationFinished += ContinueAI;
                                break;
                            case ActionGroup.AIActionType.UseCannon:
                                Debug.Log(activeIngameCharacter.GetCharacterData().characterName + " is using cannon");
                                activeIngameCharacter.UseAction(CombatAction.UseCannon);
                                break;
                        }
                        while (waitingForAnimation)
                        {
                            yield return null;
                        }
                        yield return new WaitForSeconds(waitTimeBetweenActions);
                    }
                }
            }
        }

        CombatManager.instance.SelectedCharacter = null;
        CombatManager.instance.TargetCharacter = null;

        if (characterQueue.Count > 0 && !playerLost)
        {
            StartCoroutine(PerformActionOnCharacter(characterQueue.Dequeue()));
        }
        else if (!playerLost)
        {
            failSafeTimer = false;
            CombatManager.instance.ChangeTurn();
        }
    }

    bool waitingForAnimation = false;
    private void ContinueAI()
    {
        waitingForAnimation = false;
        CombatDelegates.instance.OnAnimationFinished -= ContinueAI;
    }

    private ActionGroup PickActionGroup(List<ActionGroup> possibleActions)
    {
        possibleActions = possibleActions.OrderByDescending(x => x.score).ToList();
        return possibleActions[0];
    }

    private Character GetClosestCharacterByDistance(Vector2 originPos, List<Character> characters)
    {
        Character closest = null;
        float shortestDistance = float.MaxValue;

        foreach (var item in characters)
        {
            float testDistance = Vector2.Distance(originPos, item.transform.position);
            if (testDistance < shortestDistance)
            {
                closest = item;
                shortestDistance = testDistance;
            }
        }
        return closest;
    }

    private enum PreferedDistance { Melee, Ranged }
    private int Score(Character ingameCharacter, PreferedDistance preferedDistance, ActionGroup actionGroup, List<Character> aliveEnemies)
    {
        CharacterData characterData = ingameCharacter.GetCharacterData();

        int score = 0;
        score += ScoreEndTile(actionGroup.endTile, ingameCharacter, preferedDistance, aliveEnemies);
        score += ScoreActions(characterData, ingameCharacter, actionGroup);
        return score;
    }

    //Get Points for the chosen tile to end character turn on
    private int ScoreEndTile(ShipTile endTile, Character ingameCharacter, PreferedDistance preferedDistance, List<Character> aliveEnemies)
    {
        float score = 0;

        //Placement points at end tile
        List<Character> enemiesCloseBy = endTile.GetAdjacentEnemies(ingameCharacter.MyTeam);
        Vector2 closestCharacterPosition = GetClosestCharacterByDistance(endTile.transform.position, aliveEnemies).transform.position;
        float distanceToClosestCharacterAtEndTile = Vector2.Distance(closestCharacterPosition, endTile.transform.position);
        if (enemiesCloseBy.Count > 0)
        {
            switch (preferedDistance)
            {
                case PreferedDistance.Melee:
                    score += 10; //Add points for having a target close by to attack
                    score += enemiesCloseBy.Count - 1; //Minus for additional characters
                    break;
                case PreferedDistance.Ranged:
                    score -= enemiesCloseBy.Count;
                    break;
            }

        }
        else //If no targets are around the character
        {
            switch (preferedDistance)
            {
                case PreferedDistance.Melee:
                    score -= distanceToClosestCharacterAtEndTile;
                    break;
                case PreferedDistance.Ranged:
                    score += distanceToClosestCharacterAtEndTile;
                    break;
            }
        }

        //Prioritization
        switch (preferedDistance)
        {
            case PreferedDistance.Melee:
                score *= meleeChasemovePrio;
                break;
            case PreferedDistance.Ranged:
                score *= rangedAvertmovePrio;
                break;
        }
        return Mathf.RoundToInt(score);
    }

    //Score a group of actions
    private int ScoreActions(CharacterData characterData, Character ingameCharacter, ActionGroup actionGroup)
    {
        float score = 0;

        //Action based points
        for (int i = 0; i < actionGroup.actions.Count; i++)
        {
            Character target = null;
            if (actionGroup.targetsForAction[i] != null)
            {
                target = actionGroup.targetsForAction[i].GetCharacterOnTile();
            }
            switch (actionGroup.actions[i])
            {
                case ActionGroup.AIActionType.Move:
                    //Point included already in calculation of end tile
                    break;
                case ActionGroup.AIActionType.AttackMelee:
                    int[] meleeDamageRange = ingameCharacter.GetDamageRange(characterData.meleeMinDMG, characterData.meleeMaxDMG, AttackType.Melee, actionGroup.targetsForAction[i].GetCharacterOnTile(), out List<string> outMeleeModifierExplanations);
                    if (target is Captain)
                    {
                        score += meleeDamageRange[0] * meleeDamagePrio * targetCaptainPrio; //Add points for min damage against captain
                        if (meleeDamageRange[1] >= target.CurrentHP) //If the attack can kill the target at maximum damage
                        {
                            score += killCaptainPoint;
                        }
                    }
                    else
                    {
                        score += meleeDamageRange[0] * meleeDamagePrio * targetNonCaptainPrio; //Add points for min damage against non captain
                        if (meleeDamageRange[1] >= target.CurrentHP) //If the attack can kill the target at maximum damage
                        {
                            score += killNonCaptainPoint;
                        }
                    }
                    break;
                case ActionGroup.AIActionType.AttackRanged:
                    int[] rangedDamageRange = ingameCharacter.GetDamageRange(characterData.meleeMinDMG, characterData.meleeMaxDMG, AttackType.Melee, actionGroup.targetsForAction[i].GetCharacterOnTile(), out List<string> outRangeModifierExplanations);
                    if (target is Captain)
                    {
                        score += rangedDamageRange[0] * rangedDamagePrio * targetCaptainPrio; //Add points for min damage against captain
                        if (rangedDamageRange[1] >= target.CurrentHP) //If the attack can kill the target at maximum damage
                        {
                            score += killCaptainPoint;
                        }
                    }
                    else
                    {
                        score += rangedDamageRange[0] * rangedDamagePrio * targetNonCaptainPrio; //Add points for min damage against non captain
                        if (rangedDamageRange[1] >= target.CurrentHP) //If the attack can kill the target at maximum damage
                        {
                            score += killNonCaptainPoint;
                        }
                    }
                    break;
                case ActionGroup.AIActionType.UseCannon:
                    if (ingameCharacter is Gunner)
                    {
                        Gunner gunner = ingameCharacter as Gunner;

                        score += actionGroup.endTile.cannon.GetDamageToDeal(1 + GunnerData.cannonDamageIncrease, DamageRandomModifier.Min) * gunnerCanonPrio; //Get points of the cannon min damage

                        if (actionGroup.endTile.cannon.GetDamageToDeal(1 + GunnerData.cannonDamageIncrease, DamageRandomModifier.Max) >= playerShip.CurrentHp) //If the attack can sink the ship at maximum cannon damage
                        {
                            score += killShipPoint;
                        }
                    }
                    else
                    {
                        score += actionGroup.endTile.cannon.GetDamageToDeal(1, DamageRandomModifier.Min) * nonGunnerCannonPrio; //Get points of the cannon min damage

                        if (actionGroup.endTile.cannon.GetDamageToDeal(1, DamageRandomModifier.Max) >= playerShip.CurrentHp) //If the attack can sink the ship at maximum cannon damage
                        {
                            score += killShipPoint;
                        }
                    }
                    break;
            }
        }
        return Mathf.RoundToInt(score);
    }
}

class ActionGroup
{
    public enum AIActionType { Move, AttackMelee, AttackRanged, UseCannon }
    public int score;
    public List<AIActionType> actions = new List<AIActionType>(2); //What shall the character do
    public List<ShipTile> targetsForAction = new List<ShipTile>(2); //Which tiles are affected by the character's actions
    public ShipTile endTile; //Where shall the character end its turn, used to calculate points together with attacking actions

    public void AddAction(AIActionType newAction, ShipTile targetForAction)
    {
        actions.Add(newAction);
        targetsForAction.Add(targetForAction);
    }
}
