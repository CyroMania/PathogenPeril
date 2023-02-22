using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlayerUnit : Unit
{
    private Camera _mainCamera;
    private Collider2D _collider;
    [SerializeField]
    private bool _isMoving;
    private bool _selected;
    [SerializeField]
    private Stack<Tile> _path;

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

    protected override void Init(short maxHitPoints, short maxMovementPoints)
    {
        base.Init(maxHitPoints, maxMovementPoints);

        Start();
    }

    void Start()
    {
        _isMoving = false;
        _collider = GetComponent<Collider2D>();
        _mainCamera = Camera.main;
    }

    void Update()
    {
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
                    ResetAllTiles(ignoreProperty: nameof(Tile.Current));
                    FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                }

                return;
            }
        }

        if (Input.GetMouseButtonDown(0) && ConfirmNoOtherUnitMoving())
        {
            Vector2 clickPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

            if (!_selected)
            {
                RaycastHit2D unitHitInfo = GenerateRaycast("Unit", clickPosition);

                if (unitHitInfo.collider == _collider)
                {
                    Selected = true;
                    ResetAllTiles();
                    CalculateCurrentTile();
                    FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                }

                return;
            }

            RaycastHit2D tileHitInfo = GenerateRaycast("Tile", clickPosition);

            if (tileHitInfo.collider != null)
            {
                Tile selectedTile = tileHitInfo.collider.gameObject.GetComponent<Tile>();

                if (selectedTile.Reachable && !selectedTile.Current && !selectedTile.Inhabited)
                {
                    TargetTile = selectedTile;
                    return;
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
            if (distance <= MovementPoints && selectableTiles.Contains(t) == false)
            {
                t.Reachable = true;
                DetermineTileIsInhabited(t);

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

    private void DetermineTileIsInhabited(Tile t)
    {
        RaycastHit2D hitInfo = GenerateRaycast("Unit", t.transform.position);

        if (hitInfo.collider != null)
        {
            if (hitInfo.collider != _collider)
            {
                t.Inhabited = true;
            }
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
            }
        }
    }
}
