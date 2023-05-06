using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// static class that handles the movement of units on tiles.
/// </summary>
public static class TileMovement
{
    /// <summary>
    /// The constant speed for unit's in motion on tiles.
    /// </summary>
    public static float Speed { get; } = 2f;

    /// <summary>
    /// Finds the optimal path between the current tile and the target tile.
    /// </summary>
    /// <param name="currentTile">The tile the unit is moving from.</param>
    /// <param name="targetTile">The tile the unit is moving to.</param>
    /// <param name="tilePath">The path of tiles.</param>
    /// <param name="remainingMovementPoints">The remaining movement points of the unit.</param>
    /// <returns>An ordered path of tiles.</returns>
    internal static Stack<Tile> FindTilePath(Tile currentTile, Tile targetTile, Stack<Tile> tilePath, short remainingMovementPoints)
    {
        //This algorithm works backwards from the destination.
        //This way we can avoid including the current tile in the result.
        //It defaults to the shortest distance.
        //If the current tile is the target tile we've finished the algorithm.
        if (targetTile != null && currentTile != targetTile)
        {
            tilePath.Push(targetTile);

            float distance = FindDistance(currentTile, targetTile);
            Tile closestTile = null;

            foreach (Tile t in targetTile.NeighbouringTiles)
            {
                //Must avoid inhabited tiles and make sure the tile is reachable.
                if (!t.Inhabited && t.Reachable)
                {
                    float tempDistance = FindDistance(currentTile, t);

                    if (tempDistance < distance && !tilePath.Contains(t))
                    {
                        distance = tempDistance;
                        closestTile = t;
                    }
                }
            }

            //This will occur if the next tile is further away and goes around an inhabited tile.
            if (closestTile == null)
            {
                foreach (Tile t in targetTile.NeighbouringTiles)
                {
                    if (t == currentTile)
                    {
                        return tilePath;
                    }

                    if (!t.Inhabited)
                    {
                        float tempDistance = FindDistance(currentTile, t);

                        //This comparison allows us not to compare which is closer and see if it's still in range of movement points and accounts for obstacles.
                        if (tempDistance <= remainingMovementPoints && !tilePath.Contains(t))
                        {
                            distance = tempDistance;
                            closestTile = t;
                        }
                    }
                }
            }

            //If this second loop fails, we've had an error or unresolved case so log the error.
            if (closestTile == null)
            {
                Debug.Log("Path Finding Failed");
                return tilePath;
            }

            FindTilePath(currentTile, closestTile, tilePath, --remainingMovementPoints);
        }

        return tilePath;
    }

    /// <summary>
    /// Moves the current unit to the next tile in the path.
    /// </summary>
    /// <param name="unit">The unit to move.</param>
    /// <param name="path">The path of tiles.</param>
    internal static void MoveToTile(Unit unit, Stack<Tile> path)
    {
        Vector3 unitPos = unit.transform.position;
        Tile tile = path.Peek();
        Vector3 tilePos = tile.transform.position;

        //After a small enough distance we can snap to the tile.
        if (Vector2.Distance(unitPos, tilePos) < 0.03f)
        {
            //Remove the tile from the path after we've finished moving.
            path.Pop();
            //Update the current tile to show the user.
            unit.transform.position = tilePos;
            unit.CurrentTile.Current = false;
            unit.CurrentTile = tile;
            unit.CurrentTile.Current = true;
        }
        else
        {
            //Dynamically move there with each frame.
            unit.transform.position = Vector3.MoveTowards(unitPos, tilePos, Speed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Finds the unit's current tile.
    /// </summary>
    /// <param name="unit">The unit search beneath.</param>
    /// <returns>A tile the unit is inhabiting.</returns>
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

    /// <summary>
    /// Finds all visible tiles from a unit's current position.
    /// This method is recursive, so pass in a new Queue instance for this to work correctly.
    /// </summary>
    /// <param name="tile">The tile to begin searching from.</param>
    /// <param name="visibleTiles">The ordered queue of visible tiles.</param>
    /// <param name="visibilityRange">The maximum distance we can search.</param>
    /// <param name="distance">The distance we are from the current tile.</param>
    internal static void FindVisibleTiles(Tile tile, Queue<Tile> visibleTiles, short visibilityRange, int distance = 1)
    {
        tile.Visible = true;

        foreach (Tile t in tile.NeighbouringTiles)
        {
            if (distance <= visibilityRange && !visibleTiles.Contains(t))
            {
                visibleTiles.Enqueue(tile);
                FindVisibleTiles(t, visibleTiles, visibilityRange, distance + 1);
            }
        }

        if (visibleTiles.Count > 0)
        {
            visibleTiles.Dequeue();
        }
    }

    /// <summary>
    /// Finds the distance between the centres of two tiles.
    /// </summary>
    /// <param name="a">The first tile.</param>
    /// <param name="b">The second tile.</param>
    /// <returns>Th,e distance between the two tiles.</returns>
    internal static float FindDistance(Tile a, Tile b)
    {
        Vector2 currentTilePos = a.gameObject.transform.position;
        Vector2 targetTilePos = b.gameObject.transform.position;

        return Vector2.Distance(currentTilePos, targetTilePos);
    }

    /// <summary>
    /// Marks a tile as inhabited if a unit is on top of it.
    /// </summary>
    /// <param name="t">The tile to check.</param>
    /// <param name="currentUnitCollider">The current unit's collider.</param>
    internal static void DetermineTileIsInhabited(Tile t, Collider2D currentUnitCollider)
    {
        RaycastHit2D hitInfo = PhysicsHelper.GenerateRaycast("Unit", t.transform.position);

        if (hitInfo.collider != null)
        {
            //We use this parameter because we don't want to set the current tile as inhabited.
            if (hitInfo.collider != currentUnitCollider)
            {
                t.Inhabited = true;
            }
        }
    }
}