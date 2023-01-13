using UnityEngine;

public class Unit : MonoBehaviour
{
    private readonly short _maxHitPoints;
    private readonly short _maxMovementPoints;

    public Unit(short maxHitPoints, short maxMovementPoints)
    {
        _maxHitPoints = maxHitPoints;
        _maxMovementPoints = maxMovementPoints;
    }

    public short HitPoints { get; set; }
    public short MovementPoints { get; set; }
    public Tile CurrentTile { get; set; }

    void Start()
    {
        MovementPoints = _maxMovementPoints;
        HitPoints = _maxHitPoints;
    }
}
