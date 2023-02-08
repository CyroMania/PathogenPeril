using UnityEngine;

public class Unit : MonoBehaviour
{
    private short _maxHitPoints;
    private short _maxMovementPoints;

    public short HitPoints { get; set; }
    public short MovementPoints { get; set; }
    public Tile CurrentTile { get; set; }

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
}
