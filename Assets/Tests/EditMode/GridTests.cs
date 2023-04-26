using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[TestFixture]
public class GridTests
{
    [Test]
    public void GenerateTiles_VaryingWidthAndHeight_NumTilesGeneratedIsMultipleOfWidthAndHeight()
    {
        GameObject board = new GameObject();
        Grid grid = board.AddComponent<Grid>();

        grid.GenerateTiles(1, 1, null);

        Assert.AreEqual(1, grid.tiles.Count);
    }
}
