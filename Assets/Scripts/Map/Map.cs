using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Randomly generates and connects islands on the mapview

public class Map : MonoBehaviour
{
    private enum Direction { Right, UpRight, DownRight, MAX }

    public static Map activeMap;

    [Header("Map generation")]
    [SerializeField] float squareSize = 6f;
    [SerializeField] int gridXSize = 30;
    [SerializeField] int gridYSize = 5;
    [SerializeField] int[] xEndSections = new int[] { 7, 17, 30 };

    [Range(0.01f, 0.99f)]
    [SerializeField] float spawnRateFactor = 0.75f;
    [Range(0f, 1f)]
    [SerializeField] float randomizedOffsetMaxVal = 0.25f;

    [SerializeField] float encounterDistance = 1;

    LocationData[,] locations;
    public LocationData[,] Locations
    {
        get => locations;
    }

    static int locationID = 0;

    [Header("Prefabs/Blueprints")]
    [SerializeField] GameObject gridBoxBlueprint = null;

    [SerializeField] GameObject islandBlueprint = null;
    [SerializeField] Sprite[] islandVisuals = null;

    [SerializeField] GameObject startBlueprint = null;
    [SerializeField] GameObject boss1Blueprint = null;
    [SerializeField] GameObject boss2Blueprint = null;
    [SerializeField] GameObject boss3Blueprint = null;

    [SerializeField] Sprite randomIcon = null;
    [SerializeField] Sprite tavernIcon = null;
    [SerializeField] Sprite shipyardIcon = null;
    [SerializeField] Sprite shopIcon = null;

    [SerializeField] string randomTooltip = "Unknown location";
    [SerializeField] string tavernTooltip = "Island with a tavern";
    [SerializeField] string shipyardTooltip = "Island with a shipyard";
    [SerializeField] string shopTooltip = "Island with a shop";
    [SerializeField] string bossTooltip = "Boss encounter";


    [Header("Parenting in hirarchy")]
    [SerializeField] Transform gridParent = null;
    [SerializeField] Transform locationParent = null;
    [SerializeField] Transform encounterMapObjectsParent = null;
    [SerializeField] Transform markerParent = null;


    [Header("Misc")]
    [SerializeField] GameObject encounterHolder = null;
    [SerializeField] GameObject marker = null;


    [SerializeField] GameObject[] sectionBlockers = new GameObject[2];

    [SerializeField] MapEventSystem mapEventRandomizer = null;

    LocationData start;

    int revealedSections = 0;
    public int RevealedSections
    {
        get => revealedSections;
    }

    public void RevealSections(int numOfSections, bool withFade)
    {
        revealedSections += numOfSections;
        for (int i = 0; i < revealedSections; i++)
        {
            if (i < sectionBlockers.Length)
            {
                Disolve disolve = sectionBlockers[i].GetComponent<Disolve>();
                if (withFade && disolve != null)
                {
                    disolve.DoEffect();
                }
                else
                {
                sectionBlockers[i].SetActive(false);
                }
            }
        }
    }

    private void Start()
    {
        GenerateGrid();
        activeMap = this;

        if (!LoadMap()) //If no map is found, generate a new one
        {
            GenerateIslands();
            MapMovement.instance.SetLastVisitedLocation(start);
            MapMovement.instance.MoveShipAndCameraToPosition(start.Position);
        }
    }

    //Generates a grid visually and also sets up the array of positions of which we fill islands
    private void GenerateGrid()
    {
        for (int y = 0; y < gridYSize; y++)
        {
            for (int x = 0; x < gridXSize; x++)
            {
                GameObject obj = Instantiate(gridBoxBlueprint, GetMapPosition(new Coordinate(x, y)), Quaternion.identity, gridParent);
                obj.transform.localScale = new Vector3(squareSize, squareSize, squareSize);
            }
        }
        locations = new LocationData[gridXSize, gridYSize];
    }

