using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Simon Voss
//A location on the map

[RequireComponent(typeof(BoxCollider2D))]
public class Location : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    LocationData locationData = new LocationData();
    public LocationData LocationData
    {
        set => locationData = value;
        get => locationData;
    }

    [SerializeField] SpriteRenderer locationRenderer = null;
    [SerializeField] SpriteRenderer iconRenderer = null;

    [SerializeField] Material nonInteractableMaterial = null;
    [SerializeField] Material interactableMaterial = null;


    private void OnEnable()
    {
        locationData.position = new float[] { transform.position.x, transform.position.y };
    }

    private void Start()
    {
        SubscribeToDelegate(true);
        CheckIfInteractable();
    }

    private void SubscribeToDelegate(bool subsribe)
    {
        if (subsribe)
        {
            MapMovement.OnLocationArrived += CheckIfInteractable;
            MapMovement.OnRouteStarted += SetToNonInteractable;
        }
        else
        {
            MapMovement.OnLocationArrived -= CheckIfInteractable;
            MapMovement.OnRouteStarted -= SetToNonInteractable;
        }
    }

    private void OnDestroy()
    {
        SubscribeToDelegate(false);
    }

    string tooltipDescription = "";

    public void SetupLocation(Sprite locationSprite, Sprite iconSprite, string tooltipDescription)
    {
        locationRenderer.sprite = locationSprite;
        if (iconRenderer != null)
        {
            if (iconSprite != null)
            {
                iconRenderer.sprite = iconSprite;
            }
            else
            {
                iconRenderer.enabled = false;
            }
        }
        this.tooltipDescription = tooltipDescription;
    }

    public void SetTooltipDescription(string tooltipDescription)
    {
        this.tooltipDescription = tooltipDescription;
    }


    #region Materials for interactable
    private void SetToNonInteractable()
    {
        SetMaterial(false);
    }

    private void CheckIfInteractable(int mapSection, LocationData.ArrivalEvent arrivalEvent)
    {
        SetMaterial(MapMovement.instance.IsPathAvailable(this.locationData, out string explanation));
    }

    private void CheckIfInteractable()
    {
        SetMaterial(MapMovement.instance.IsPathAvailable(this.locationData, out string explanation));
    }

    private void SetMaterial(bool interactable)
    {
        locationRenderer.material = interactable ? interactableMaterial : nonInteractableMaterial;
    }
    #endregion

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Location clicked on");
        if (!MapMovement.instance.ChoosePath(this.locationData, out string explanation))
        {
            MouseTooltip.SetUpToolTip(MouseTooltip.ColorText.InvalidTarget, explanation);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MouseTooltip.SetUpToolTip(MouseTooltip.ColorText.Default, tooltipDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseTooltip.HideTooltip();
    }
}

[System.Serializable]
public class LocationData
{
    public enum ArrivalEvent { Nothing, BossCombat, Shop, Event, Treasure, Tavern, Shipyard }
    public ArrivalEvent eventAtArrival = ArrivalEvent.Nothing;
    public enum LocationType { Start, Island, Boss1, Boss2, Boss3 }
    public LocationType typeOfLocation;
    public float[] position = new float[2];
    public int locationID;
    public int spriteIndex;

    public Vector2 Position
    {
        get => new Vector2(position[0], position[1]);
    }
    public Coordinate coordinate;
    public List<Path> paths = new List<Path>();

    public bool ContainsPathToLocation(LocationData locationToCheck)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i].endLocationID == locationToCheck.locationID)
            {
                return true;
            }
        }
        return false;
    }

    public void AddPath(Vector2[] positionsOnPath, LocationData connectedIsland)
    {
        paths.Add(new Path(positionsOnPath, connectedIsland));
    }
}

[System.Serializable]
public class Path
{
    public float[,] travelPoints;
    public int endLocationID;

    public Path(Vector2[] positionsOnPath, LocationData connectedIsland)
    {
        travelPoints = new float[positionsOnPath.Length, 2];

        for (int i = 0; i < positionsOnPath.Length; i++)
        {
            travelPoints[i, 0] = positionsOnPath[i].x;
            travelPoints[i, 1] = positionsOnPath[i].y;
        }
        endLocationID = connectedIsland.locationID;
    }

    public Vector2 GetPositionOfIndex(int index)
    {
        if (index < travelPoints.GetLength(0))
        {
            return new Vector2(travelPoints[index, 0], travelPoints[index, 1]);
        }
        else
        {
            Debug.LogError("Index out of bounds");
            return Vector2.zero;
        }
    }
}

[System.Serializable]
public class Coordinate
{
    public int x;
    public int y;

    public Coordinate(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public Coordinate(Vector2Int vector)
    {
        x = vector.x;
        y = vector.y;
    }
}
