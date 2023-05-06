using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class is the parent class for all enemy unit types found in the game.
/// It handles enemy unit pathfinding and attacks.
/// </summary>
public abstract class ImmuneCell : Unit
{
    //This boolean is temporary. It's a bodge fix due to a poor design decision earlier in development.
    //It will be removed during a large refactor.
    private bool _firstCheckedClosestUnit = false;
    private bool _finishedTurn = false;
    private bool _canAttack = false;
    private PlayerUnit _targetUnit;
    private Renderer _renderer;
    private Collider2D _collider;
    private Stack<Tile> _path = new Stack<Tile>();

    /// <summary>
    /// True if the immune cell has finished moving and attacking, otherwise false.
    /// </summary>
    public bool FinishedTurn
    {
        get => _finishedTurn;
        set => _finishedTurn = value;
    }

    /// <summary>
    /// The target player unit to this enemy unit is attacking.
    /// </summary>
    public PlayerUnit TargetUnit
    {
        get => _targetUnit;
        set => _targetUnit = value;
    }

    /// <inheritdoc cref="Unit.Init" />
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
            //To begin the turn we need to find a target unit and a short path to them.
            if (BeginTurn)
            {
                ResetUnit();
                _firstCheckedClosestUnit = false;
                BeginTurn = false;
                //Reset targets foreach turn.
                _targetUnit = null;
                TargetTile = null;

                FindNearestPathogen();
                List<Tile> selectableTiles = FindSelectableTiles(CurrentTile, new List<Tile>(), 1);

                //After finding a unit to target, we check that they are visible, then reachable.
                //All these methods are fairly self-explanatory but further explanations can be found in them.
                if (CheckUnitVisible(_targetUnit.CurrentTile))
                {
                    if (CheckUnitReachable(CurrentTile, _targetUnit.CurrentTile, out bool collided))
                    {
                        if (!collided && CheckClosestEnemyUnit(_targetUnit.CurrentTile.transform.position, ImmuneCells.Where(cell => cell != this).ToList()))
                        {
                            //We set the closest unit to attack the player unit if multiple units are targeting the same.
                            Debug.Log($"{gameObject.name}: I can attack");
                            _canAttack = true;
                        }

                        if (collided)
                        {
                            Debug.Log($"{gameObject.name}: Tile is Taken!");
                            _path = TileMovement.FindTilePath(CurrentTile, TargetTile, new Stack<Tile>(), MovementPoints);
                        }
                        else
                        {
                            Debug.Log($"{gameObject.name}: No collision happened");
                            _path = FindTargetUnitNearestNeighbourTile();
                        }
                    }
                    else
                    {
                        Debug.Log($"{gameObject.name}: Can't reach them, finding nearest tile");
                        _path = FindPathClosestToTargetUnit();
                    }
                }
                else
                {
                    Debug.Log($"{gameObject.name}: Can't see them, finding random tile");
                    Tile targetTile = selectableTiles[Random.Range(0, selectableTiles.Count)];
                    _path = TileMovement.FindTilePath(CurrentTile, targetTile, new Stack<Tile>(), MovementPoints);
                }

                MovementPoints -= (short)_path.Count;
            }
            else
            {
                //Now just move along the path we've found.
                if (_path.Count > 0)
                {
                    TileMovement.MoveToTile(this, _path);
                }
                else if (_path.Count == 0)
                {
                    //Attack the unit once we're next to them.
                    CheckCanAttack();
                    _finishedTurn = true;
                    CheckLastImmuneCellFinished();
                }
            }
        }
    }

    //This method compares all of a target unit's neighbour tiles and sees which one is the closest.
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

                //If closest tile is null we can't compare it to anything so we need to assign a tile immediately.
                if (tempDistance < closestDistance || closestTile == null)
                {
                    closestTile = neighbourTile;
                    closestDistance = tempDistance;
                }
            }
        }

        return TileMovement.FindTilePath(CurrentTile, closestTile, new Stack<Tile>(), MovementPoints);
    }

    //This algorithm works similarly to the one in PlayerUnit.
    //However, it can't use the same state so it relies on the list of selectable tiles to make sure a tile hasn't been found.
    private List<Tile> FindSelectableTiles(Tile origin, List<Tile> selectableTiles, int distance)
    {
        foreach (Tile t in origin.NeighbouringTiles)
        {
            //Distance is how far we are from the origin.
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

    //Checks if the target unit is reachable.
    private bool CheckUnitReachable(Tile currentTile, Tile targetUnitTile, out bool collided)
    {
        List<ImmuneCell> otherImmuneCells = ImmuneCells.Where(cell => cell != this).ToList();
        collided = false;

        //First check that they are greater than movement points.
        if (TileMovement.FindDistance(CurrentTile, targetUnitTile) > MovementPoints)
        {
            return false;
        }

        //Check if the player unit is next to the current enemy unit.
        if (targetUnitTile.NeighbouringTiles.Contains(currentTile))
        {
            List<ImmuneCell> neighbours = new List<ImmuneCell>();

            //We tally up all neighbours who are next to the target unit that are other enemy units.
            foreach (Tile t in targetUnitTile.NeighbouringTiles.Where(t => t != currentTile))
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
                    //If these neighbours exist and are targeting our unit we must go somewhere else.
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
        //This hack allows us to check if the neighbouring tiles to our target are reachable.
        foreach (Tile t in targetUnitTile.NeighbouringTiles)
        {
            if (selectableTiles.Contains(t))
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
                        //If not the closest enemy unit to this target player unit receive a random target tile near it.
                        if (!CheckClosestEnemyUnit(_targetUnit.CurrentTile.transform.position, ImmuneCells.Where(cell => cell != this).ToList()))
                        {
                            AssignRandomTargetTileFromCollection(this, targetUnitTile.NeighbouringTiles);
                            collided = true;
                        }
                        else
                        {
                            _canAttack = true;
                        }
                    }
                }
            }

            //If no collision we can attack.
            if (!collided && _targetUnit != null)
            {
                _canAttack = true;
            }

            return true;
        }

        return false;
    }

    //Returns true if the target unit is within the range of this enemy.
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

    //This is to give the unit arriving here a new target tile so they don't end up with the same current tile.
    private void AssignRandomTargetTileFromCollection(ImmuneCell cell, List<Tile> tiles)
    {
        FindSelectableTiles(CurrentTile, new List<Tile>(), 1);
        //Set the first explored tile our original target so we don't repeat looking at it.
        List<Tile> exploredTiles = new List<Tile>() { cell.TargetTile };
        cell.TargetTile = GetAvailableTile(tiles, exploredTiles);
        if (cell.TargetTile != null)
        {
            Debug.Log($"Assigned new tile to unit: {cell.TargetTile.name}");
        }
    }

    //This recursively calls itself until an available tile is found.
    //Used in case many enemy units are dogpilling one player unit.
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

        //Returns a random tile from what is available in the given list.
        if (availableTiles.Count > 0)
        {
            return tiles[Random.Range(0, tiles.Count)];
        }
        else
        {
            //If no tile is found, then we recursively try to find another available tile with a random neighbour tile's neighbours.
            //Then we add the neighbour tile to explored tiles to avoid backtracking.
            Tile randomTile = tiles[Random.Range(0, tiles.Count)];
            exploredTiles.Add(randomTile);
            GetAvailableTile(randomTile.NeighbouringTiles, exploredTiles);
        }

        //We return null if no tiles are available.
        //This means that no units can move however, which would be hard to achieve.
        Debug.Log("We should never hit this!");
        return null;
    }

    //Sets the nearest available player unit as the target unit.
    private void FindNearestPathogen()
    {
        if (PlayerUnits.Count > 0)
        {
            Tile closestUnitTile = null;
            float closestDistance = float.MaxValue;

            List<ImmuneCell> otherImmuneCells = ImmuneCells.Where(cell => cell != this).ToList();

            foreach (PlayerUnit unit in PlayerUnits)
            {
                //Make units target different units for simplicity if there are more player units.
                if (ImmuneCells.Count <= PlayerUnits.Count)
                {
                    //We confirm that no other Unit has already targeted this player unit before targeting them ourselves
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

    //This method is used to see what units specifically are the cloest to a given target.
    private bool CheckClosestEnemyUnit(Vector2 targetPos, List<ImmuneCell> immuneCells)
    {
        foreach (ImmuneCell otherCell in immuneCells)
        {
            //If this enemy unit's distance is greater than target then we are not the closest.
            if (Vector2.Distance(transform.position, targetPos) > Vector2.Distance(otherCell.transform.position, targetPos))
            {
                return false;
            }
            //We need to check if the other enemy unit has the same distance from the player unit as this enemy unit.
            //If we didn't do pathfinding in the update loop we could've avoided this loop and check.
            //Need to remove this in a refactor and have enemy units complete pathfinding in sequence.
            else if (Vector2.Distance(transform.position, targetPos) == Vector2.Distance(otherCell.transform.position, targetPos))
            {
                //What we do in this case is run another loop and identify if any other units have already checked this player unit before this enemy unit.
                foreach (ImmuneCell otherCell2 in immuneCells)
                {
                    //If any have we return false.
                    if (otherCell2._firstCheckedClosestUnit)
                    {
                        return false;
                    }
                }

                //If none have checked so far we are the first to so to avoid any others targeting we set our variable to true.
                _firstCheckedClosestUnit = true;
            }
        }

        return true;
    }

    //If the unit cannot be reached but can be seen we want the closes tile to them.
    //We simply compare all the unit's reachable tiles with the target player unit's tile.
    //Whichever is the closest is the target tile.
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

    //Calls specific enemy unit type's attack if they are able.
    private void CheckCanAttack()
    {
        if (_canAttack)
        {
            if (this is Macrophage macrophage)
            {
                macrophage.Phagocytosis(_targetUnit);
                _canAttack = false;
            }
        }
    }

    //Since all enemy units find units simultaneously,
    //we must avoid overlapping targets as much as possible.
    private bool CheckUnitAlreadyATarget(List<ImmuneCell> otherImmuneCells, PlayerUnit unit)
    {
        foreach (ImmuneCell otherCell in otherImmuneCells)
        {
            if (otherCell._targetUnit != null && otherCell._targetUnit == unit)
            {
                Debug.Log($"{name}: This Unit already Is Already Targeted!");
                return true;
            }
        }

        return false;
    }

    //First thing checked in the update loop.
    //If a player unit can see this immune cell's current tile we make them visible.
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