    List<Direction> testedDirections = new List<Direction>();
    private void GenerateIslands()
    {
        LocationData previousLocation = null;
        LocationData newLocation = null;
        Queue<LocationData> islandsWithNoExits = new Queue<LocationData>();

        //Start location
        newLocation = SpawnLocation(new Coordinate(0, Mathf.CeilToInt(gridYSize / 2)), LocationData.LocationType.Start, LocationData.ArrivalEvent.Nothing);
        islandsWithNoExits.Enqueue(newLocation);
        start = newLocation;

        //Spawn main map part in sections
        for (int i = 0; i < xEndSections.Length; i++)
        {
            int spawnChance = 100;
            int rng = Random.Range(0, 100);

            //Spawn sections
            while (islandsWithNoExits.Count > 0)
            {
                previousLocation = islandsWithNoExits.Dequeue();

                if (previousLocation.coordinate.x == xEndSections[i] - 2) //If we have reached the 2nd last position in the grid on the X-axis
                {
                    islandsWithNoExits.Enqueue(previousLocation);
                    break;
                }

                spawnChance = 100;
                rng = 0;
                testedDirections.Clear();

                while (rng <= spawnChance && testedDirections.Count < 3) //While rng is lower than spawnchance, and 3 directions has not yet been tried ((all directions))
                {
                    Direction direction = (Direction)Random.Range(0, (int)Direction.MAX);

                    //If there is a island to the right of us, use it 
                    if (!IsCoordinateFree(new Coordinate(previousLocation.coordinate.x + 1, previousLocation.coordinate.y)))
                    {
                        direction = Direction.Right;
                    }

                    while (testedDirections.Contains(direction))
                    {
                        direction = (Direction)Random.Range(0, (int)Direction.MAX);
                    }
                    testedDirections.Add(direction);

                    if (AddLocationOrPathToExisting(previousLocation, direction, out LocationData foundLocation, LocationData.LocationType.Island, mapEventRandomizer.RandomizeIslandArrivalEvent()))
                    {
                        if (foundLocation.paths.Count < 1)
                        {
                            islandsWithNoExits.Enqueue(foundLocation);
                        }
                        rng = Random.Range(0, 100);
                        spawnChance = Mathf.RoundToInt(spawnChance * spawnRateFactor);
                    }
                    else
                    {
                        Debug.Log("Found no suitable location to the " + direction.ToString() + " on coordinate " + previousLocation.coordinate.x + ":" + previousLocation.coordinate.y);
                    }

                    //Always spawn at least 2 choices at the first part of each section
                    if (previousLocation.typeOfLocation == LocationData.LocationType.Start || previousLocation.typeOfLocation == LocationData.LocationType.Boss1
                        || previousLocation.typeOfLocation == LocationData.LocationType.Boss2 || previousLocation.typeOfLocation == LocationData.LocationType.Boss3)
                    {
                        if (previousLocation.paths.Count < 2)
                        {
                            rng = 0;
                        }
                    }
                }
            }


            LocationData.LocationType bossType = LocationData.LocationType.Boss1;
            if (i == 0)
            {
                bossType = LocationData.LocationType.Boss1;
            }
            else if (i == 1)
            {
                bossType = LocationData.LocationType.Boss2;
            }
            else if (i == 2)
            {
                bossType = LocationData.LocationType.Boss3;
            }
            else
            {
                Debug.LogWarning("Secion has no boss setup");
            }
            //Spawn section boss
            newLocation = SpawnLocation(new Coordinate(xEndSections[i] - 1, Mathf.CeilToInt(gridYSize / 2)), bossType, LocationData.ArrivalEvent.BossCombat);

            //Connect loose ends to exit
            while (islandsWithNoExits.Count > 0)
            {
                previousLocation = islandsWithNoExits.Dequeue();
                CreateRoute(previousLocation, newLocation);
            }

            //Add boss to list of locations to add new connections from
            islandsWithNoExits.Enqueue(newLocation);
        }
    }

