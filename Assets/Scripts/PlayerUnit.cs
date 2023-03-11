using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlayerUnit : Unit
{
    private Camera _mainCamera;
    private Collider2D _collider;
    private bool _isMoving;
    private bool _selected;
    private Stack<Tile> _path;
    private UI _UI;

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

    protected UI UI
    {
        get => _UI;
    }

    public void Kill()
    {
        GameObject.Find("Canvas").GetComponent<UnitUI>().DestroyStatBars(this);

        List<ImmuneCell> cells = FindObjectsOfType<ImmuneCell>().ToList();

        foreach (ImmuneCell cell in cells)
        {
            if (cell.TargetUnit == this)
            {
                cell.TargetUnit = null;
            }
        }

        Destroy(gameObject);
    }

    protected override void Init(short maxHitPoints, short maxMovementPoints, short visibilityRange)
    {
        base.Init(maxHitPoints, maxMovementPoints, visibilityRange);
        _collider = GetComponent<Collider2D>();
        _mainCamera = Camera.main;
        _isMoving = false;
        _UI = GameObject.Find("Canvas").GetComponent<UI>();

        if (!Clone)
        {
            Start();
        }
    }

    private void Start()
    {
        CurrentTile = TileMovement.CalculateCurrentTile(this);
        TileMovement.FindVisibleTiles(CurrentTile, new Queue<Tile>(), 1, Visibility);
    }

    private void Update()
    {
        if (IsPlayerTurn)
        {
            if (BeginTurn)
            {
                ResetUnit();
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
                        _UI.DisplayButtons();
                        _UI.CheckButtonsUsable(MovementPoints, MaxMovementPoints);
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
                        _UI.HideButtons();
                        ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible) });
                    }
                }
            }
        }
    }

    private bool ConfirmNoOtherUnitMoving()
    {
        List<PlayerUnit> units = FindObjectsOfType<PlayerUnit>().ToList();

        foreach (PlayerUnit unit in units)
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
        List<PlayerUnit> units = FindObjectsOfType<PlayerUnit>().ToList();

        foreach (PlayerUnit unit in units)
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
        List<PlayerUnit> units = FindObjectsOfType<PlayerUnit>().ToList();

        foreach (PlayerUnit unit in units)
        {
            TileMovement.FindVisibleTiles(unit.CurrentTile, new Queue<Tile>(), 1, unit.Visibility);
        }
    }
}
