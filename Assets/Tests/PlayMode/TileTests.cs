using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class TileTests
{
    private Color orange = Vector4.Lerp(Color.red, Color.yellow, 0.7f);

    private void AssertTileColour(Color colour, GameObject tile)
    {
        Assert.AreEqual(colour, tile.GetComponent<Renderer>().material.color);
    }

    private Tile SetupTile()
    {
        GameObject tileObject = new GameObject();
        tileObject.AddComponent<SpriteRenderer>();
        return tileObject.AddComponent<Tile>();
    }

    [UnityTest]
    public IEnumerator Update_TileIsGoal_TileIsRed()
    {
        Tile tile = SetupTile();

        tile.Goal = true;
        yield return null;

        AssertTileColour(Color.red, tile.gameObject);
    }

    [UnityTest]
    public IEnumerator Update_TileIsCurrent_TileIsBlue()
    {
        Tile tile = SetupTile();

        tile.Current = true;
        yield return null;

        AssertTileColour(Color.blue, tile.gameObject);
    }

    [UnityTest]
    public IEnumerator Update_TileIsReachable_TileIsYellow()
    {
        Tile tile = SetupTile();

        tile.Reachable = true;
        yield return null;

        AssertTileColour(Color.yellow, tile.gameObject);
    }

    [UnityTest]
    public IEnumerator Update_TileIsInhabited_TileIsBlack()
    {
        Tile tile = SetupTile();

        tile.Inhabited = true;
        yield return null;

        AssertTileColour(Color.black, tile.gameObject);
    }

    [UnityTest]
    public IEnumerator Update_TileIsVisible_TileIsWhite()
    {
        Tile tile = SetupTile();

        tile.Visible = true;
        yield return null;

        AssertTileColour(Color.white, tile.gameObject);
    }

    [UnityTest]
    public IEnumerator Update_TileIsInvisible_TileIsGrey()
    {
        Tile tile = SetupTile();

        yield return null;

        AssertTileColour(Color.grey, tile.gameObject);
    }

    [UnityTest]
    public IEnumerator Update_TileIsReachableAndGoal_TileIsOrange()
    {
        Tile tile = SetupTile();

        tile.Goal = true;
        tile.Reachable = true;
        yield return null;

        AssertTileColour(orange, tile.gameObject);
    }
}