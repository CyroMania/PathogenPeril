using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class PlayerUnit : Unit
{
    private Camera _mainCamera;
    private Collider2D _collider;
    private bool _isMoving;
    private bool _selected;
    private Stack<Tile> _path;

    protected bool Clone { get; set; } = false;

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

    public bool IsMoving
    {
        get => _isMoving;
        set => _isMoving = value;
    }

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

        Destroy(gameObject);
        PlayerUnits.Remove(this);
        CheckNoPlayerUnitsAlive();
    }

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

    private void Start()
    {
        CurrentTile = TileMovement.CalculateCurrentTile(this);
        BeginTurn = true;
    }

    private void Update()
    {
        if (IsPlayerTurn)
        {
            if (BeginTurn)
            {
                ResetUnit();
                TileMovement.FindVisibleTiles(CurrentTile, new Queue<Tile>(), 1, Visibility);
                BeginTurn = false;
            }

            if (TargetTile != null)
            {
                if (!_isMoving)
                {
                    _path = TileMovement.FindTilePath(CurrentTile, TargetTile, new Stack<Tile>(), MovementPoints);
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
                        ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Current) });
                        FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                        FindAllVisibleTiles();
                    }

                    return;
                }
            }

            if (Input.GetMouseButtonDown(0) && ConfirmNoOtherUnitMoving())
            {
                Vector2 clickPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

                if (!_selected)
                {
                    RaycastHit2D unitHitInfo = PhysicsHelper.GenerateRaycast("Unit", clickPosition);

                    if (unitHitInfo.collider == _collider)
                    {
                        Selected = true;
                        UI.DisplayButtons();
                        UI.CheckButtonsUsable(MovementPoints, MaxMovementPoints);
                        ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible) });
                        CurrentTile.Current = true;
                        FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                    }

                    return;
                }

                RaycastHit2D tileHitInfo = PhysicsHelper.GenerateRaycast("Tile", clickPosition);

                if (tileHitInfo.collider != null)
                {
                    Tile selectedTile = tileHitInfo.collider.gameObject.GetComponent<Tile>();

                    if (selectedTile.Current)
                    {
                        return;
                    }
                    else if (selectedTile.Reachable && !selectedTile.Current && !selectedTile.Inhabited)
                    {
                        TargetTile = selectedTile;
                        return;
                    }
                    else if (!selectedTile.Reachable && !selectedTile.Visible)
                    {
                        Selected = false;
                        CurrentTile.Current = false;
                        UI.HideButtons();
                        ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible) });
                    }
                }
            }
        }
    }

    private bool ConfirmNoOtherUnitMoving()
    {
        foreach (PlayerUnit unit in PlayerUnits)
        {
            if (unit != this && unit.IsMoving)
            {
                return false;
            }
        }

        return true;
    }

    private void FindSelectableTiles(Tile tile, Queue<Tile> selectableTiles, int distance)
    {
        foreach (Tile t in tile.NeighbouringTiles)
        {
            if (distance <= MovementPoints && !selectableTiles.Contains(t))
            {
                t.Reachable = true;
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

    private void FindAllVisibleTiles()
    {
        foreach (PlayerUnit unit in PlayerUnits)
        {
            TileMovement.FindVisibleTiles(unit.CurrentTile, new Queue<Tile>(), 1, unit.Visibility);
        }
    }

    /// <summary>
    /// Retrieved from stackOverflow https://stackoverflow.com/questions/43754608/unity5-when-i-click-on-a-ui-button-the-gameobject-behind-it-gets-clicked
    /// </summary>
    /// <returns></returns>
    private static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
