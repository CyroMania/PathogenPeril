using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Unit : MonoBehaviour
{
    //This variable is associated with the turn system
    //Due to it's highly coupled relationship to the behaviours of units it is here for ease of use
    private static bool _isPlayerTurn = true;

    private short _maxHitPoints;
    private short _maxMovementPoints;

    public short HitPoints { get; set; }
    public short MovementPoints { get; set; }
    public Tile CurrentTile { get; set; }
    public Tile TargetTile { get; set; }

    protected bool IsPlayerTurn
    {
        get { return _isPlayerTurn; }
        set { _isPlayerTurn = value; }
    }

    protected virtual void Init(short maxHitPoints, short maxMovementPoints)
    {
        _maxHitPoints = maxHitPoints;
        _maxMovementPoints = maxMovementPoints;

        Start();
    }


    void Start()
    {
        MovementPoints = _maxMovementPoints;
        HitPoints = _maxHitPoints;

        if (transform.position.z != -1)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        }
    }

    protected void ResetAllTiles(string ignoreProperty = "")
    {
        List<Tile> tiles = FindObjectsOfType<Tile>().ToList();

        foreach (Tile t in tiles)
        {
            t.ResetTile(ignoreProperty);
        }
    }

    protected void SetTargetTileToCurrentTile()
    {
        CurrentTile.Current = false;
        TargetTile.Current = true;
        CurrentTile = TargetTile;
        TargetTile = null;
    }

    public static void EndCurrentTurn()
    {
        _isPlayerTurn = !_isPlayerTurn;
    }
}
