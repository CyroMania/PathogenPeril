using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class PlayerMovement
{
    private class TestPlayerUnit : Unit
    {
        private Stack<Tile> _path;
        private bool _isMoving;
        private bool _isSelected;

        private bool Selected { get; set; } = false;

        private static float FindDistance(Tile a, Tile b)
        {
            Vector2 currentTilePos = a.gameObject.transform.position;
            Vector2 targetTilePos = b.gameObject.transform.position;

            return Vector2.Distance(currentTilePos, targetTilePos);
        }

        private void FindSelectableTiles(Tile tile, Queue<Tile> selectableTiles, int distance)
        {
            foreach (Tile t in tile.NeighbouringTiles)
            {
                if (distance <= MovementPoints && !selectableTiles.Contains(t))
                {
                    t.Reachable = true;

                    if (!t.Inhabited)
                    {
                        selectableTiles.Enqueue(tile);
                        FindSelectableTiles(t, selectableTiles, distance + 1);
                    }
                }
            }

            if (selectableTiles.Count > 0)
            {
                selectableTiles.Dequeue();
            }
        }

        private static Stack<Tile> FindTilePath(Tile currentTile, Tile targetTile, Stack<Tile> tilePath, short remainingMovementPoints)
        {
            if (targetTile != null && currentTile != targetTile)
            {
                tilePath.Push(targetTile);

                float distance = FindDistance(currentTile, targetTile);
                Tile closestTile = null;

                foreach (Tile t in targetTile.NeighbouringTiles)
                {
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

                            if (tempDistance <= remainingMovementPoints && !tilePath.Contains(t))
                            {
                                distance = tempDistance;
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

                FindTilePath(currentTile, closestTile, tilePath, --remainingMovementPoints);
            }

            return tilePath;
        }

        private static void MoveToTile(Unit unit, Stack<Tile> path)
        {
            Vector3 unitPos = unit.transform.position;
            Tile tile = path.Peek();
            Vector3 tilePos = tile.transform.position;

            if (Vector2.Distance(unitPos, tilePos) < 0.03f)
            {
                path.Pop();
                unit.transform.position = tilePos;
                unit.CurrentTile.Current = false;
                unit.CurrentTile = tile;
                unit.CurrentTile.Current = true;
            }
            else
            {
                unit.transform.position = Vector3.MoveTowards(unitPos, tilePos, TileMovement.Speed * Time.deltaTime);
            }
        }

        private void Update()
        {
            if (Selected && !_isSelected)
            {
                _isSelected = true;
                FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
            }

            if (TargetTile != null)
            {
                if (!_isMoving)
                {
                    _path = FindTilePath(CurrentTile, TargetTile, new Stack<Tile>(), MovementPoints);
                    MovementPoints -= (short)_path.Count;
                    _isMoving = true;
                    return;
                }
                else
                {
                    MoveToTile(this, _path);

                    if (_path.Count == 0)
                    {
                        _isMoving = false;
                        SetTargetTileToCurrentTile();

                        if (Selected)
                        {
                            ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Current), nameof(Tile.Goal) });
                            FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                        }
                        else
                        {
                            ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Goal) });
                        }
                    }

                    return;
                }
            }
        }

        [UnityTest]
        public IEnumerator UnitMoves_TargetTileIsReachable_UnitMovesToTile()
        {
            Unit.StaticsSetup = true;
            Unit.IsPlayerTurn = true;

            GameObject unit = new GameObject();
            TestPlayerUnit bacteria = unit.AddComponent<TestPlayerUnit>();

            GameObject board = new GameObject();
            Grid grid = board.AddComponent<Grid>();

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            bacteria.CurrentTile = grid.tiles[45];
            bacteria.Selected = true;

            bacteria.TargetTile = grid.tiles[47];

            Assert.Equals(bacteria.CurrentTile, grid.tiles[47]);
        }
    }
}