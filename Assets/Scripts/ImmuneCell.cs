using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ImmuneCell : Unit
{
    private PlayerUnit _targetUnit;
    private Renderer _renderer;
    private Collider2D _collider;
    private Stack<Tile> _path = new Stack<Tile>();

    private bool _finishedTurn = false;
    private bool _canAttack = false;

    public bool FinishedTurn
    {
        get => _finishedTurn;
        set => _finishedTurn = value;
    }

    protected override void Init(short maxHitPoints, short maxMovementPoints, short visibiltyRange)
    {
        base.Init(maxHitPoints, maxMovementPoints, visibiltyRange);
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();

        Start();
    }

    private void Start()
    {
        CurrentTile = TileMovement.CalculateCurrentTile(this);
    }

    private void Update()
    {
        //if (CurrentTile.Visible)
        //{
        //    _renderer.enabled = true;
        //}
        //else
        //{
        //    _renderer.enabled = false;
        //}

        if (_path.Count > 0)
        {
            TileMovement.MoveToTile(this, _path);

            if (_path.Count == 0)
            {
                CurrentTile = TileMovement.CalculateCurrentTile(this);
                _finishedTurn = true;
                CheckLastImmuneCellFinished();

                if (_canAttack)
                {
                    if (this is Macrophage)
                    {
                        Macrophage macrophage = this as Macrophage;
                        macrophage.Phagocytosis(_targetUnit);
                        _canAttack = false;
                    }
                }
            }

            return;
        }

        if (!IsPlayerTurn && !_finishedTurn)
        {
            if (BeginTurn)
            {
                ResetUnit();
                BeginTurn = false;
            }

            if (_path.Count == 0 && MovementPoints == MaxMovementPoints)
            {
                FindNearestPathogen();

                if (CheckUnitVisible(_targetUnit.CurrentTile))
                {
                    if (CheckUnitReachable(CurrentTile, _targetUnit.CurrentTile, out bool collided))
                    {
                        if (collided)
                        {
                            Debug.Log(gameObject.name + ": Tile is Taken!");

                            Tile closestNeighbourTile = TileMovement.FindClosestTileInCollection(this, _targetUnit.CurrentTile.NeighbouringTiles);
                            _path = TileMovement.FindTilePath(CurrentTile, closestNeighbourTile, new Stack<Tile>(), MovementPoints);
                            MovementPoints -= (short)_path.Count;
                            return;
                        }

                        _path = FindTargetUnitNearestNeighbourTile();
                        _canAttack = true;
                    }
                    else
                    {
                        _path = FindPathClosestToTargetUnit();
                    }
                }
                else
                {
                    List<Tile> selectableTiles = FindSelectableTiles(CurrentTile, new List<Tile>(), 1);
                    Tile targetTile = selectableTiles.ToArray()[Random.Range(0, selectableTiles.Count)];
                    _path = TileMovement.FindTilePath(CurrentTile, targetTile, new Stack<Tile>(), MovementPoints);
                }

                MovementPoints -= (short)_path.Count;
            }
        }
    }

    private Stack<Tile> FindTargetUnitNearestNeighbourTile()
    {
        List<Tile> selectableTiles = FindSelectableTiles(CurrentTile, new List<Tile>(), 1);

        Tile closestTile = _targetUnit.CurrentTile.NeighbouringTiles.FirstOrDefault();
        float closestDistance = TileMovement.FindDistance(CurrentTile, closestTile);

        foreach (Tile neighbourTile in _targetUnit.CurrentTile.NeighbouringTiles)
        {
            if (selectableTiles.Contains(neighbourTile))
            {
                float tempDistance = TileMovement.FindDistance(CurrentTile, neighbourTile);

                if (tempDistance < closestDistance)
                {
                    closestTile = neighbourTile;
                    closestDistance = tempDistance;
                }
            }
        }

        return TileMovement.FindTilePath(CurrentTile, closestTile, new Stack<Tile>(), MovementPoints);
    }

    private List<Tile> FindSelectableTiles(Tile origin, List<Tile> selectableTiles, int distance)
    {
        foreach (Tile t in origin.NeighbouringTiles)
        {
            if (distance <= MovementPoints)
            {
                TileMovement.DetermineTileIsInhabited(t, _collider);

                if (!t.Inhabited & t != CurrentTile)
                {
                    t.Reachable = true;

                    if (!selectableTiles.Contains(t))
                    {
                        selectableTiles.Add(t);
                    }

                    FindSelectableTiles(t, selectableTiles, distance + 1);
                }
            }
        }

        return selectableTiles;
    }

    private bool CheckUnitReachable(Tile currentTile, Tile targetUnitTile, out bool collided)
    {
        collided = false;

        if (TileMovement.FindDistance(CurrentTile, targetUnitTile) > MovementPoints)
        {
            return false;
        }

        List<Tile> selectableTiles = FindSelectableTiles(currentTile, new List<Tile>(), 1);
        int count = 0;

        foreach (Tile t in targetUnitTile.NeighbouringTiles)
        {
            if (t.Reachable)
            {
                count++;
            }
        }

        if (count > 1)
        {
            List<ImmuneCell> cells = FindObjectsOfType<ImmuneCell>().Where(cell => cell != this).ToList();

            if (cells.Count > 0)
            {
                foreach (ImmuneCell cell in cells)
                {
                    if (cell._targetUnit != null && cell._targetUnit.CurrentTile == targetUnitTile)
                    {
                        collided = true;
                    }
                }
            }

            return true;
        }

        return false;
    }

    private bool CheckUnitVisible(Tile targetUnitTile)
    {
        if (TileMovement.FindDistance(CurrentTile, targetUnitTile) > Visibility)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void FindNearestPathogen()
    {
        List<PlayerUnit> playerUnits = FindObjectsOfType<PlayerUnit>().ToList();

        if (playerUnits.Count > 0)
        {
            Tile closestUnitTile = TileMovement.CalculateCurrentTile(playerUnits.First());
            float closestDistance = TileMovement.FindDistance(CurrentTile, closestUnitTile);

            List<ImmuneCell> cells = FindObjectsOfType<ImmuneCell>().Where(cell => cell != this).ToList();

            foreach (PlayerUnit unit in playerUnits)
            {
                if (cells.Count < playerUnits.Count)
                {
                    bool skipUnit = false;

                    foreach (ImmuneCell cell in cells)
                    {
                        if (cell._targetUnit != null && cell._targetUnit == unit)
                        {
                            skipUnit = true;
                        }
                    }

                    if (skipUnit)
                    {
                        continue;
                    }
                }

                Tile currentUnitTile = TileMovement.CalculateCurrentTile(unit);
                float currentDistance = TileMovement.FindDistance(CurrentTile, currentUnitTile);

                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    closestUnitTile = currentUnitTile;
                }
            }

            RaycastHit2D hitInfo = PhysicsHelper.GenerateRaycast("Unit", closestUnitTile.transform.position);
            _targetUnit = hitInfo.collider.GetComponent<PlayerUnit>();
        }
    }

    private Stack<Tile> FindPathClosestToTargetUnit()
    {
        List<Tile> selectableTiles = FindSelectableTiles(CurrentTile, new List<Tile>(), 1);
        Tile closestTile = selectableTiles.FirstOrDefault();
        float closestDistance = TileMovement.FindDistance(closestTile, _targetUnit.CurrentTile);

        foreach (Tile t in selectableTiles)
        {
            float tempDistance = TileMovement.FindDistance(t, _targetUnit.CurrentTile);

            if (tempDistance < closestDistance)
            {
                closestTile = t;
                closestDistance = tempDistance;
            }
        }

        return TileMovement.FindTilePath(CurrentTile, closestTile, new Stack<Tile>(), MovementPoints);
    }
}