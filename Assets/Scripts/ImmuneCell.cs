using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ImmuneCell : Unit
{
    private PlayerUnit _targetUnit;

    protected override void Init(short maxHitPoints, short maxMovementPoints, short visibiltyRange)
    {
        base.Init(maxHitPoints, maxMovementPoints, visibiltyRange);
    }

    private void Update()
    {
        if (!IsPlayerTurn)
        {
            List<ImmuneCell> cells = FindObjectsOfType<ImmuneCell>().ToList();
            CurrentTile =  TileMovement.CalculateCurrentTile(this);

            foreach (ImmuneCell cell in cells)
            {
                FindNearestPathogen(cell);

                //if unit visible

                //Moves towards unit

                //otherwise moves randomly
            }
        }
    }

    private void FindNearestPathogen(ImmuneCell immuneActor)
    {
        List<PlayerUnit> playerUnits = FindObjectsOfType<PlayerUnit>().ToList();

        if (playerUnits.Count > 0)
        {
            Tile closestUnitTile = TileMovement.CalculateCurrentTile(playerUnits.First());
            float closestDistance = TileMovement.FindDistance(CurrentTile, closestUnitTile);

            foreach (PlayerUnit unit in playerUnits)
            {
                Tile currentUnitTile = TileMovement.CalculateCurrentTile(unit);
                float currentDistance = TileMovement.FindDistance(CurrentTile, currentUnitTile);

                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    closestUnitTile = currentUnitTile;
                }
            }

            RaycastHit2D hitInfo = PhysicsHelper.GenerateRaycast("Unit", closestUnitTile.transform.position);
            immuneActor._targetUnit = hitInfo.collider.GetComponent<PlayerUnit>();
        }
    }
}