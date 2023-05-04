using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    private const string EndTurnBtnName = "_endTurnBtn";
    private const string DivideBtnName = "_divideBtn";

    //This variable is associated with the turn system
    //Due to it's highly coupled relationship to the behaviours of units it is here for ease of use
    private static bool _isPlayerTurn = true;

    private static List<PlayerUnit> _playerUnits;
    private static List<ImmuneCell> _immuneCells;

    private short _maxHitPoints;
    private short _maxMovementPoints;
    private short _visibilityRange;

    /// <summary>
    /// Set to true, if we want to ignore dependencies for testing.
    /// </summary>
    public static bool UnitTesting { get; set; } = false;

    /// <summary>
    /// False on load, set to true after static values that need to be generated when the scene loads.
    /// </summary>
    public static bool StaticsSetup { get; set; }

    private void Awake()
    {
        if (!StaticsSetup)
        {
            UI = GameObject.Find("Canvas").GetComponent<UI>();
            _playerUnits = new List<PlayerUnit>();
            _immuneCells = new List<ImmuneCell>();
            StaticsSetup = true;
        }
    }

    private void Start()
    {
        MovementPoints = _maxMovementPoints;
        HitPoints = _maxHitPoints;
    }

    /// <summary>
    /// True if it's the player's turn, false if it's the enemy's turn.
    /// </summary>
    public static bool IsPlayerTurn
    {
        get => _isPlayerTurn;
        set => _isPlayerTurn = value;
    }

    /// <summary>
    /// Set to true for a particular unit type at the beginning of their turn to handle setup.
    /// </summary>
    public bool BeginTurn { get; set; }

    /// <summary>
    /// The number of remaining health points awat from death a unit is.
    /// </summary>
    public short HitPoints { get; set; }

    /// <summary>
    /// The number of remaining squares a unit can move.
    /// </summary>
    public short MovementPoints { get; set; }

    /// <summary>
    /// The unit's maximum number of health points.
    /// Not in use, but will be in future.
    /// </summary>
    public short MaxHitPoints
    {
        get => _maxHitPoints;
    }

    /// <summary>
    /// The unit's maximum number of movement points.
    /// </summary>
    public short MaxMovementPoints
    {
        get => _maxMovementPoints;
    }

    /// <summary>
    /// The unit's currently inhabited tile. This can't be null.
    /// </summary>
    public Tile CurrentTile { get; set; }

    /// <summary>
    /// The unit's destination tile when moving. This can be null.
    /// </summary>
    public Tile TargetTile { get; set; }

    /// <summary>
    /// The number of tiles away from the current tile this unit can detect other units.
    /// </summary>
    protected short Visibility
    {
        get => _visibilityRange;
        set => _visibilityRange = value;
    } 

    /// <summary>
    /// All active player units.
    /// </summary>
    protected static List<PlayerUnit> PlayerUnits
    {
        get => _playerUnits;
    }

    /// <summary>
    /// All active enemy units.
    /// </summary>
    protected static List<ImmuneCell> ImmuneCells
    {
        get => _immuneCells;
    }

    /// <summary>
    /// The main canvas UI.
    /// </summary>
    internal static UI UI { get; private set; }

    /// <summary>
    /// Resets declared properties on all tiles with given restrictions.
    /// </summary>
    /// <param name="ignoredProps">The properties to ignore or not reset.</param>
    public static void ResetAllTiles(params string[] ignoredProps)
    {
        List<Tile> tiles = FindObjectsOfType<Tile>().ToList();

        foreach (Tile t in tiles)
        {
            t.ResetTile(ignoredProps);
        }
    }

    /// <summary>
    /// Checks if any player unit is currently selected.
    /// </summary>
    /// <returns>True if one is selected, otherwise false.</returns>
    public static bool CheckAnyPlayerUnitSelected()
    {
        foreach (PlayerUnit unit in PlayerUnits)
        {
            if (unit.Selected)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Ends the current turn and begins the opposing unit type's turn.
    /// </summary>
    public static void EndCurrentTurn()
    {
        _isPlayerTurn = !_isPlayerTurn;
        Debug.Log("PlayerTurn: " + _isPlayerTurn);

        if (_isPlayerTurn)
        {
            UI.DisplayButton(EndTurnBtnName, true);

            if (CheckAnyPlayerUnitSelected())
            {
                UI.DisplayButton(DivideBtnName, true);
            }

            foreach (PlayerUnit unit in PlayerUnits)
            {
                unit.BeginTurn = true;
            }
        }
        else
        {
            UI.DisplayButton(EndTurnBtnName, false);

            if (CheckAnyPlayerUnitSelected())
            {
                UI.DisplayButton(DivideBtnName, false);
            }

            SpawnNewEnemyUnit();

            foreach (ImmuneCell unit in ImmuneCells)
            {
                unit.BeginTurn = true;
                unit.FinishedTurn = false;
            }
        }
    }

    /// <summary>
    /// Initializes the current unit's maximum attributes and assigns necessary components.
    /// </summary>
    /// <param name="maxHitPoints">The unit's maximum health points.</param>
    /// <param name="maxMovementPoints">The unit's maximum movement points.</param>
    /// <param name="visibilityRange">The unit's maximum tile range of detecting other units.</param>
    protected virtual void Init(short maxHitPoints, short maxMovementPoints, short visibilityRange)
    {
        _maxHitPoints = maxHitPoints;
        _maxMovementPoints = maxMovementPoints;
        _visibilityRange = visibilityRange;

        Start();
    }

    /// <summary>
    /// Resets all generic attributes associated with all units.
    /// </summary>
    protected void ResetUnit()
    {
        MovementPoints = _maxMovementPoints;
    }

    /// <summary>
    /// When finishing movement, sets the previous target tile to this unit's current tile.
    /// </summary>
    protected void SetTargetTileToCurrentTile()
    {
        CurrentTile.Current = false;
        TargetTile.Current = true;
        CurrentTile = TargetTile;
        TargetTile = null;
    }

    /// <summary>
    /// Checks if all the Enemy Units have made a move this turn.
    /// </summary>
    protected void CheckLastImmuneCellFinished()
    {
        foreach (ImmuneCell cell in ImmuneCells)
        {
            if (!cell.FinishedTurn)
            {
                return;
            }
        }

        ResetAllTiles(new string[] { nameof(Tile.Goal) });
        EndCurrentTurn();
    }

    /// <summary>
    /// Checks for any active player units.
    /// If there aren't any ends the game.
    /// </summary>
    protected static void CheckNoPlayerUnitsAlive()
    {
        if (PlayerUnits.Count == 0 && UI.SucceededUnits != UI.RequiredSucceededUnits)
        {
            Debug.Log("Game Over");
            UI.GameLost();
        }
    }

    //Handles spawning a new Enemy Unit every turn.
    private static void SpawnNewEnemyUnit()
    {
        //The odds will better reflect reality if we account for how many units you have.
        // 1 Pathogen => 25%.
        // 3 Pathogens => 50%.
        // 6 Pathogens => 75%.
        // 9 Pathogens => 80%.
        int spawn = Random.Range(0, ImmuneCells.Count + 3);

        if (spawn < ImmuneCells.Count)
        {
            Debug.Log("Unit Spawned");

            List<Tile> AllTiles = FindObjectsOfType<Tile>().ToList();
            List<Tile> AcceptableTiles = new List<Tile>();

            foreach (Tile t in AllTiles)
            {
                bool enemyUnitTile = false;

                //We have to check the current tile referenced by each unit because the Tile.Current attribute only handles the selected unit.
                //This way if it's already a unit's tile we can skip to then next tile.
                foreach (ImmuneCell cells in ImmuneCells)
                {
                    if (cells.CurrentTile == t)
                    {
                        enemyUnitTile = true;
                        break;
                    }
                }

                //The enemy units can't spanw on goal tiles that would be unfair.
                if (enemyUnitTile)
                {
                    continue;
                }
                else if (t.Visible)
                {
                    continue;
                }
                else if (t.Goal)
                {
                    continue;
                }

                AcceptableTiles.Add(t);
            }

            //Check that there  is at least one tile available otherwise no spawning can occur.
            if (AcceptableTiles.Count > 0)
            {
                Tile spawnTile = AcceptableTiles[Random.Range(0, AcceptableTiles.Count)];
                Macrophage macrophage = Instantiate(FindObjectOfType<Macrophage>(), spawnTile.transform.position, Quaternion.identity);
                macrophage.GetComponent<Macrophage>().CurrentTile = spawnTile;
            }
        }
    }
}