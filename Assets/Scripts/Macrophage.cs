public class Macrophage : ImmuneCell
{
    private static readonly short _maxHitPoints = 50;
    private static readonly short _maxMovementPoints = 5;

    private void Start()
    {
        base.Init(_maxHitPoints, _maxMovementPoints);
    }
} 