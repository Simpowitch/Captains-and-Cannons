using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

//Simon Voss
//Responsible for handling the ship and movements on the map

public class MapMovement : MonoBehaviour
{
    #region Singleton
    public static MapMovement instance;
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

    const float MOVEINTERVALTIME = 0.75f; //in seconds
    [SerializeField] GameObject marker = null;
    [SerializeField] Text moveButtonText = null;
    [SerializeField] Button moveButton = null;
    [SerializeField] string moveAllowedText = "Set Sail!";
    [SerializeField] string moveInProgressText = "On route for adventure!";
    [SerializeField] string moveNotAllowedText = "Pick a destination on the map";
    [SerializeField] UISFXSender mapScribbleSound = null;
    
    [SerializeField] MapCamera mapCamera = null;

    [SerializeField] Transform ship = null;
    public Vector2 PlayerShipPosition
    {
        get => ship.position;
    }

    public delegate void OceanEventTrigger(int mapSection);
    public static OceanEventTrigger OnOceanMove;
    public delegate void LocationEventTrigger(int mapSection, LocationData.ArrivalEvent arrivalEvent);
    public static LocationEventTrigger OnLocationArrived;
    public delegate void RouteStart();
    public static RouteStart OnRouteStarted;

    bool startedRoute = false;
    public bool StartedRoute
    {
        get => startedRoute;
    }
    int travelIndex = 0;
    public int TravelIndex
    {
        get => travelIndex;
    }

    Path activePath;
    public Path ActivePath
    {
        get => activePath;
    }

    LocationData lastVisitedLocation;
    public LocationData LastVisitedIsland
    {
        get => lastVisitedLocation;
    }

    public void SetLastVisitedLocation(LocationData location)
    {
        lastVisitedLocation = location;
    }

    //Startfunction
    private void Start()
    {
        moveButton.interactable = ActivePath != null;
        moveButtonText.text = ActivePath != null ? moveAllowedText : moveNotAllowedText;
    }

    //Loads a saved session and sets path and ship placement etc.
    public void ReloadSession(MapData data)
    {
        startedRoute = data.startedRoute;
        MoveShipAndCameraToPosition(new Vector2(data.playerPosition[0], data.playerPosition[1]));
        travelIndex = data.travelIndex;
        activePath = data.activePath;
        lastVisitedLocation = data.lastVisitedIsland;
        if (activePath != null)
        {
            marker.SetActive(true);
            marker.transform.position = Map.activeMap.GetLocationPosition(activePath.endLocationID).Position;
        }
        moveButtonText.text = ActivePath != null ? moveAllowedText : moveNotAllowedText;
        moveButton.interactable = ActivePath != null;
    }

    public void MoveShipAndCameraToPosition(Vector2 pos)
    {
        ship.position = pos;
        mapCamera.SetCameraPosition(pos);
    }

    public bool IsPathAvailable(LocationData targetIsland, out string explanation)
    {
        if (startedRoute)
        {
            explanation = "A route is already in progress";
            return false;
        }
        if (!lastVisitedLocation.ContainsPathToLocation(targetIsland))
        {
            explanation = "No route to this island from current position";
            return false;
        }
        explanation = "";
        return true;
    }

    public bool ChoosePath(LocationData targetIsland, out string explanation)
    {
        if (IsPathAvailable(targetIsland, out explanation))
        {
            for (int i = 0; i < lastVisitedLocation.paths.Count; i++)
            {
                if (lastVisitedLocation.paths[i].endLocationID == targetIsland.locationID)
                {
                    activePath = lastVisitedLocation.paths[i];
                    break;
                }
            }
            marker.SetActive(true);
            marker.transform.position = targetIsland.Position;
            moveButton.interactable = ActivePath != null;
            moveButtonText.text = ActivePath != null ? moveAllowedText : moveNotAllowedText;
            mapScribbleSound.PlayCustom();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void MoveShip()
    {
        if (activePath == null)
        {
            Debug.Log("No path selected");
            return;
        }
        moveButtonText.text = moveInProgressText;
        allowAutoMove = true;
        moveButton.interactable = false;
        StartCoroutine(AutomaticMove());
        OnRouteStarted?.Invoke();
    }

    private void MovedOnOcean()
    {
        OnOceanMove?.Invoke(Map.activeMap.RevealedSections);
        StartCoroutine(AutomaticMove());
    }

    private void ArrivedAtLocation()
    {
        marker.SetActive(false);
        travelIndex = 0;
        lastVisitedLocation = Map.activeMap.GetLocationPosition(activePath.endLocationID);
        LocationData.ArrivalEvent arrivalEvent = lastVisitedLocation.eventAtArrival;
        switch (arrivalEvent)
        {
            case LocationData.ArrivalEvent.Nothing:
                break;
            case LocationData.ArrivalEvent.BossCombat:
                //Reveal next session
                Map.activeMap.RevealSections(1, true);
                Debug.Log("Arrived at boss");
                activePath = null;
                startedRoute = false;
                Map.activeMap.SaveMap();
                OnLocationArrived?.Invoke(Map.activeMap.RevealedSections, arrivalEvent);
                break;
            case LocationData.ArrivalEvent.Shop:
            case LocationData.ArrivalEvent.Event:
            case LocationData.ArrivalEvent.Treasure:
            case LocationData.ArrivalEvent.Tavern:
            case LocationData.ArrivalEvent.Shipyard:
                Debug.Log("Arrived at location");
                PlayerSession.instance.locationsVisited++;
                activePath = null;
                startedRoute = false;
                Map.activeMap.SaveMap();
                OnLocationArrived?.Invoke(Map.activeMap.RevealedSections, arrivalEvent);
                break;
        }
    }

    public IEnumerator AutomaticMove()
    {
        yield return new WaitForSeconds(MOVEINTERVALTIME);
        if (allowAutoMove)
        {
            if (startedRoute)
            {
                PlayerSession.instance.nodesPassed++;
                travelIndex++;
                if (travelIndex >= activePath.travelPoints.GetLength(0))
                {
                    ship.position = Map.activeMap.GetLocationPosition(activePath.endLocationID).Position;
                    ArrivedAtLocation();
                }
                else
                {
                    ship.position = activePath.GetPositionOfIndex(travelIndex);
                    MovedOnOcean();
                }
            }
            else
            {
                startedRoute = true;
                ship.position = activePath.GetPositionOfIndex(0);
                MovedOnOcean();
            }
        }
    }

    private bool allowAutoMove = false;
    public void StopAutomaticMove()
    {
        allowAutoMove = false;
        moveButtonText.text = ActivePath != null ? moveAllowedText : moveNotAllowedText;
        moveButton.interactable = ActivePath != null;
    }
}
