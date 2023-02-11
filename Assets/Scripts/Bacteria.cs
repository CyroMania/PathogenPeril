public class Bacteria : PlayerUnit
{
	private static readonly short _maxHitPoints = 8;
	private static readonly short _maxMovementPoints = 5;

    private void Start()
    {
        base.Init(_maxHitPoints, _maxMovementPoints);
    }
}