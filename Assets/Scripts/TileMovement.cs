using System.Collections.Generic;
using UnityEngine;

public static class TileMovement
{
    public static Vector3 UnitLayer { get; } = Vector3.back;

    public static float Speed { get; } = 2f;

    internal static Stack<Tile> FindTilePath(Tile currentTile, Tile targetTile, Stack<Tile> tilePath, short remainingMovementPoints)
    {
        tilePath.Push(targetTile);

        float distance = FindDistance(currentTile, targetTile);
        Tile closestTile = null;

        foreach (Tile t in targetTile.NeighbouringTiles)
        {
            if (!t.Inhabited)
            {
                if (FindDistance(currentTile, t) < distance && !tilePath.Contains(t) && t.Reachable)
                {
                    distance = FindDistance(currentTile, t);
                    closestTile = t;
                }
            }
        }

        if (closestTile == null)
        {
            foreach (Tile t in targetTile.NeighbouringTiles)
            {
                if (!t.Inhabited)
                {
                    if (FindDistance(currentTile, t) <= remainingMovementPoints && !tilePath.Contains(t))
                    {
                        distance = FindDistance(currentTile, t);
                        closestTile = t;
                    }
                }
            }
        }

        if (closestTile == null) 
        {
            Debug.Log("Path Finding Failed");
            return tilePath;
        }

        if (closestTile != currentTile)
        {
            FindTilePath(currentTile, closestTile, tilePath, --remainingMovementPoints);
        }

        return tilePath;
    }

    internal static void MoveToTile(PlayerUnit unit, Stack<Tile> path)
    {
        Vector3 unitPos = unit.transform.position;
        Tile tile = path.Peek();
        Vector3 tilePos = tile.transform.position + UnitLayer;

        if (Vector2.Distance(unitPos, tilePos) < 0.03f)
        {
            path.Pop();
            unit.transform.position = tilePos;
        }
        else
        {
            unit.transform.position = Vector3.MoveTowards(unitPos, tilePos, Speed * Time.deltaTime);
        }
    }

    internal static Tile CalculateCurrentTile(Unit unit)
    {
        RaycastHit2D hitInfo = PhysicsHelper.GenerateRaycast("Tile", unit.transform.position);
        GameObject target = hitInfo.collider.gameObject;

        if (target.layer == 3)
        {
            Tile current = target.GetComponent<Tile>();
            return current;
        }

        return null;
    }

    internal static void FindVisibleTiles(Tile tile, Queue<Tile> visibleTiles, int distance, short visibilityRange)
    {
        tile.Visible = true;

        foreach (Tile t in tile.NeighbouringTiles)
        {
            if (distance <= visibilityRange && !visibleTiles.Contains(t))
            {
                visibleTiles.Enqueue(tile);
                FindVisibleTiles(t, visibleTiles, distance + 1, visibilityRange);
            }
        }

        if (visibleTiles.Count > 0)
        {
            visibleTiles.Dequeue();
        }
    }

    internal static float FindDistance(Tile a, Tile b)
    {
        Vector2 currentTilePos = a.gameObject.transform.position;
        Vector2 targetTilePos = b.gameObject.transform.position;

        return Vector2.Distance(currentTilePos, targetTilePos);
    }

    internal static void DetermineTileIsInhabited(Tile t, Collider2D currentUnitCollider)
    {
        RaycastHit2D hitInfo = PhysicsHelper.GenerateRaycast("Unit", t.transform.position);

        if (hitInfo.collider != null)
        {
            if (hitInfo.collider != currentUnitCollider)
            {
                t.Inhabited = true;
            }
        }
    }
}
