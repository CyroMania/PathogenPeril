using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class GridTests
{
    [Test]
    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 3)]
    [TestCase(5, 5)]
    [TestCase(10, 10)]
    [TestCase(10, 0)]
    public void GenerateTiles_VaryingWidthAndHeight_NumTilesGeneratedIsMultipleOfWidthAndHeight(int width, int height)
    {
        GameObject board = new GameObject();
        Grid grid = board.AddComponent<Grid>();

        grid.GenerateTiles(width, height, null);

        Assert.AreEqual(width * height, grid.tiles.Count);
    }

    [Test]
    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 3)]
    [TestCase(5, 5)]
    [TestCase(10, 10)]
    [TestCase(0, 1)]
    [TestCase(0, 5)]
    public void GenerateTiles_HeightIsAtleastOne_EndTilesAreGoalTiles(int width, int height)
    {
        GameObject board = new GameObject();
        Grid grid = board.AddComponent<Grid>();

        grid.GenerateTiles(width, height, null);
        int numGoalTiles = 0;

        foreach (Tile t in grid.tiles)
        {
            if (t.transform.position.y == height - 1)
            {
                Assert.AreEqual(true, t.Goal);
                numGoalTiles++;
            }
        }

        Assert.AreEqual(width, numGoalTiles);
    }
}
