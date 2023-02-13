using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    private short _maxHitPoints;
    private short _maxMovementPoints;

    public short HitPoints { get; set; }
    public short MovementPoints { get; set; }
    public Tile CurrentTile { get; set; }
    public Tile TargetTile { get; set; }

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
    }

    protected void ResetAllTiles(string ignoreProperty = "")
    {
        List<Tile> tiles = FindObjectsOfType<Tile>().ToList();

        foreach (Tile t in tiles)
        {
            t.ResetTile(ignoreProperty);
        }
    }
}
