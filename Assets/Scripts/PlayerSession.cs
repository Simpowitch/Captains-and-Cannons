using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Simple dataholder for the player, with all information about the session. Is only valid during the session. If the player dies or the game shuts down nothing shall be saved

public class PlayerSession : MonoBehaviour
{
    public delegate void SessionStatsChanged();
    public static SessionStatsChanged OnSessionStatsChanged;

    #region Singleton
    public static PlayerSession instance;
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

    #region StartValues
    [Header("Start Event")]
    [SerializeField] NonCombatEvent chooseCaptainStartEvent = null;

    [Header("Player stats")]
    [SerializeField] int startDubloons = 100;
    [SerializeField] int shipStartHP = 100;
    float startTime;

    [Header("Debugger")]
    [SerializeField] bool isDebugger = true;
    [SerializeField] List<CharacterBlueprint> debugBlueprints = new List<CharacterBlueprint>();
    [SerializeField] Item debugItem = null;
    #endregion

    #region Dubloons/Gold
    int dubloons = 0;
    public int Dubloons
    {
        get => dubloons;
    }
    public bool SpendDubloons(int cost)
    {
        if (cost > dubloons)
        {
            return false;
        }
        else
        {
            audioSource.PlayOneShot(spendGold);
            dubloons -= cost;
            OnSessionStatsChanged?.Invoke();
            return true;
        }
    }
    public void GainDubloons(int increase)
    {
        audioSource.PlayOneShot(getGold);
        dubloons += increase;
        OnSessionStatsChanged?.Invoke();
    }
    #endregion

    #region ShipHP
    int shipMaxHP;
    public int ShipMaxHP
    {
        get => shipMaxHP;
    }
    int shipCurrentHP;
    public int ShipCurrentHP
    {
        get => shipCurrentHP;
        set
        {
            if (value > shipMaxHP)
            {
                shipCurrentHP = shipMaxHP; //If trying to increase hp above max hp, set it to max hp
                OnSessionStatsChanged?.Invoke();
            }
            else
            {
                shipCurrentHP = Mathf.Max(1, value); //If the value is below 0, set it to 1 - this is to prevent the player from losing in the map-view. The player can still lose in combat
                OnSessionStatsChanged?.Invoke();
            }
        }
    }
    #endregion

    [Header("Audio")]
    [SerializeField] AudioSource audioSource = null;
    [SerializeField] AudioClip getGold = null;
    [SerializeField] AudioClip spendGold = null;


    [Header("Encounter")]
    public CombatEvent nextEncounter;

    #region Characters
    List<CharacterData> aliveCharacters = new List<CharacterData>();
    List<CharacterData> deadCharacters = new List<CharacterData>();

    public readonly int characterLimit = 5;

    public List<CharacterData> GetCharacterDatas(bool alive)
    {
        if (alive)
        {
            return aliveCharacters;
        }
        else
        {
            return deadCharacters;
        }
    }

    public CharacterData GetCharacter(int index)
    {
        return aliveCharacters[index];
    }

    public int GetNumberOfAliveCharacters()
    {
        return aliveCharacters.Count;
    }

    public void AddNewCharacter(CharacterData newCharacter)
    {
        StartCoroutine(RecruitCharacterSound(newCharacter.recruitmentLine));
        aliveCharacters.Add(newCharacter);
        log.Add(newCharacter.characterName + " was bought at location " + locationsVisited);
        OnSessionStatsChanged?.Invoke();
    }

    public void KillCharacter(CharacterData characterData)
    {
        if (aliveCharacters.Contains(characterData))
        {
            aliveCharacters.Remove(characterData);
            deadCharacters.Add(characterData);
            log.Add(characterData.characterName + " died at location " + locationsVisited);
            OnSessionStatsChanged?.Invoke();
        }
    }

    public void WalkThePlank(CharacterData characterData)
    {
        if (aliveCharacters.Contains(characterData))
        {
            aliveCharacters.Remove(characterData);
            log.Add(characterData.characterName + " was thrown overboard at location " + locationsVisited);
            OnSessionStatsChanged?.Invoke();
        }
    }

    bool isPlayingRecruitmentLine = false;
    IEnumerator RecruitCharacterSound(AudioClip recruitmentLine)
    {
        if (!isPlayingRecruitmentLine)
        {
            isPlayingRecruitmentLine = true;
            audioSource.PlayOneShot(recruitmentLine);
            yield return new WaitForEndOfFrame();
            isPlayingRecruitmentLine = false;
        }
    }


