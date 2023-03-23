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
    public bool IsDead { get; internal set; }

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

        if (Selected)
        {
            CurrentTile.Current = false;
        }

        CurrentTile = null;
        CheckNoPlayerUnitsAlive();
    }

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

        UI.DisplayButton("_divideBtnAnim", false);
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
        if (IsPlayerTurn && !IsDead)
        {
            if (BeginTurn)
            {
                ResetUnit();
                UnitUI.UpdateStatBarValue(this, "Energy");
                UnitUI.UpdateStatBarPositions(this, _mainCamera.WorldToScreenPoint(gameObject.transform.position));
                TileMovement.FindVisibleTiles(CurrentTile, new Queue<Tile>(), 1, Visibility);

                if (Selected)
                {
                    CurrentTile.Current = true;
                    UI.CheckButtonsUsable(MovementPoints, MaxMovementPoints);
                    FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                }

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

                        if (CurrentTile.Goal)
                        {
                            Kill();
                            UI.UpdateScoreText(1);
                            ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible), nameof(Tile.Goal) });
                            return;
                        }

                        ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Current), nameof(Tile.Goal) });
                        FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                        FindAllVisibleTiles();
                    }

                    return;
                }
            }

            if (Input.GetMouseButtonDown(0) && ConfirmNoOtherUnitMoving())
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Vector2 clickPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

                if (!_selected)
                {
                    RaycastHit2D unitHitInfo = PhysicsHelper.GenerateRaycast("Unit", clickPosition);

                    if (unitHitInfo.collider == _collider)
                    {
                        Selected = true;
                        UI.DisplayButton("_divideBtnAnim", true);
                        UI.CheckButtonsUsable(MovementPoints, MaxMovementPoints);
                        ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible), nameof(Tile.Goal) });
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
                        UI.DisplayButton("_divideBtnAnim", false);
                        ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible), nameof(Tile.Goal) });
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
}