    /// <summary>
    /// Creates a new location or a connection to an existing location
    /// Returns true if direction was inside the bounds of the map, returns false if not
    /// Outs the new location, or the found existing one
    /// </summary>
    private bool AddLocationOrPathToExisting(LocationData previousLocation, Direction directionToNewLocation, out LocationData foundLocation, LocationData.LocationType locationType, LocationData.ArrivalEvent arrivalEvent)
    {
        Coordinate newCoordinates = null;
        switch (directionToNewLocation)
        {
            case Direction.Right:
                newCoordinates = new Coordinate(previousLocation.coordinate.x + 1, previousLocation.coordinate.y);
                break;
            case Direction.UpRight:
                newCoordinates = new Coordinate(previousLocation.coordinate.x + 1, previousLocation.coordinate.y + 1);
                break;
            case Direction.DownRight:
                newCoordinates = new Coordinate(previousLocation.coordinate.x + 1, previousLocation.coordinate.y - 1);
                break;
            case Direction.MAX:
                Debug.LogError("Wrong direction");
                break;
        }

        if (IsCoordinateWithinBounds(newCoordinates))
        {
            if (IsCoordinateFree(newCoordinates))
            {
                foundLocation = SpawnLocation(newCoordinates, locationType, arrivalEvent);
                CreateRoute(previousLocation, foundLocation);
            }
            else
            {
                CreateRoute(previousLocation, locations[newCoordinates.x, newCoordinates.y]);
                foundLocation = locations[newCoordinates.x, newCoordinates.y];
            }
            return true;
        }
        else
        {
            foundLocation = null;
            return false;
        }
    }

    /// <summary>
    /// Returns true if the coordinates are within the bounds of our grid
    /// </summary>
    private bool IsCoordinateWithinBounds(Coordinate coordinate)
    {
        return !(coordinate.x < 0 || coordinate.x >= gridXSize || coordinate.y < 0 || coordinate.y >= gridYSize);
    }

    /// <summary>
    /// Check whether or not the square in our grid already has an island on it
    /// </summary>
    private bool IsCoordinateFree(Coordinate coordinate)
    {
        return (locations[coordinate.x, coordinate.y] == null);
    }


    //Spawns and sets up a location
    private LocationData SpawnLocation(Coordinate coordinate, LocationData.LocationType typeOfLocation, LocationData.ArrivalEvent eventAtArrival)
    {
        GameObject spawnBlueprint = null;
        Sprite icon = null;
        string hooverTooltip = "";
        switch (typeOfLocation)
        {
            case LocationData.LocationType.Start:
                spawnBlueprint = startBlueprint;
                break;
            case LocationData.LocationType.Island:
                spawnBlueprint = islandBlueprint;
                break;
            case LocationData.LocationType.Boss1:
                spawnBlueprint = boss1Blueprint;
                break;
            case LocationData.LocationType.Boss2:
                spawnBlueprint = boss2Blueprint;
                break;
            case LocationData.LocationType.Boss3:
                spawnBlueprint = boss3Blueprint;
                break;
        }

        switch (eventAtArrival)
        {
            case LocationData.ArrivalEvent.Nothing:
                hooverTooltip = "";
                break;
            case LocationData.ArrivalEvent.BossCombat:
                hooverTooltip = bossTooltip;
                break;
            case LocationData.ArrivalEvent.Shop:
                icon = shopIcon;
                hooverTooltip = shopTooltip;
                break;
            case LocationData.ArrivalEvent.Event:
            case LocationData.ArrivalEvent.Treasure:
                icon = randomIcon;
                hooverTooltip = randomTooltip;
                break;
            case LocationData.ArrivalEvent.Tavern:
                icon = tavernIcon;
                hooverTooltip = tavernTooltip;
                break;
            case LocationData.ArrivalEvent.Shipyard:
                icon = shipyardIcon;
                hooverTooltip = shipyardTooltip;
                break;
        }

        Location locationObject = Instantiate(spawnBlueprint, GetMapPosition(coordinate) + GetRandomizedOffset(), Quaternion.identity, locationParent).GetComponent<Location>();
        LocationData createdLocation = locationObject.LocationData;
        createdLocation.typeOfLocation = typeOfLocation;
        createdLocation.eventAtArrival = eventAtArrival;
        createdLocation.coordinate = coordinate;
        createdLocation.locationID = locationID++;
        if (typeOfLocation == LocationData.LocationType.Island)
        {
            createdLocation.spriteIndex = Random.Range(0, islandVisuals.Length);
            locationObject.SetupLocation(islandVisuals[createdLocation.spriteIndex], icon, hooverTooltip);
        }
        else
        {
            locationObject.SetTooltipDescription(hooverTooltip);
        }
        return locations[coordinate.x, coordinate.y] = createdLocation;
    }

