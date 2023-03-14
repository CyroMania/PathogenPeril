using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    //This variable is associated with the turn system
    //Due to it's highly coupled relationship to the behaviours of units it is here for ease of use
    private static bool _isPlayerTurn = true;

    private static List<PlayerUnit> _playerUnits;
    private static List<ImmuneCell> _immuneCells;

    private short _maxHitPoints;
    private short _maxMovementPoints;
    private short _visibilityRange;

    private static bool staticsSetUp = false;

    private void Start()
    {
        if (!staticsSetUp)
        {
            _playerUnits = new List<PlayerUnit>();
            _immuneCells = new List<ImmuneCell>();
            UI = GameObject.Find("Canvas").GetComponent<UI>();

            staticsSetUp = true;
        }

        MovementPoints = _maxMovementPoints;
        HitPoints = _maxHitPoints;

        if (transform.position.z != -1)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        }
    }

    public static UI UI { get; set; }

    public bool BeginTurn { get; set; }

    protected static List<PlayerUnit> PlayerUnits
    {
        get => _playerUnits;
    }

    protected static List<ImmuneCell> ImmuneCells
    {
        get => _immuneCells;
    }

    protected short MaxHitPoints 
    { 
        get => _maxHitPoints;
    }

    public short MaxMovementPoints
    {
        get => _maxMovementPoints;
    }

    protected short HitPoints { get; set; }

    public short MovementPoints { get; set; }

    protected short Visibility 
    { 
        get => _visibilityRange; 
        set => _visibilityRange = value; 
    }

    public Tile CurrentTile { get; set; }

    public Tile TargetTile { get; set; }

    public static bool IsPlayerTurn
    {
        get => _isPlayerTurn;
        set => _isPlayerTurn = value;
    }

    protected void ResetUnit()
    {
        MovementPoints = _maxMovementPoints;
    }

    protected virtual void Init(short maxHitPoints, short maxMovementPoints, short visibilityRange)
    {
        _maxHitPoints = maxHitPoints;
        _maxMovementPoints = maxMovementPoints;
        _visibilityRange = visibilityRange;

        Start();
    }

    protected void ResetAllTiles(params string[] ignoredProps)
    {
        List<Tile> tiles = FindObjectsOfType<Tile>().ToList();

        foreach (Tile t in tiles)
        {
            t.ResetTile(ignoredProps);
        }
    }

    protected void SetTargetTileToCurrentTile()
    {
        CurrentTile.Current = false;
        TargetTile.Current = true;
        CurrentTile = TargetTile;
        TargetTile = null;
    }

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

    protected static void CheckNoPlayerUnitsAlive()
    {
        if (PlayerUnits.Count == 0)
        {
            Debug.Log("Game Over"); 
            UI.GameLost();
        }
    }

    public static void EndCurrentTurn()
    {
        _isPlayerTurn = !_isPlayerTurn;
        Debug.Log("PlayerTurn: " + _isPlayerTurn);

        if (_isPlayerTurn)
        {
            foreach (PlayerUnit unit in PlayerUnits)
            {
                unit.BeginTurn = true;
            }
        }
        else
        {
            foreach (ImmuneCell unit in ImmuneCells)
            {
                unit.BeginTurn = true;
                unit.FinishedTurn = false;
            }
        }
    }
}
