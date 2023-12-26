using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

//Simon Voss

//A tile on the battlefield

[RequireComponent(typeof(PolygonCollider2D))]
public class ShipTile : MonoBehaviour, IPointerClickHandler
{
    public Color tileAvailableColor = Color.green;
    public Color tileDefaultColor = Color.white;

    Character characterOnTile = null;

    [SerializeField] List<ShipTile> adjacentTiles = new List<ShipTile>();
    public List<ShipTile> AdjacentTiles
    {
        get => adjacentTiles;
    }

    [SerializeField] List<ShipTile> boardingTiles = new List<ShipTile>();
    public List<ShipTile> BoardingTiles
    {
        get => boardingTiles;
    }
    public List<ShipTile> AdjacentAndBoardingTiles
    {
        get
        {
            List<ShipTile> allTiles = new List<ShipTile>();
            allTiles.AddRange(AdjacentTiles);
            allTiles.AddRange(BoardingTiles);
            return allTiles;
        }
    }

    public Cannon cannon;

    bool interactable = false;
    public bool DisplayInteractable
    {
        get => interactable;
        set
        {
            GetComponent<SpriteRenderer>().color = value ? tileAvailableColor : tileDefaultColor;
            interactable = value;
        }
    }

    private void ReturnTileToNonInteractable(Character character)
    {
        if (DisplayInteractable)
        {
            DisplayInteractable = false;
        }
    }

    private void Start()
    {
        CombatDelegates.instance.OnSelectedCharacter += ReturnTileToNonInteractable;
        CombatDelegates.instance.OnCharacterMoved += ReturnTileToNonInteractable;
        CombatDelegates.instance.OnPreparedActionChanged += ReturnTileToNonInteractable;
    }

    #region CharactersOnTile
    public void MoveCharacterToTile(Character newCharacter)
    {
        if (characterOnTile == null)
        {
            characterOnTile = newCharacter;
        }
        else
        {
            Debug.LogError("Tried to move a character to an occupied space");
        }
    }

    public void MoveCharacterAwayFromTile(Character characterToRemove)
    {
        if (characterOnTile != null)
        {
            characterOnTile = null;
        }
    }

    public Character GetCharacterOnTile()
    {
        return characterOnTile;
    }

    public bool IsAvailable()
    {
        return characterOnTile == null;
    }

    public bool IsCaptainOnAdjacentTiles(Team team, out Captain captain)
    {
        foreach (ShipTile surroundingTile in AdjacentTiles)
        {
            Character character = surroundingTile.GetCharacterOnTile();
            if (character != null && character is Captain && character.MyTeam == team)
            {
                captain = character as Captain;
                return true;
            }
        }
        captain = null;
        return false;
    }

    public List<Character> GetAdjacentEnemies(Team myTeam)
    {
        List<Character> enemies = new List<Character>();
        foreach (ShipTile surroundingTile in AdjacentTiles)
        {
            Character character = surroundingTile.GetCharacterOnTile();
            if (character != null && character.MyTeam != myTeam && character.Alive)
            {
                enemies.Add(character);
            }
        }
        return enemies;
    }
    #endregion

    //Mouse handling
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                if (CombatManager.instance.GameState == CombatManager.State.PlayerTurn)
                {
                    if (CombatManager.instance.SelectedCharacter == characterOnTile)
                    {
                        CombatManager.instance.SelectedCharacter = null;
                    }
                    else
                    {
                        CombatManager.instance.SelectedCharacter = characterOnTile;
                    }
                }
                else
                {
                    Debug.Log("Not player turn");
                }
                break;
            case PointerEventData.InputButton.Right:
                if (interactable)
                {
                    if (CombatManager.instance.SelectedCharacter != null)
                    {
                        if (CombatManager.instance.GameState == CombatManager.State.PlayerTurn)
                        {
                            CombatManager.instance.SelectedCharacter.MoveCharacter(this);
                        }
                        else
                        {
                            Debug.Log("Not player turn");
                        }
                    }
                    else
                    {
                        Debug.Log("No character selected");
                    }
                }
                else
                {
                    CombatManager.instance.SetPendingAction(CombatAction.None);
                    Debug.Log("Not interactable");
                }
                break;
            case PointerEventData.InputButton.Middle:
                break;
        }
    }
}

public class TileMovement
{
    public ShipTile newTile;
    public int cost;

    public TileMovement(ShipTile _newTile, int _cost)
    {
        newTile = _newTile;
        cost = _cost;
    }


    public static bool ExistsInList(List<TileMovement> tileMovements, ShipTile tileToCheck)
    {
        foreach (var item in tileMovements)
        {
            if (item.newTile == tileToCheck)
            {
                return true;
            }
        }
        return false;
    }

    public static TileMovement GetTileMovementInList(List<TileMovement> tileMovements, ShipTile tileToCheck)
    {
        foreach (var item in tileMovements)
        {
            if (item.newTile == tileToCheck)
            {
                return item;
            }
        }
        return null;
    }

    public static List<TileMovement> SearchTileForPossibleMoves(int actionpoints, ShipTile tileToCheck, int costToThisPoint, ref List<TileMovement> possibleMoves)
    {
        //End if not enough points to move further
        if (costToThisPoint > actionpoints)
        {
            return possibleMoves;
        }

        //Search normal movement
        List<ShipTile> newlyAddedTiles = new List<ShipTile>();
        foreach (var newTile in tileToCheck.AdjacentTiles)
        {
            if (!TileMovement.ExistsInList(possibleMoves, newTile)) //Tile is new 
            {
                if (newTile.IsAvailable())
                {
                    newlyAddedTiles.Add(newTile);
                    possibleMoves.Add(new TileMovement(newTile, costToThisPoint));
                }
            }
            else
            {
                TileMovement movementToCheck = GetTileMovementInList(possibleMoves, newTile);
                if (movementToCheck != null && movementToCheck.cost > costToThisPoint) //If Tile is found and used and the cost is higher than the current cost
                {
                    movementToCheck.cost = costToThisPoint;
                    newlyAddedTiles.Add(newTile); //Re-search this tile for new options with the newly updated cost
                }
            }
        }

        //Search boarding movements
        foreach (var newTile in tileToCheck.BoardingTiles)
        {
            if (!TileMovement.ExistsInList(possibleMoves, newTile))
            {
                if (newTile.IsAvailable())
                {
                    newlyAddedTiles.Add(newTile);
                    possibleMoves.Add(new TileMovement(newTile, costToThisPoint));
                }
            }
            else
            {
                TileMovement movementToCheck = GetTileMovementInList(possibleMoves, newTile);
                if (movementToCheck != null && movementToCheck.cost > costToThisPoint) //If Tile is found and used and the cost is higher than the current cost
                {
                    movementToCheck.cost = costToThisPoint;
                    newlyAddedTiles.Add(newTile); //Re-search this tile for new options with the newly updated cost
                }
            }
        }

        //Add point and continue looking at added tiles -> adjacent tiles
        costToThisPoint++;
        foreach (var newTile in newlyAddedTiles)
        {
            SearchTileForPossibleMoves(actionpoints, newTile, costToThisPoint, ref possibleMoves);
        }

        return possibleMoves;
    }
}