    //Reloades a saved location and spawns it into the world
    private void SpawnLocation(LocationData loadedLocation)
    {
        GameObject spawnBlueprint = null;
        Sprite icon = null;
        string hooverTooltip = "";
        switch (loadedLocation.typeOfLocation)
        {
            case LocationData.LocationType.Start:
                spawnBlueprint = startBlueprint;
                break;
            case LocationData.LocationType.Island:
                spawnBlueprint = islandBlueprint;
                break;
            case LocationData.LocationType.Boss1:
                spawnBlueprint = boss1Blueprint;
                break;
            case LocationData.LocationType.Boss2:
                spawnBlueprint = boss2Blueprint;
                break;
            case LocationData.LocationType.Boss3:
                spawnBlueprint = boss3Blueprint;
                break;
        }

        switch (loadedLocation.eventAtArrival)
        {
            case LocationData.ArrivalEvent.Nothing:
                hooverTooltip = "";
                break;
            case LocationData.ArrivalEvent.BossCombat:
                hooverTooltip = bossTooltip;
                break;
            case LocationData.ArrivalEvent.Shop:
                icon = shopIcon;
                hooverTooltip = shopTooltip;
                break;
            case LocationData.ArrivalEvent.Event:
            case LocationData.ArrivalEvent.Treasure:
                icon = randomIcon;
                hooverTooltip = randomTooltip;
                break;
            case LocationData.ArrivalEvent.Tavern:
                icon = tavernIcon;
                hooverTooltip = tavernTooltip;
                break;
            case LocationData.ArrivalEvent.Shipyard:
                icon = shipyardIcon;
                hooverTooltip = shipyardTooltip;
                break;
        }

        Location locationObject = Instantiate(spawnBlueprint, new Vector3(loadedLocation.Position.x, loadedLocation.Position.y), Quaternion.identity, locationParent).GetComponent<Location>();
        locationObject.LocationData = loadedLocation;
        if (loadedLocation.typeOfLocation == LocationData.LocationType.Island)
        {
            locationObject.SetupLocation(islandVisuals[loadedLocation.spriteIndex], icon, hooverTooltip);
        }
        else
        {
            locationObject.SetTooltipDescription(hooverTooltip);
        }
    }


    //Returns an offset
    private Vector2 GetRandomizedOffset()
    {
        float max = squareSize * randomizedOffsetMaxVal;
        return new Vector2(Random.Range(-max, max), Random.Range(-max, max));
    }

    //Returns the exact position of a coordinate in our grid
    private Vector2 GetMapPosition(Coordinate coordinate)
    {
        return new Vector2((coordinate.x - gridXSize / 2) * squareSize, (coordinate.y - gridYSize / 2) * squareSize);
    }

    /// <summary>
    /// Create and connect the locations together
    /// </summary>
    private void CreateRoute(LocationData previousIsland, LocationData newIsland)
    {
        if (previousIsland.ContainsPathToLocation(newIsland))
        {
            return;
        }


        float distance = Vector2.Distance(previousIsland.Position, newIsland.Position);

        //Encounter-positions
        int possibleEncounters = Mathf.FloorToInt(distance / encounterDistance);
        Vector2[] path = new Vector2[possibleEncounters + 1];

        float spawnDistance = distance / (path.Length + 1);
        Vector2 pos = previousIsland.Position;
        Vector2 dir = (newIsland.Position - previousIsland.Position).normalized * spawnDistance;

        for (int i = 0; i < path.Length; i++)
        {
            pos += dir;
            path[i] = pos;
            Instantiate(encounterHolder, path[i], Quaternion.identity, encounterMapObjectsParent);
        }

        //Markers
        float markersDistance = encounterDistance / 4;
        int markerLines = Mathf.FloorToInt(distance / markersDistance);

        pos = previousIsland.Position;
        dir = (newIsland.Position - previousIsland.Position).normalized * markersDistance;

        for (int i = 0; i < markerLines; i++)
        {
            pos += dir;
            GameObject obj = Instantiate(marker, pos, Quaternion.identity, markerParent);
            //Set rotation towards newisland.position
            obj.transform.right = (newIsland.Position - previousIsland.Position);
        }

        previousIsland.AddPath(path, newIsland);
    }

