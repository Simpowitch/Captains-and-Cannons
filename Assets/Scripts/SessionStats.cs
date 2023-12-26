using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SessionStats
{
    public enum EventType { Battle, Tavern, Shipyard, LandEvent, OceanEvent, LandTreasure, OceanTreasure, Shop}
    public enum GameEnd { Won, Lost, Restart, Exit}

    string gameVersion;
    float timeStarted;
    float timePlayed;

    GameEnd end;
    int finishedAtMapPoint;
    int numberOfNodesPassed;

    int playerGold;
    int shipHP;

    SessionEvents sessionEvents;

    List<CharacterStats> charactersOwned;

    List<string> log;

    //Constructor
    public SessionStats(string gameVersion,
    float timeStarted,

    GameEnd end,
    int finishedAtMapPoint,
    int numberOfNodesPassed,

    int playerGold,
    int shipHP,

    SessionEvents sessionEvents,

    List<CharacterData> characterDatas,

    List<string> log
    )
    {
        this.gameVersion = gameVersion;
        this.timeStarted = timeStarted;
        this.timePlayed = Time.realtimeSinceStartup - timeStarted;

        this.end = end;
        this.finishedAtMapPoint = finishedAtMapPoint;
        this.numberOfNodesPassed = numberOfNodesPassed;

        this.playerGold = playerGold;
        this.shipHP = shipHP;

        this.sessionEvents = sessionEvents;

        this.charactersOwned = new List<CharacterStats>();

        for (int i = 0; i < characterDatas.Count; i++)
        {
            CharacterStats character = new CharacterStats(characterDatas[i]);
            charactersOwned.Add(character);
        }
        this.log = log;
    }
    public void CreateStatsMessage()
    {
        string path;
        string playerName = "";
        string message = "";

        //Get Player Name
        if (System.Environment.UserName != null)
        {
            playerName = System.Environment.UserName;
        }
        if (playerName == "") //if a name could not be found
        {
            playerName = System.Environment.MachineName;
        }
        //Session Message
        message =
            $"Player: {playerName}" + "\n\n" + 
            $"Game Version: {gameVersion}" + "\n" +
            $"Resolution: {Camera.main.scaledPixelWidth} + {Camera.main.scaledPixelHeight}" + "\n" + 
            $"Time Played: {Mathf.RoundToInt(timePlayed / 60)} min {timePlayed % 60} sec" + "\n" +
            $"Ended by: {end.ToString()}" + "\n" +
            $"Finishied after: {finishedAtMapPoint}" + "\n" +
            $"Number of nodes passed: {numberOfNodesPassed}" + "\n" +
            $"Gold at the end: {playerGold}" + "\n" +
            $"Ship Health at the end: {shipHP}" + "\n" +
            $"Taverns visited: {sessionEvents.tavernsVisited}" + "\n" +
            $"Shipyards visited: {sessionEvents.shipyardsVisited}" + "\n" +
            $"Shops visited {sessionEvents.shopVisited}" + "\n" +
            $"Island events occured: {sessionEvents.landEventOccured}" + "\n" +
            $"Ocean events occured: {sessionEvents.oceanEventOccured}" + "\n" +
            $"Battle events occured: {sessionEvents.battlesOccured}" + "\n" +
            $"Island Treasures occured: {sessionEvents.landTreasureOccured}" + "\n" +
            $"Ocean Treasures occured: {sessionEvents.oceanTreasureOccured}" + "\n";
        foreach (CharacterStats character in charactersOwned)
        {
            message +=
                $"-----------------Character---------------------" + "\n" +
                $"name: {character.name}" + "\n" +
                $"class: {character.characterClass}" + "\n" +
                $"join crew at point: {character.characterBoughtAtMapPoint}" + "\n" +
                $"left crew at point: {character.characterDiedAtMapPoint}" + "\n" +
                $"Level: {character.level}" + "\n" +
                $"Health: {character.currentHP} / {character.maxHP} HP" + "\n" +
                $"Crit chance: {character.critChance}" + "\n" +
                $"Melee Damage: {character.meleeMinDMG} - {character.meleeMaxDMG}" + "\n" +
                $"Ranged Damage: {character.rangedMinDMG} - {character.rangedMaxDMG}" + "\n";
        }

        message += "--------------------Log------------------------" + "\n";
        foreach (string logItem in log)
        {
            message += logItem + "\n"; 
        }
        path = Application.dataPath + $"/{playerName}_v{gameVersion}.txt";
        if (File.Exists(path))
        {
            File.Delete(path);
            File.WriteAllText(path, message);
        }
        else
        {
            File.WriteAllText(path, message);
        }
        StatisticsToForm.SendMessage(path);
    }
}

[System.Serializable]
public class CharacterStats
{
    public string name;

    public string characterClass;
    public int level;

    public int currentHP;
    public int maxHP;

    public int meleeMinDMG;
    public int meleeMaxDMG;

    public int rangedMinDMG;
    public int rangedMaxDMG;

    public float critChance;

    public int characterBoughtAtMapPoint;
    public int characterDiedAtMapPoint;
    public CharacterStats(CharacterData character)
    {
        this.name = character.characterName;

        if (character is CaptainData)
        {
            this.characterClass = "Captain";
        }
        else if(character is GunnerData)
        {
            this.characterClass = "Gunnder";
        }
        else if (character is FighterData)
        {
            this.characterClass = "Fighter";
        }
        else if (character is ChefData)
        {
            this.characterClass = "Chef";
        }
        this.level = character.Level;
        this.currentHP = character.currentHP;
        this.maxHP = character.maxHP;
        this.meleeMinDMG = character.meleeMinDMG;
        this.meleeMaxDMG = character.meleeMaxDMG;
        this.rangedMinDMG = character.rangedMinDMG;
        this.rangedMaxDMG = character.rangedMaxDMG;
        this.critChance = character.critChance;
    }
}

public struct SessionEvents
{
    public int battlesOccured;
    public int tavernsVisited;
    public int shipyardsVisited;
    public int landEventOccured;
    public int oceanEventOccured;
    public int landTreasureOccured;
    public int oceanTreasureOccured;
    public int shopVisited;

    public void EventOccured(SessionStats.EventType eventType)
    {
        switch (eventType)
        {
            case SessionStats.EventType.Battle:
                battlesOccured++;
                break;
            case SessionStats.EventType.Tavern:
                tavernsVisited++;
                break;
            case SessionStats.EventType.Shipyard:
                shipyardsVisited++;
                break;
            case SessionStats.EventType.LandEvent:
                landEventOccured++;
                break;
            case SessionStats.EventType.OceanEvent:
                oceanEventOccured++;
                break;
            case SessionStats.EventType.LandTreasure:
                landTreasureOccured++;
                break;
            case SessionStats.EventType.OceanTreasure:
                oceanTreasureOccured++;
                break;
            case SessionStats.EventType.Shop:
                shopVisited++;
                break;
        }
    }
    public SessionEvents(int startvalue)
    {
        battlesOccured = startvalue;
        tavernsVisited = startvalue;
        shipyardsVisited = startvalue;
        landEventOccured = startvalue;
        oceanEventOccured = startvalue;
        landTreasureOccured = startvalue;
        oceanTreasureOccured = startvalue;
        shopVisited = startvalue;
    }
}