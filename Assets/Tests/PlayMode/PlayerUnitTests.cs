using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PlayerUnitTests
{
    //Class used for a test instance of the playerUnit class
    private class TestPlayerUnit : PlayerUnit
    {

    }

    private TestPlayerUnit SetupPlayerUnit()
    {
        Unit.UnitTesting = true;

        GameObject unit = new GameObject();
        TestPlayerUnit playerUnit = unit.AddComponent<TestPlayerUnit>();

        return playerUnit;
    }

    private GameObject[] SetupGrid(int width, int height)
    {
        GameObject[] tileObjects = new GameObject[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x * y;
                tileObjects[index] = new GameObject();
                tileObjects[index].transform.position = new Vector2(x, y);
                tileObjects[index].AddComponent<Tile>();
                tileObjects[index].AddComponent<SpriteRenderer>();
                tileObjects[index].AddComponent<BoxCollider2D>();
                tileObjects[index].GetComponent<BoxCollider2D>().size = new Vector2(1, 1);
                tileObjects[index].layer = LayerMask.NameToLayer("Tile");
            }
        }

        return tileObjects;
    }

    [UnityTest]
    public IEnumerator Start_Invoked_CurrentTileSet()
    {
        Unit.StaticsSetup = true;
        GameObject[] tiles = SetupGrid(5, 5);
        TestPlayerUnit playerUnit = SetupPlayerUnit();

        yield return new WaitForEndOfFrame(); //Awake
        yield return new WaitForEndOfFrame(); //Start

        Assert.NotNull(playerUnit.CurrentTile);
    }

    [UnityTest]
    public IEnumerator Selected_IsPlayerTurn_CurrentTileSetToCurrent()
    {
        GameObject canvas = new GameObject();
        canvas.AddComponent<Canvas>();
        Unit.UI = canvas.AddComponent<UI>();

        Unit.IsPlayerTurn = true;
        Unit.StaticsSetup = false;
        Unit.UnitTesting = true;

        GameObject[] tiles = SetupGrid(5, 5);
        TestPlayerUnit playerUnit = SetupPlayerUnit();

        playerUnit.Selected = true;
        yield return null;

        Assert.AreEqual(true, playerUnit.CurrentTile.Current);
    }
}