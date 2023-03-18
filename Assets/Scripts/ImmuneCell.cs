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

    public PlayerUnit TargetUnit
    {
        get => _targetUnit;
        set => _targetUnit = value;
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
        ImmuneCells.Add(this);
    }

    private void Update()
    {
        UpdateRenderer();

        if (!IsPlayerTurn && !_finishedTurn)
        {
            //To Begin the Turn we need to find a Target Unit and A short Path to Them
            if (BeginTurn)
            {
                ResetUnit();
                BeginTurn = false;

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
                        }
                        else
                        {
                            _path = FindTargetUnitNearestNeighbourTile();
                            _canAttack = true;
                        }
                    }
                    else
                    {
                        _path = FindPathClosestToTargetUnit();
                    }
                }
                else
                {
                    List<Tile> selectableTiles = FindSelectableTiles(CurrentTile, new List<Tile>(), 1);
                    Tile targetTile = selectableTiles[Random.Range(0, selectableTiles.Count)];
                    _path = TileMovement.FindTilePath(CurrentTile, targetTile, new Stack<Tile>(), MovementPoints);
                }

                MovementPoints -= (short)_path.Count;
            }
            //Otherwise Move towards that unit and attack them if possible
            else
            {
                if (_path.Count > 0)
                {
                    TileMovement.MoveToTile(this, _path);
                }
                else if (_path.Count == 0)
                {
                    CheckCanAttack();

                    _finishedTurn = true;
                    CheckLastImmuneCellFinished();
                }
            }
        }
    }

    private Stack<Tile> FindTargetUnitNearestNeighbourTile()
    {
        if (_targetUnit.CurrentTile.NeighbouringTiles.Contains(CurrentTile))
        {
            Debug.Log("Standing Next To Target Unit");
            return new Stack<Tile>();
        }

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

        if (targetUnitTile.NeighbouringTiles.Contains(currentTile))
        {
            return true;
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
            List<ImmuneCell> otherImmuneCells = ImmuneCells.Where(cell => cell != this).ToList();

            if (otherImmuneCells.Count > 0)
            {
                foreach (ImmuneCell cell in otherImmuneCells)
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
        if (TileMovement.FindDistance(CurrentTile, targetUnitTile) < Visibility)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Sets the nearest available player unit as the target Unit
    /// </summary>
    private void FindNearestPathogen()
    {
        if (PlayerUnits.Count > 0)
        {
            Tile closestUnitTile = null;
            float closestDistance = float.MaxValue;

            List<ImmuneCell> otherImmuneCells = ImmuneCells.Where(cell => cell != this).ToList();

            foreach (PlayerUnit unit in PlayerUnits)
            {
                if (otherImmuneCells.Count < PlayerUnits.Count)
                {
                    //We confirm that no other Unit has already targeted this Player Unit before targeting them ourselves
                    bool skipUnit = CheckUnitAlreadyATarget(otherImmuneCells, unit);

                    if (skipUnit)
                    {
                        continue;
                    }
                }

                Tile currentUnitTile = TileMovement.CalculateCurrentTile(unit);
                float currentDistance = TileMovement.FindDistance(CurrentTile, currentUnitTile);

                if (currentDistance < closestDistance || closestUnitTile == null)
                {
                    closestDistance = currentDistance;
                    closestUnitTile = currentUnitTile;
                }
            }

            RaycastHit2D hitInfo = PhysicsHelper.GenerateRaycast("Unit", closestUnitTile.transform.position);
            _targetUnit = hitInfo.collider.GetComponent<PlayerUnit>();

            //Check that this particular unit is the closest to this target unit
            if (otherImmuneCells.Count < PlayerUnits.Count)
            {
                if (CheckClosestEnemyUnit(_targetUnit.transform.position, otherImmuneCells))
                {
                    foreach (ImmuneCell otherCell in otherImmuneCells)
                    {
                        if (otherCell.TargetUnit == _targetUnit)
                        {
                            otherCell.TargetUnit = null;
                            otherCell.BeginTurn = true;
                        }
                    }
                }
            }
        }
    }

    private bool CheckClosestEnemyUnit(Vector2 targetPos, List<ImmuneCell> immuneCells)
    {
        foreach (ImmuneCell otherCell in immuneCells)
        {
            if (Vector2.Distance(transform.position, targetPos) > Vector2.Distance(otherCell.transform.position, targetPos))
            {
                return false;
            }
        }

        return true;
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

    private void CheckCanAttack()
    {
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

    private bool CheckUnitAlreadyATarget(List<ImmuneCell> otherImmuneCells, PlayerUnit unit)
    {
        foreach (ImmuneCell otherCell in otherImmuneCells)
        {
            if (otherCell._targetUnit != null && otherCell._targetUnit == unit)
            {
                Debug.Log(name + ": This Unit already Is Already Targeted!");
                return true;
            }
        }

        return false;
    }

    private void UpdateRenderer()
    {
        if (CurrentTile.Visible)
        {
            _renderer.enabled = true;
        }
        else
        {
            _renderer.enabled = false;
        }
    }
}