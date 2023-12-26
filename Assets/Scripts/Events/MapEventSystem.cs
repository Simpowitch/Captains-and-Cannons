using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Simon Voss
//Random generator of events on the map

public class MapEventSystem : MonoBehaviour
{


    [Header("Ocean")]
    [Header("Probabilities")]
    [Range(0f, 1f)] [SerializeField] float oceanFightChance = 0.2f;
    [Range(0f, 1f)] [SerializeField] float oceanTreasureChance = 0.1f;
    [Range(0f, 1f)] [SerializeField] float oceanDefaultEventChance = 0.1f;

    [Header("Island")]
    [Range(0f, 1f)] [SerializeField] float islandShopChance = 0.05f;
    [Range(0f, 1f)] [SerializeField] float islandTreasureChance = 0.05f;
    [Range(0f, 1f)] [SerializeField] float islandTavernChance = 0.33f;

    [Header("Tavern is also Shipyard")]
    [Range(0f, 1f)] [SerializeField] float islandshipyardChance = 0.33f;

    [Header("Battle Events - Ocean")]
    [Header("Events Ocean")]
    [SerializeField] CombatEvent[] actOneBattlesOcean = null;
    [SerializeField] CombatEvent[] actTwoBattlesOcean = null;
    [SerializeField] CombatEvent[] actThreeBattlesOcean = null;

    [Header("Treasure Events - Ocean")]
    [SerializeField] NonCombatEvent[] actOneTreasuresOcean = null;
    [SerializeField] NonCombatEvent[] actTwoTreasuresOcean = null;
    [SerializeField] NonCombatEvent[] actThreeTreasuresOcean = null;

    [Header("Default Events - Ocean")]
    [SerializeField] NonCombatEvent[] actOneDefaultEventsOcean = null;
    [SerializeField] NonCombatEvent[] actTwoDefaultEventsOcean = null;
    [SerializeField] NonCombatEvent[] actThreeDefaultEventsOcean = null;

    [Header("Shop Events - Island")]
    [Header("Events Island")]
    [SerializeField] ShopEvent[] actOneShopsIsland = null;
    [SerializeField] ShopEvent[] actTwoShopsIsland = null;
    [SerializeField] ShopEvent[] actThreeShopsIsland = null;

    [Header("Treasure Events - Island")]
    [SerializeField] NonCombatEvent[] actOneTreasuresIsland = null;
    [SerializeField] NonCombatEvent[] actTwoTreasuresIsland = null;
    [SerializeField] NonCombatEvent[] actThreeTreasuresIsland = null;

    [Header("Taverns - Island")]
    [SerializeField] TavernEvent[] actOneTaverns = null;
    [SerializeField] TavernEvent[] actTwoTaverns = null;
    [SerializeField] TavernEvent[] actThreeTaverns = null;

    [Header("Taverns + Shipyard - Island")]
    [SerializeField] TavernEvent[] actOneTavernShipyards = null;
    [SerializeField] TavernEvent[] actTwoTavernShipyards = null;
    [SerializeField] TavernEvent[] actThreeTavernShipyards = null;

    [Header("Default Events - Island")]
    [SerializeField] NonCombatEvent[] actOneDefaultEventsIsland = null;
    [SerializeField] NonCombatEvent[] actTwoDefaultEventsIsland = null;
    [SerializeField] NonCombatEvent[] actThreeDefaultEventsIsland = null;

    [Header("Boss encounters")]
    [SerializeField] CombatEvent actOneBoss = null;
    [SerializeField] CombatEvent actTwoBoss = null;
    [SerializeField] CombatEvent actThreeBoss = null;

    [Header("Scene Change")]
    [SerializeField] SceneTransition sceneTransition = null;

    enum Location { Ocean, Island }

    private void Start()
    {
        MapMovement.OnOceanMove += GenerateOceanEvent;
        MapMovement.OnLocationArrived += GenerateLocationEvent;
        if (oceanFightChance + oceanTreasureChance + oceanDefaultEventChance > 1)
        {
            Debug.LogWarning("Ocean probabilities exceeds 100%");
        }
        if (islandShopChance + islandTreasureChance + islandTavernChance > 1)
        {
            Debug.LogWarning("Island probabilities exceeds 100%");
        }
    }

    private void OnDestroy()
    {
        MapMovement.OnOceanMove -= GenerateOceanEvent;
        MapMovement.OnLocationArrived -= GenerateLocationEvent;
    }

    public LocationData.ArrivalEvent RandomizeIslandArrivalEvent()
    {
        float rng = Random.Range(0f, 1f);


        if (rng <= islandShopChance)
        {
            return LocationData.ArrivalEvent.Shop;
        }
        else if (rng <= islandShopChance + islandTreasureChance)
        {
            return LocationData.ArrivalEvent.Treasure;
        }
        else if (rng <= islandShopChance + islandTreasureChance + islandTavernChance)
        {
            float rng2 = Random.Range(0f, 1f);

            if (rng2 <= islandshipyardChance)
            {
                return LocationData.ArrivalEvent.Shipyard;
            }
            else
            {
                return LocationData.ArrivalEvent.Tavern;
            }
        }
        else
        {
            return LocationData.ArrivalEvent.Event;
        }
    }

