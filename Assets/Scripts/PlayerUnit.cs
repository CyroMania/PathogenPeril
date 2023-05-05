using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The base class for pathogens the player controls.
/// </summary>
public abstract class PlayerUnit : Unit
{
    private const string DivideBtnName = "_divideBtn";
    private const string UnitLayer = "Unit";
    private const string TileLayer = "Tile";

    private bool _isMoving;
    private bool _selected;
    private Camera _mainCamera;
    private Collider2D _collider;
    private Stack<Tile> _path;

    /// <summary>
    /// True if the player unit is selected by the player, otherwise false.
    /// </summary>
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            if (_selected)
            {
                //Ensures that only ever one unit is selected
                DeselectOtherUnits();
            }
        }
    }

    /// <summary>
    /// True when the unit is in motion, otherwise false.
    /// </summary>
    public bool IsMoving
    {
        get => _isMoving;
        set => _isMoving = value;
    }

    /// <summary>
    /// True if the unit is dead and is about to be destroyed, otherwise false.
    /// Used to prevent interactions from external influences.
    /// </summary>
    public bool IsDead { get; internal set; }

    /// <summary>
    /// True if the unit is a copy of another unit.
    /// </summary>
    protected bool Clone { get; set; } = false;

    /// <summary>
    /// True if the unit has divided this turn to create a copy of itself.
    /// </summary>
    protected bool JustDivided { get; set; }

    private void Start()
    {
        //We need to set the currentTile the moment the unit is active.
        CurrentTile = TileMovement.CalculateCurrentTile(this);
        BeginTurn = true;
    }

    private void Update()
    {
        if (IsPlayerTurn && !IsDead)
        {
            if (BeginTurn)
            {
                if (JustDivided)
                {
                    //This provides a limitation to the divide mechanic.
                    MovementPoints = (short)(MaxMovementPoints - 1);
                    JustDivided = false;
                }
                else
                {
                    ResetUnit();
                }

                //These unit ui methods require other dependencies so for testing we have kept them behind this variable.
                if (!UnitTesting)
                {
                    UnitUI.UpdateStatBarValue(this, "Energy");
                    UnitUI.UpdateStatBarPositions(this, _mainCamera.WorldToScreenPoint(gameObject.transform.position));
                }

                TileMovement.FindVisibleTiles(CurrentTile, new Queue<Tile>(), Visibility);

                //This rehighlights the selected unit's associated tiles in case they were reset.
                if (Selected)
                {
                    CurrentTile.Current = true;
                    UI.CheckButtonsUsable(MovementPoints, MaxMovementPoints, IsFullySurrounded());
                    FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                }

                BeginTurn = false;
            }

            //If we have a target tile we need to move to it.
            //This if block handles the path finding for a given player unit.
            if (TargetTile != null)
            {
                if (!_isMoving)
                {
                    _path = TileMovement.FindTilePath(CurrentTile, TargetTile, new Stack<Tile>(), MovementPoints);
                    //Subtract the number of tiles in the path because each tile is worth one movement point.
                    MovementPoints -= (short)_path.Count;
                    _isMoving = true;
                    return;
                }
                else
                {
                    TileMovement.MoveToTile(this, _path);

                    if (_path.Count == 0)
                    {
                        _isMoving = false;
                        SetTargetTileToCurrentTile();

                        if (CurrentTile.Goal)
                        {
                            //We must despawn the unit if they are on a goal tile and increment the score.
                            UI.UpdateScoreText(1);
                            Kill();
                            //We need to reset all tiles so they're visible tiles are reset.
                            ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Goal) });
                            FindAllVisibleTiles();
                            //This returns because the unit is destroyed so no more logic involving their state needs to take place.
                            return;
                        }

                        if (Selected)
                        {
                            //This rehighlights all tiles that are still reachable after they stop moving.
                            ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Current), nameof(Tile.Goal) });
                            FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                            //This checks if the divide button is still usable.
                            UI.CheckButtonsUsable(MovementPoints, MaxMovementPoints, IsFullySurrounded());
                        }
                        else
                        {
                            ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Goal) });
                        }

                        FindAllVisibleTiles();
                    }

                    return;
                }
            }

            if (Input.GetMouseButtonDown(0) && ConfirmNoOtherUnitMoving())
            {
                //This ensures that we are not colliding with a different object on mouse click, such as a hidden UI element.
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Vector2 clickPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

                if (!_selected)
                {
                    //Tiles are irrelevant for selection unless we have a unit selected so we check for units first.
                    RaycastHit2D unitHitInfo = PhysicsHelper.GenerateRaycast(UnitLayer, clickPosition);

                    //Checks that we actually hit this unit's collider after clicking the screen.
                    if (unitHitInfo.collider == _collider)
                    {
                        Selected = true;
                        //Show the one move the bacteria can make.
                        UI.DisplayButton(DivideBtnName, true);
                        ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible), nameof(Tile.Goal) });
                        //Make their current tile stand out.
                        CurrentTile.Current = true;
                        //Update tiles to see where this unit can move.
                        FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                        UI.CheckButtonsUsable(MovementPoints, MaxMovementPoints, IsFullySurrounded());
                    }

                    //We return here because this click was for a unit selection.
                    return;
                }

                //We now raycast tiles because we've confirmed that units are not relevant to this selection.
                RaycastHit2D tileHitInfo = PhysicsHelper.GenerateRaycast(TileLayer, clickPosition);

                if (tileHitInfo.collider != null)
                {
                    Tile selectedTile = tileHitInfo.collider.gameObject.GetComponent<Tile>();

                    //We can't handle the case for the currently selected unit in the previous raycast.
                    //This returns if we are clicking the current player unit because we already have them selected.
                    if (selectedTile.Current)
                    {
                        return;
                    }
                    //This case handles any reachable tile that is available and then sets it as a target.
                    else if (selectedTile.Reachable && !selectedTile.Inhabited)
                    {
                        TargetTile = selectedTile;
                        return;
                    }
                    //This case deselects the unit if they click on an invisible tile.
                    //Also hides the attack buttons in the UI.
                    else if (!selectedTile.Reachable && !selectedTile.Visible)
                    {
                        Selected = false;
                        CurrentTile.Current = false;
                        UI.DisplayButton(DivideBtnName, false);
                        ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible), nameof(Tile.Goal) });
                    }
                }
            }
        }
    }

    /// <summary>
    /// Destroys this player unit and removes all references to it.
    /// </summary>
    public void Kill()
    {
        GameObject.Find("Canvas").GetComponent<UnitUI>().DestroyStatBars(this);

        foreach (ImmuneCell cell in ImmuneCells)
        {
            if (cell.TargetUnit == this)
            {
                cell.TargetUnit = null;
            }
        }

        if (gameObject != null)
        {
            Destroy(gameObject);
        }

        PlayerUnits.Remove(this);

        if (Selected)
        {
            CurrentTile.Current = false;
        }

        CurrentTile = null;
        CheckNoPlayerUnitsAlive();
    }

    /// <summary>
    /// Deselects all player units and hides the attack buttons.
    /// </summary>
    public static void DeselectAllUnits()
    {
        foreach (PlayerUnit unit in PlayerUnits)
        {
            if (unit.Selected)
            {
                unit.Selected = false;
                unit.CurrentTile.Current = false;
            }
        }

        UI.DisplayButton(DivideBtnName, false);
    }

    /// <inheritdoc cref="Unit.Init" />
    protected override void Init(short maxHitPoints, short maxMovementPoints, short visibilityRange)
    {
        base.Init(maxHitPoints, maxMovementPoints, visibilityRange);
        _collider = GetComponent<Collider2D>();
        _mainCamera = Camera.main;
        _isMoving = false;

        if (!Clone)
        {
            Start();
        }

        PlayerUnits.Add(this);
    }

    /// <summary>
    /// Checks if the all the player unit's neighbouring tiles are inhabited.
    /// </summary>
    /// <returns>True if there are no uninhabited neighbouring tiles, otherwise false.</returns>
    protected bool IsFullySurrounded()
    {
        foreach (Tile t in CurrentTile.NeighbouringTiles)
        {
            if (!t.Inhabited)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks that no other unit is currently moving.
    /// </summary>
    /// <returns>True if another player unit is moving, otherwise false.</returns>
    private bool ConfirmNoOtherUnitMoving()
    {
        //Multiple selections were causing bugs to occurs so I set in this restriction.
        //I want to make this a feature in the future.
        //For demo purposes it exists.
        foreach (PlayerUnit unit in PlayerUnits)
        {
            if (unit != this && unit.IsMoving)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Finds all tiles that are reachable for the current unit.
    /// This method is recursive, so pass in a new Queue instance for this to work correctly.
    /// </summary>
    /// <param name="tile">The starting tile. Usually the unit's current tile.</param>
    /// <param name="selectableTiles">A sorted queue of reachable tiles.</param>
    /// <param name="distance">Number of tiles to search away from the starting tile.</param>
    private void FindSelectableTiles(Tile tile, Queue<Tile> selectableTiles, int distance)
    {
        foreach (Tile t in tile.NeighbouringTiles)
        {
            if (distance <= MovementPoints && !selectableTiles.Contains(t))
            {
                t.Reachable = true;
                //The tile may not have the inhabited property by default because this is only set within the visibilty range.
                //Therefore if the visibilty is less than the maximum movement range we need to determine it's availability separately.
                TileMovement.DetermineTileIsInhabited(t, _collider);

                if (!t.Inhabited)
                {
                    selectableTiles.Enqueue(tile);
                    FindSelectableTiles(t, selectableTiles, distance + 1);
                }
            }
        }

        if (selectableTiles.Count > 0)
        {
            selectableTiles.Dequeue();
        }
    }

    //Is run when the current unit is selected.
    //This way only one unit is selected at a time.
    private void DeselectOtherUnits()
    {
        foreach (PlayerUnit unit in PlayerUnits)
        {
            if (unit != this && unit.Selected)
            {
                unit.Selected = false;
                unit.CurrentTile.Current = false;
            }
        }
    }

    //This is used to refresh all visible tiles so the player can see all of them when necessary.
    private void FindAllVisibleTiles()
    {
        foreach (PlayerUnit unit in PlayerUnits)
        {
            TileMovement.FindVisibleTiles(unit.CurrentTile, new Queue<Tile>(), unit.Visibility);
        }
    }
}