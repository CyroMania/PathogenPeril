using UnityEngine;

public class Bacteria : PlayerUnit
{
	[SerializeField]
	private static readonly short _maxHitPoints = 8;
	[SerializeField]
	private static readonly short _maxMovementPoints = 10;

	public Bacteria()
		: base(_maxHitPoints, _maxMovementPoints)
	{
	}
}