    private void GenerateOceanEvent(int mapSection)
    {
        Debug.Log("Checking for ocean event");

        float rng = Random.Range(0f, 1f);
        Debug.Log("Chance rng:" + rng);



        if (rng <= oceanFightChance)
        {
            Map.activeMap.SaveMap();
            MapMovement.instance.StopAutomaticMove();
            FightHandler(mapSection);
        }
        else if (rng <= oceanFightChance + oceanTreasureChance)
        {
            MapMovement.instance.StopAutomaticMove();
            TreasureHandler(Location.Ocean, mapSection);
        }
        else if (rng <= oceanFightChance + oceanTreasureChance + oceanDefaultEventChance)
        {
            MapMovement.instance.StopAutomaticMove();
            EventHandler(Location.Ocean, mapSection);
        }
        else
        {
            Debug.Log("Open Sea");
        }
    }

    public void GenerateLocationEvent(int revealedSections, LocationData.ArrivalEvent arrivalEvent)
    {
        switch (arrivalEvent)
        {
            case LocationData.ArrivalEvent.Nothing:
                break;
            case LocationData.ArrivalEvent.BossCombat:
                MapMovement.instance.StopAutomaticMove();
                BossHandler(revealedSections);
                break;
            case LocationData.ArrivalEvent.Shop:
                MapMovement.instance.StopAutomaticMove();
                ShopHandler(revealedSections);
                break;
            case LocationData.ArrivalEvent.Event:
                MapMovement.instance.StopAutomaticMove();
                EventHandler(Location.Island, revealedSections);
                break;
            case LocationData.ArrivalEvent.Treasure:
                MapMovement.instance.StopAutomaticMove();
                TreasureHandler(Location.Island, revealedSections);
                break;
            case LocationData.ArrivalEvent.Tavern:
                MapMovement.instance.StopAutomaticMove();
                TavernHandler(false, revealedSections);
                break;
            case LocationData.ArrivalEvent.Shipyard:
                MapMovement.instance.StopAutomaticMove();
                TavernHandler(true, revealedSections);
                break;
        }
    }

    #region EventHandlers
    private void FightHandler(int mapIndex)
    {
        CombatEvent encounter = null;
        if (mapIndex == 0)
        {
            encounter = actOneBattlesOcean[Random.Range(0, actOneBattlesOcean.Length)];
        }
        else if (mapIndex == 1)
        {
            encounter = actTwoBattlesOcean[Random.Range(0, actTwoBattlesOcean.Length)];
        }
        else
        {
            encounter = actThreeBattlesOcean[Random.Range(0, actTwoBattlesOcean.Length)];
        }
        Debug.Log("A Fight on the ocean");

        if (encounter != null)
        {
            PlayerSession.instance.LogEvent(SessionStats.EventType.Battle, encounter.name);
            PlayerSession.instance.nextEncounter = encounter;
            sceneTransition.ChangeScene();
        }
        else
        {
            Debug.LogWarning("No event found");
        }
    }

    private void ShopHandler(int mapIndex)
    {
        ShopEvent shopEvent = null;

        if (mapIndex == 0)
        {
            shopEvent = Utility.ReturnRandom(actOneShopsIsland);
        }
        else if (mapIndex == 1)
        {
            shopEvent = Utility.ReturnRandom(actTwoShopsIsland);
        }
        else
        {
            shopEvent = Utility.ReturnRandom(actThreeShopsIsland);
        }

        Debug.Log("A Shop");
        if (shopEvent != null)
        {
            Debug.Log("Starting event " + shopEvent.name);
            PlayerSession.instance.LogEvent(SessionStats.EventType.Shop, shopEvent.name);
            shopEvent.StartMyEvent();
        }
        else
        {
            Debug.LogWarning("No Shop found");
        }
    }

    private void TavernHandler(bool shipyard, int mapIndex)
    {
        TavernEvent tavernEvent = null;
        if (shipyard)
        {
            if (mapIndex == 0)
            {
                tavernEvent = Utility.ReturnRandom(actOneTavernShipyards);
            }
            else if (mapIndex == 1)
            {
                tavernEvent = Utility.ReturnRandom(actTwoTavernShipyards);
            }
            else
            {
                tavernEvent = Utility.ReturnRandom(actThreeTavernShipyards);
            }
            Debug.Log("Tavern " + "Shipyard");
        }
        else
        {
            if (mapIndex == 0)
            {
                tavernEvent = Utility.ReturnRandom(actOneTaverns);
            }
            else if (mapIndex == 1)
            {
                tavernEvent = Utility.ReturnRandom(actTwoTaverns);
            }
            else
            {
                tavernEvent = Utility.ReturnRandom(actThreeTaverns);
            }
            Debug.Log("A Tavern");
        }

        if (tavernEvent != null)
        {
            Debug.Log("Starting event " + tavernEvent.name);
            PlayerSession.instance.LogEvent(SessionStats.EventType.Tavern, tavernEvent.name);
            TavernDisplay.instance.StartTavernEvent(tavernEvent);
        }
        else
        {
            Debug.LogWarning("No event found");
        }
    }

