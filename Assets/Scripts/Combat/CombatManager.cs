using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Simon Voss

//Responsible for handling turn-based combat systems and information

public class CombatManager : MonoBehaviour
{
    #region Singleton
    public static CombatManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Another instance of : " + instance.ToString() + " was tried to be instanced, but was destroyed from gameobject: " + this.transform.name);
            GameObject.Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    #endregion

    [Header("Character materials")]
    public Material defaultSpriteMat = null;
    public Material outlineSelectionSpriteMat = null;
    public Material outlineTargetSpriteMat = null;

    [Header("AI")]
    [SerializeField] AI ai = null;

    [Header("Transform Parents")]
    [SerializeField] Transform playerCharacterTransform = null;
    [SerializeField] Transform aiCharacterTransform = null;

    List<Character> allCharacters = new List<Character>();
    public List<Character> GetAliveCharacters(Team team)
    {
        List<Character> characters = new List<Character>();
        foreach (var item in allCharacters)
        {
            if (item.MyTeam == team && item.Alive)
            {
                characters.Add(item);
            }
        }
        return characters;
    }
    [SerializeField] BossIntro bossIntro = null;
    [SerializeField] SceneTransition toCreditsScene = null;

    private void Start()
    {
        //intro animation?
        bossIntro.RunIntro();
        StartFight();
    }

    private void Update()
    {
#if UNITY_EDITOR
        CheckCheatHotkey();
#endif
        CheckHotkeys();
    }

    private void CheckHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameState == State.PlayerTurn)
            {
                ChangeTurn();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            CyclePlayerCharacters();
        }
    }

    private void CyclePlayerCharacters()
    {
        if (SelectedCharacter == null)
        {
            SelectedCharacter = GetAliveCharacters(Team.Player)[0];
        }
        else
        {
            List<Character> characters = GetAliveCharacters(Team.Player);
            int indexNext = 0;
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] == SelectedCharacter)
                {
                    indexNext = i + 1;
                }
            }

            if (indexNext >= characters.Count)
            {
                indexNext = 0;
            }
            SelectedCharacter = characters[indexNext];
        }
    }

#if UNITY_EDITOR
    int combatSceneBuildIndex = 3;

    private void CheckCheatHotkey()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("Win Cheat Used");
                EndFightInstantly(true);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log("Loss Cheat Used");
                EndFightInstantly(false);
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                Debug.Log("XP Cheat Used (give 100 xp)");
                GiveXPToCharacters();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Restart fight cheat used");
                RestartFight();
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log("Heal cheat used");
                HealAllMyCharacters();
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("Item cheat used");
                GiveItem();
            }
        }
    }

    private void RestartFight()
    {
        SceneManager.LoadScene(combatSceneBuildIndex);
    }

    private void EndFightInstantly(bool win)
    {
        EndBattleEvent(null, ships[win ? 1 : 0]);
    }

    private void GiveXPToCharacters()
    {
        PlayerSession.instance.GiveXPToAllCharacters(100);
    }

    private void HealAllMyCharacters()
    {
        PlayerSession.instance.ResetHealthOfAllCharacters();
    }

    private void GiveItem()
    {
        PlayerSession.instance.AddDebugItem();
    }

