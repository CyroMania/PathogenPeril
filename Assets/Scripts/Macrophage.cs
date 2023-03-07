public class Macrophage : ImmuneCell
{
    private static readonly short _maxHitPoints = 50;
    private static readonly short _maxMovementPoints = 3;
    private static readonly short _visibiliyRange = 6;

    private void Start()
    {
        base.Init(_maxHitPoints, _maxMovementPoints, _visibiliyRange);
    }
} 