    /// <summary>
    /// Create the route visual objects
    /// </summary>
    private void SpawnRouteVisuals(Path path, Vector2 startpos, Vector2 endpos)
    {
        for (int i = 0; i < path.travelPoints.GetLength(0); i++)
        {
            Instantiate(encounterHolder, path.GetPositionOfIndex(i), Quaternion.identity, encounterMapObjectsParent);
        }

        float distance = Vector2.Distance(startpos, endpos);

        //Markers
        float markersDistance = encounterDistance / 4;
        int markerLines = Mathf.FloorToInt(distance / markersDistance);

        Vector2 pos = startpos;
        Vector2 dir = (endpos - startpos).normalized * markersDistance;

        for (int i = 0; i < markerLines; i++)
        {
            pos += dir;
            GameObject obj = Instantiate(marker, pos, Quaternion.identity, markerParent);
            //Set rotation towards newisland.position
            obj.transform.right = (endpos - startpos);
        }
    }

    public LocationData GetLocationPosition(int id)
    {
        for (int x = 0; x < locations.GetLength(0); x++)
        {
            for (int y = 0; y < locations.GetLength(1); y++)
            {
                if (locations[x, y] != null)
                {
                    if (locations[x, y].locationID == id)
                    {
                        return locations[x, y];
                    }
                }
            }
        }
        return null;
    }

    //Load the saved map
    private bool LoadMap()
    {
        MapData data = Utility.LoadMap();
        if (data != null && data.islands != null)
        {
            //Spawn locations
            List<LocationData> spawnedIslands = new List<LocationData>();
            for (int x = 0; x < data.islands.GetLength(0); x++)
            {
                for (int y = 0; y < data.islands.GetLength(1); y++)
                {
                    if (data.islands[x, y] != null)
                    {
                        SpawnLocation(data.islands[x, y]);
                        spawnedIslands.Add(data.islands[x, y]);
                    }
                }
            }
            locations = data.islands;

            //Spawn route objects
            foreach (var island in spawnedIslands)
            {
                foreach (var path in island.paths)
                {
                    SpawnRouteVisuals(path, island.Position, GetLocationPosition(path.endLocationID).Position);
                }
            }

            //Reload ship path etc.
            MapMovement.instance.ReloadSession(data);

            //Show sections revealed
            RevealSections(data.revealedSections, false);
            return true;
        }
        else
        {
            return false;
        }
    }


    //Save the active map (overwriting any previous saved)
    public void SaveMap()
    {
        Utility.SaveMap(this, MapMovement.instance);
    }
}

[System.Serializable]
public class MapData
{
    public int revealedSections;

    public LocationData[,] islands;

    public float[] playerPosition;

    public bool startedRoute;
    public int travelIndex;
    public Path activePath;
    public LocationData lastVisitedIsland;

    public MapData(Map map, MapMovement mapMovement)
    {
        revealedSections = map.RevealedSections;
        islands = map.Locations;
        playerPosition = new float[] { mapMovement.PlayerShipPosition.x, mapMovement.PlayerShipPosition.y };
        startedRoute = mapMovement.StartedRoute;
        travelIndex = mapMovement.TravelIndex;
        activePath = mapMovement.ActivePath;
        lastVisitedIsland = mapMovement.LastVisitedIsland;
    }
}