    public bool CheckIfCharacterIsNew(CharacterData characterToCheck)
    {
        //Check alive character names if they match  match with the input-character
        foreach (var item in aliveCharacters)
        {
            if (item.characterName == characterToCheck.characterName)
            {
                return false;
            }
        }
        //Check dead character names if they match with the input-character
        foreach (var item in deadCharacters)
        {
            if (item.characterName == characterToCheck.characterName)
            {
                return false;
            }
        }
        return true;
    }

    public void GiveXPToAllCharacters(int xpToGive)
    {
        for (int i = 0; i < aliveCharacters.Count; i++)
        {
            aliveCharacters[i].IncreaseXP(xpToGive);
            OnSessionStatsChanged?.Invoke();
        }
    }

#if UNITY_EDITOR
    public void ResetHealthOfAllCharacters()
    {
        foreach (var item in aliveCharacters)
        {
            item.currentHP = item.maxHP;
        }
    }
#endif
    #endregion

    #region Inventory
    public readonly List<Item> itemInventory = new List<Item>();
    public readonly int inventoryLimit = 25;


    public void AddItem(Item newItem)
    {
        if (itemInventory.Count < inventoryLimit)
        {
            itemInventory.Add(newItem);
            OnSessionStatsChanged?.Invoke();
        }
        else
        {
            Debug.Log($" {newItem.name} was not added to inventory, Inventory Full");
        }
    }

    public int GetNumberOfItemsInInventory()
    {
        return itemInventory.Count;
    }
    public void UseItem(Item item)
    {
        RemoveItem(item);
        OnSessionStatsChanged?.Invoke();
    }
    public void RemoveItem(Item item)
    {
        if (itemInventory.Contains(item))
        {
            itemInventory.Remove(item);
            OnSessionStatsChanged?.Invoke();
        }
    }
    public bool ItemExistsInInventory(Item item)
    {
        return itemInventory.Contains(item);
    }

#if UNITY_EDITOR
    public void AddDebugItem()
    {
        AddItem(debugItem);
    }
#endif

    #endregion

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        if (isDebugger)
        {
            StartNewSession();
            for (int i = 0; i < debugBlueprints.Count; i++)
            {
                AddNewCharacter(debugBlueprints[i].GetCharacterData());
            }
        }
    }

    public void StartNewSession()
    {
        Debug.Log("Starting new session");

        aliveCharacters.Clear();
        deadCharacters.Clear();
        itemInventory.Clear();

        dubloons = startDubloons;
        shipMaxHP = shipStartHP;
        ShipCurrentHP = shipStartHP;

        ResetSessionStats();

        Utility.DeleteMap();

        //If the session has just started and there are 0 active characters
        if (!isDebugger)
        {
            if (chooseCaptainStartEvent != null)
            {
                Debug.Log("Starting new start event");
                chooseCaptainStartEvent.SendMyEvent();
            }
        }
    }

    #region ActiveSessionStats
    SessionEvents sessionEvents = new SessionEvents(0);
    string sendSessionStringKey = "Send session";
    public void LogEvent(SessionStats.EventType eventType, string eventName)
    {
        sessionEvents.EventOccured(eventType);
        log.Add("Event: " + eventName + ", occured at location: " + locationsVisited);
    }

    private void ResetSessionStats()
    {
        PlayerPrefs.SetInt(sendSessionStringKey, 1);
        startTime = Time.realtimeSinceStartup;
        sessionEvents = new SessionEvents(0);
        locationsVisited = 0;
        nodesPassed = 0;
        log = new List<string>();
    }

    public int locationsVisited;
    public int nodesPassed;

    public List<string> log = new List<string>();
    #endregion

    public void EndSession(SessionStats.GameEnd end)
    {
        
        if (!Application.isEditor && PlayerPrefs.GetInt(sendSessionStringKey) == 1 && log.Count > 0)
        {
            List<CharacterData> allCharacters = new List<CharacterData>();
            allCharacters.AddRange(GetCharacterDatas(true));
            allCharacters.AddRange(GetCharacterDatas(false));
            SessionStats endedSession = new SessionStats(Application.version, startTime, end, locationsVisited, nodesPassed, Dubloons, ShipCurrentHP, sessionEvents, allCharacters, log);
            endedSession.CreateStatsMessage();
            PlayerPrefs.SetInt(sendSessionStringKey, 0);
        }
    }
}