    private void TreasureHandler(Location location, int mapIndex)
    {
        NonCombatEvent nonCombatEvent = null;
        switch (location)
        {
            case Location.Ocean:
                if (mapIndex == 0)
                {
                    nonCombatEvent = Utility.ReturnRandom(actOneTreasuresOcean);
                }
                else if (mapIndex == 1)
                {
                    nonCombatEvent = Utility.ReturnRandom(actTwoTreasuresOcean);
                }
                else
                {
                    nonCombatEvent = Utility.ReturnRandom(actThreeTreasuresOcean);
                }
                if (nonCombatEvent != null)
                {
                    PlayerSession.instance.LogEvent(SessionStats.EventType.OceanTreasure, nonCombatEvent.name);
                }
                Debug.Log("Ocean treasure");
                break;
            case Location.Island:
                if (mapIndex == 0)
                {
                    nonCombatEvent = Utility.ReturnRandom(actOneTreasuresIsland);
                }
                else if (mapIndex == 1)
                {
                    nonCombatEvent = Utility.ReturnRandom(actTwoTreasuresIsland);
                }
                else
                {
                    nonCombatEvent = Utility.ReturnRandom(actThreeTreasuresIsland);
                }
                if (nonCombatEvent != null)
                {
                    PlayerSession.instance.LogEvent(SessionStats.EventType.LandTreasure, nonCombatEvent.name);
                }
                Debug.Log("Island treasure");
                break;
        }
        if (nonCombatEvent != null)
        {
            Debug.Log("Starting event " + nonCombatEvent.name);
            nonCombatEvent.SendMyEvent();
        }
        else
        {
            Debug.LogWarning("No event found");
        }
    }

    private void EventHandler(Location location, int mapIndex)
    {
        NonCombatEvent nonCombatEvent = null;

        switch (location)
        {
            case Location.Ocean:

                if (mapIndex == 0)
                {
                    nonCombatEvent = Utility.ReturnRandom(actOneDefaultEventsOcean);
                }
                else if (mapIndex == 1)
                {
                    nonCombatEvent = Utility.ReturnRandom(actTwoDefaultEventsOcean);
                }
                else
                {
                    nonCombatEvent = Utility.ReturnRandom(actThreeDefaultEventsOcean);
                }

                if (nonCombatEvent != null)
                {
                    PlayerSession.instance.LogEvent(SessionStats.EventType.OceanEvent, nonCombatEvent.name);
                }

                Debug.Log("An ocean Event");
                break;
            case Location.Island:
                if (mapIndex == 0)
                {
                    nonCombatEvent = Utility.ReturnRandom(actOneDefaultEventsIsland);
                }
                else if (mapIndex == 1)
                {
                    nonCombatEvent = Utility.ReturnRandom(actTwoDefaultEventsIsland);
                }
                else
                {
                    nonCombatEvent = Utility.ReturnRandom(actThreeDefaultEventsIsland);
                }

                if (nonCombatEvent != null)
                {
                    PlayerSession.instance.LogEvent(SessionStats.EventType.LandEvent, nonCombatEvent.name);
                }

                Debug.Log("An island Event");
                break;
        }

        if (nonCombatEvent != null)
        {
            Debug.Log("Starting event " + nonCombatEvent.name);
            nonCombatEvent.SendMyEvent();
        }
        else
        {
            Debug.LogWarning("No event found");
        }
    }

    private void BossHandler(int revealedSections)
    {
        CombatEvent encounter = null;

        Debug.Log("Boss: " + revealedSections.ToString() + " being loaded");

        if (revealedSections == 1)
        {
            encounter = actOneBoss;
        }
        else if (revealedSections == 2)
        {
            encounter = actTwoBoss;
        }
        else if (revealedSections == 3)
        {
            encounter = actThreeBoss;
        }
        else
        {
            Debug.LogWarning("No boss implemented for section 4");
        }

        if (encounter != null)
        {
            PlayerSession.instance.LogEvent(SessionStats.EventType.Battle, encounter.name);
            PlayerSession.instance.nextEncounter = encounter;
            sceneTransition.ChangeScene();
        }
        else
        {
            Debug.LogWarning("No boss event found");
        }
    }
    #endregion
}