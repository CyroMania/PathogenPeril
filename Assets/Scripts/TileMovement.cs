using System.Collections.Generic;
using UnityEngine;

public static class TileMovement
{
    private static Vector3 _unitLayer = Vector3.back;
    private static float _speed = 2f;

    internal static Stack<Tile> FindTilePath(Tile currentTile, Tile targetTile, Stack<Tile> tilePath)
    {
        tilePath.Push(targetTile);

        float distance = FindDistance(currentTile, targetTile);
        Tile closestTile = null;

        foreach (Tile t in targetTile.NeighbouringTiles)
        {
            if (FindDistance(currentTile, t) < distance)
            {
                distance = FindDistance(currentTile, t);
                closestTile = t;
            }
        }

        if (closestTile != currentTile)
        {
            FindTilePath(currentTile, closestTile, tilePath);
        }

        return tilePath;
    }

    internal static void MoveToTile(PlayerUnit unit, Stack<Tile> path)
    {
        Vector3 unitPos = unit.transform.position;
        Tile tile = path.Peek();
        Vector3 tilePos = tile.transform.position + _unitLayer;

        if (Vector2.Distance(unitPos, tilePos) < 0.03f)
        {
            path.Pop();
            unit.transform.position = tilePos;
        }
        else
        {
            unit.transform.position = Vector3.MoveTowards(unitPos, tilePos,_speed * Time.deltaTime);
        }
    }

    private static float FindDistance(Tile a, Tile b)
    {
        Vector2 currentTilePos = a.gameObject.transform.position;
        Vector2 targetTilePos = b.gameObject.transform.position;

        return Vector2.Distance(currentTilePos, targetTilePos);
    }
}
