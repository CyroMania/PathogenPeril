using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    //This variable is associated with the turn system
    //Due to it's highly coupled relationship to the behaviours of units it is here for ease of use
    private static bool _isPlayerTurn = true;

    private short _maxHitPoints;
    private short _maxMovementPoints;
    private short _visibilityRange;

    private void Start()
    {
        MovementPoints = _maxMovementPoints;
        HitPoints = _maxHitPoints;

        if (transform.position.z != -1)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        }
    }

    protected short MaxHitPoints 
    { 
        get => _maxHitPoints;
    }
    protected short MaxMovementPoints
    {
        get => _maxMovementPoints;
    }

    protected short HitPoints { get; set; }
    protected short MovementPoints { get; set; }
    protected short Visibility 
    { 
        get => _visibilityRange; 
        set => _visibilityRange = value; 
    }

    public Tile CurrentTile { get; set; }
    public Tile TargetTile { get; set; }

    protected bool IsPlayerTurn
    {
        get => _isPlayerTurn;
        set => _isPlayerTurn = value;
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
        List<ImmuneCell> immuneCells = FindObjectsOfType<ImmuneCell>().ToList();

        foreach (ImmuneCell cell in immuneCells)
        {
            if (!cell.FinishedTurn)
            {
                return;
            }
        }

        ResetAllTiles(Array.Empty<string>());
        EndCurrentTurn();
    }

    public static void EndCurrentTurn()
    {
        _isPlayerTurn = !_isPlayerTurn;
        Debug.Log("PlayerTurn: " + _isPlayerTurn);
    }
}