#endif

    #region Turn
    public enum State { Intro, PlayerTurn, AITurn, Lost, Won }
    private State gameState;
    public State GameState
    {
        get => gameState;
    }

    public void ChangeTurn()
    {
        if (gameState != State.AITurn && gameState != State.PlayerTurn)
        {
            return;
        }
        if (gameState == State.PlayerTurn)
        {
            SetTurn(Team.AI);
        }
        else
        {
            SetTurn(Team.Player);
        }
    }

    private void SetTurn(Team team)
    {
        gameState = team == Team.Player ? State.PlayerTurn : State.AITurn;
        SelectedCharacter = null;
        TargetCharacter = null;
        CombatDelegates.instance.OnTurnStatusChanged?.Invoke(gameState);
        if (team == Team.AI)
        {
            ai.NewTurn();
        }
    }

    public void EndBattleEvent(Captain deadCaptain, Ship sunkenShip)
    {
        Team losingTeam;
        if (deadCaptain != null)
        {
            losingTeam = deadCaptain.MyTeam;
        }
        else if (sunkenShip != null)
        {
            losingTeam = sunkenShip.Team;
        }
        else
        {
            Debug.LogError("No parameters valid to end battle, but was still called");
            return;
        }
        if (losingTeam == Team.Player)
        {
            CombatDelegates.instance.PlayerLost();
            PlayerSession.instance.EndSession(SessionStats.GameEnd.Lost);
        }
        else
        {
            if (PlayerSession.instance.nextEncounter.isFinalBoss)
            {
                toCreditsScene.ChangeScene();
                PlayerSession.instance.EndSession(SessionStats.GameEnd.Won);
            }
            else
            {
                CombatDelegates.instance.PlayerWon();
                PlayerSession.instance.GiveXPToAllCharacters(PlayerSession.instance.nextEncounter.GetXPPerCharacter(PlayerSession.instance.GetNumberOfAliveCharacters()));
                PlayerSession.instance.ShipCurrentHP = ships[0].CurrentHp;
                PlayerSession.instance.GainDubloons(PlayerSession.instance.nextEncounter.Dubloons);
            }
        }
    }

    #endregion

    #region Selections
    private Character selectedCharacter;
    public Character SelectedCharacter
    {
        get => selectedCharacter;
        set
        {
            if (selectedCharacter != null) //Deselcting previous selected character
            {
                selectedCharacter.ApplyMaterial(defaultSpriteMat);
            }
            selectedCharacter = value;
            if (selectedCharacter != null) //Setting new character as selected
            {
                selectedCharacter.ApplyMaterial(outlineSelectionSpriteMat);
            }
            //Enable collision on all characters except the selected
            foreach (var item in allCharacters)
            {
                item.SetColliderStatus(item != selectedCharacter);
            }
            TargetCharacter = null;
            SetPendingAction(CombatAction.None);
            CombatDelegates.instance.OnSelectedCharacter?.Invoke(selectedCharacter);
        }
    }

    private Character targetCharacter;
    public Character TargetCharacter
    {
        get => targetCharacter;
        set
        {
            if (value != null)
            {
                if (SelectedCharacter)
                {
                    Team selectedCharacterTeam = SelectedCharacter.MyTeam;

                    //If the selected character is of the same team as the active team
                    if ((selectedCharacterTeam == Team.Player && gameState == State.PlayerTurn) || selectedCharacterTeam == Team.AI && gameState == State.AITurn)
                    {
                        if (targetCharacter != null)
                        {
                            targetCharacter.ApplyMaterial(defaultSpriteMat);
                        }

                        targetCharacter = value;
                        targetCharacter.ApplyMaterial(outlineTargetSpriteMat);

                        if (selectedCharacter.PreparedAction != CombatAction.None)
                        {
                            SelectedCharacter.UseAction(selectedCharacter.PreparedAction);
                        }
                    }
                }
                else if (SelectedItem != null)
                {
                    targetCharacter = value;

                    //If player turn
                    if (gameState == State.PlayerTurn)
                    {
                        switch (selectedItem.myTargetType)
                        {
                            case Item.TargetType.SingleFriendly:
                                if (TargetCharacter.MyTeam == Team.Player)
                                {
                                    selectedItem.ApplyEffect(targetCharacter);
                                    selectedItem.RemoveItemFromPlayer();
                                    selectedItem = null;
                                }
                                break;
                            case Item.TargetType.SingleEnemy:
                                if (TargetCharacter.MyTeam == Team.AI)
                                {
                                    selectedItem.ApplyEffect(targetCharacter);
                                    selectedItem.RemoveItemFromPlayer();
                                    selectedItem = null;
                                }
                                break;
                            case Item.TargetType.SingleAny:
                                selectedItem.ApplyEffect(targetCharacter);
                                selectedItem.RemoveItemFromPlayer();
                                selectedItem = null;
                                break;
                            default:
                                Debug.LogWarning("Item cannot target multiple targets.");
                                break;
                        }
                    }
                }
            }
            else
            {
                if (targetCharacter != null)
                {
                    targetCharacter.ApplyMaterial(defaultSpriteMat);
                }
                targetCharacter = value;
            }
        }
    }

    private Item selectedItem;
    public Item SelectedItem
    {
        get => selectedItem;
        set
        {
            if (GameState == State.PlayerTurn)
            {
                TargetCharacter = null;
                SelectedCharacter = null;
                selectedItem = value;
                CombatAudio.instance.PlayAudio(CombatAudio.ActionAudio.Selection);
            }
        }
    }
    public void SetPendingAction(CombatAction action)
    {
        if (selectedCharacter)
        {
            selectedCharacter.PreparedAction = action;

            //Remove collisions for easier clicks on tiles if player wants to move a character
            if (action == CombatAction.Move)
            {
                //Disable collision on all characters
                foreach (var item in allCharacters)
                {
                    item.SetColliderStatus(false);
                }
            }
            else
            {
                //Enable collision on all characters except the selected
                foreach (var item in allCharacters)
                {
                    item.SetColliderStatus(item != selectedCharacter);
                }
            }
        }
    }
    #endregion

    #region Setup
    public Ship[] ships = new Ship[2];
    public void StartFight()
    {
        if (gameState != State.Intro)
        {
            return;
        }

        CombatEvent encounter = PlayerSession.instance.nextEncounter;
        Debug.Log("Begining battle with:" + encounter.name);

        //Audio
        CombatAudio.instance.PlayMusic(encounter.isFinalBoss);
        if (!encounter.isFinalBoss)
        {
            CombatAudio.instance.PlayIntroStinger();
        }

        //Characters
        foreach (var item in PlayerSession.instance.GetCharacterDatas(true))
        {
            //Spawn character
            Character newCharacter = Instantiate(item.prefabObject, playerCharacterTransform).GetComponent<Character>();
            //Add character data to spawned character
            newCharacter.SetCharacterData(item);
            allCharacters.Add(newCharacter);
        }


        foreach (var item in encounter.characterBlueprints)
        {
            //Spawn characters
            Character newCharacter = Instantiate(item.GetCharacterData().prefabObject, aiCharacterTransform).GetComponent<Character>();
            //Add character data to spawned character
            newCharacter.SetCharacterData(item.GetCharacterData());
            //Add spawned character to AI
            ai.AddCharacterToControl(newCharacter);
            newCharacter.MyTeam = Team.AI;
            allCharacters.Add(newCharacter);
        }

        AssignTilesToCharacters(GetAliveCharacters(Team.Player), ships[0].MyTiles);
        AssignTilesToCharacters(GetAliveCharacters(Team.AI), ships[1].MyTiles);

        //Sets character ready for combat
        foreach (Character character in allCharacters)
        {
            character.StartBattle();
        }
        //


        //Ships
        ships[1].spriteRenderer.sprite = encounter.shipSprite;
        ships[1].railingRenderer.sprite = encounter.shipRailing;
        ships[1].MaxHp = encounter.shipHP;
        ships[1].CurrentHp = encounter.shipHP;


        ships[0].MaxHp = PlayerSession.instance.ShipMaxHP;
        ships[0].CurrentHp = PlayerSession.instance.ShipCurrentHP;

        //Player starts always
        Team teamStart = Team.Player;
        SetTurn(teamStart);
    }


    private void AssignTilesToCharacters(List<Character> characterList, List<ShipTile> tileList)
    {
        tileList = Utility.ShuffleList(tileList);
        for (int i = 0; i < characterList.Count; i++)
        {
            tileList[i].MoveCharacterToTile(characterList[i]);
            characterList[i].Tile = tileList[i];
        }
    }
    #endregion

}
