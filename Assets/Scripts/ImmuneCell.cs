using System.Collections.Generic;
using System.Linq;

public abstract class ImmuneCell : Unit
{
    private PlayerUnit _targetUnit;

    protected override void Init(short maxHitPoints, short maxMovementPoints)
    {
        base.Init(maxHitPoints, maxMovementPoints);
    }

    private void Update()
    {
        if (!IsPlayerTurn)
        {
            List<ImmuneCell> cells = FindObjectsOfType<ImmuneCell>().ToList();
            CalculateCurrentTile();

            foreach (var cell in cells)
            {
                FindNearestPathogen(cell);
            }
        }
    }

    private void FindNearestPathogen(ImmuneCell immuneActor)
    {
        List<PlayerUnit> playerUnits = FindObjectsOfType<PlayerUnit>().ToList();
        PlayerUnit closestUnit = null;

        foreach (PlayerUnit unit in playerUnits) 
        {
            unit.
        }
    }
}