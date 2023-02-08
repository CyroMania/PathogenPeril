using UnityEngine;

public class Bacteria : PlayerUnit
{
	[SerializeField]
	private static readonly short _maxHitPoints = 8;
	[SerializeField]
	private static readonly short _maxMovementPoints = 5;

    private void Start()
    {
        base.Init(_maxHitPoints, _maxMovementPoints);
    }
}
