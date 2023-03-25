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
        //UpdateRenderer();

        if (!IsPlayerTurn && !_finishedTurn)
        {
            //To Begin the Turn we need to find a Target Unit and A short Path to Them
            if (BeginTurn)
            {
                ResetUnit();
                BeginTurn = false;
                TargetUnit = null;

                FindNearestPathogen();
                List<Tile> selectableTiles = FindSelectableTiles(CurrentTile, new List<Tile>(), 1);

                if (CheckUnitVisible(_targetUnit.CurrentTile))
                {
                    if (CheckUnitReachable(CurrentTile, _targetUnit.CurrentTile, out bool collided))
                    {
                        if (!collided && CheckClosestEnemyUnit(_targetUnit.CurrentTile.transform.position, ImmuneCells.Where(cell => cell != this).ToList()))
                        {
                            Debug.Log(gameObject.name + " I can attack");
                            _canAttack = true;
                        }
                        if (collided)
                        {
                            Debug.Log(gameObject.name + ": Tile is Taken!");
                            _path = TileMovement.FindTilePath(CurrentTile, TargetTile, new Stack<Tile>(), MovementPoints);
                        }
                        else
                        {
                            Debug.Log(gameObject.name + ": No collision happened");
                            _path = FindTargetUnitNearestNeighbourTile();
                        }
                    }
                    else
                    {
                        Debug.Log(gameObject.name + ": Can't reach them, finding nearest tile");
                        _path = FindPathClosestToTargetUnit();
                    }
                }
                else
                {
                    Debug.Log(gameObject.name + ": Can't see them, finding random tile");
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
                    _targetUnit = null;
                    TargetTile = null;
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

        Tile closestTile = null;
        float closestDistance = float.MaxValue;

        foreach (Tile neighbourTile in _targetUnit.CurrentTile.NeighbouringTiles)
        {
            if (selectableTiles.Contains(neighbourTile) && !neighbourTile.Inhabited)
            {
                float tempDistance = TileMovement.FindDistance(CurrentTile, neighbourTile);

                if (tempDistance < closestDistance || closestTile == null)
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
        List<ImmuneCell> otherImmuneCells = ImmuneCells.Where(cell => cell != this).ToList();
        collided = false;

        if (TileMovement.FindDistance(CurrentTile, targetUnitTile) > MovementPoints)
        {
            return false;
        }

        if (targetUnitTile.NeighbouringTiles.Contains(currentTile))
        {
            List<ImmuneCell> neighbours = new List<ImmuneCell>();

            foreach  (Tile t in targetUnitTile.NeighbouringTiles.Where(t => t != currentTile))
            {
                if (t.Inhabited)
                {
                    RaycastHit2D raycastInfo = PhysicsHelper.GenerateRaycast("Unit", t.transform.position);
                    if (raycastInfo.collider != null && raycastInfo.collider.gameObject.name.Contains("Macrophage"))
                    {
                        neighbours.Add(raycastInfo.collider.gameObject.GetComponent<ImmuneCell>());
                    }
                }
            }

            if (neighbours.Count > 0)
            {
                foreach (ImmuneCell cell in neighbours)
                {
                    if (cell.TargetUnit != null && cell.TargetUnit.CurrentTile == targetUnitTile)
                    {
                        AssignRandomTargetTileFromCollection(this, targetUnitTile.NeighbouringTiles);
                        collided = true;
                    }
                }
            }

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

        if (count >= 1)
        {
            if (otherImmuneCells.Count > 0)
            {
                foreach (ImmuneCell cell in otherImmuneCells)
                {
                    if (cell.TargetUnit != null && cell.TargetUnit.CurrentTile == targetUnitTile)
                    {
                        AssignRandomTargetTileFromCollection(this, targetUnitTile.NeighbouringTiles);
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

    //This is to give the unit arriving here a new target tile so they don't end up with the same current tile
    private void AssignRandomTargetTileFromCollection(ImmuneCell cell, List<Tile> tiles)
    {
        List<Tile> exploredTiles = new List<Tile>() { cell.TargetTile };
        cell.TargetTile = GetAvailableTile(tiles, exploredTiles);
        Debug.Log("Assigned new tile to unit: " + cell.TargetTile.name);
    }

    //this recursively calls itself until an available tile is found
    private Tile GetAvailableTile(List<Tile> tiles, List<Tile> exploredTiles)
    {
        List<Tile> availableTiles = new List<Tile>();

        foreach (Tile t in tiles)
        {
            if (!t.Inhabited && !exploredTiles.Contains(t))
            {
                availableTiles.Add(t);
            }
        }

        if (availableTiles.Count > 0)
        {
            return tiles[Random.Range(0, tiles.Count)];
        }
        else
        {
            Tile randomTile = tiles[Random.Range(0, tiles.Count)];
            exploredTiles.Add(randomTile);
            GetAvailableTile(randomTile.NeighbouringTiles, exploredTiles);
        }

        Debug.Log("We should never hit this!");
        return null;
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
                //make units target different units for simplicity if there are more playerUnits
                if (ImmuneCells.Count <= PlayerUnits.Count)
                {
                    //We confirm that no other Unit has already targeted this Player Unit before targeting them ourselves
                    if (CheckUnitAlreadyATarget(otherImmuneCells, unit))
                    {
                        continue;
                    }
                }

                //All units otherwise target their nearest unit
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