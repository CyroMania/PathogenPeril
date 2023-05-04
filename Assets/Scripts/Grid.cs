using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{

    [SerializeField]
    private int _gridWidth = 10;

    [SerializeField]
    private int _gridHeight = 30;

    [SerializeField]
    private Tile _tile;

    /// <summary>
    /// All tiles in the grid Gameboard.
    /// </summary>
    public List<Tile> Tiles { get; set; }

    private void Start()
    {
        Tiles = new List<Tile>();

        if (Tiles.Count == 0)
        {
            GenerateTiles(_gridWidth, _gridHeight, _tile);
        }
    }

    /// <summary>
    /// Generates tiles for the grid with a calculated position and rotation.
    /// </summary>
    /// <param name="width">Number of tiles along the x axis.</param>
    /// <param name="height">Number of tiles along the y axis.</param>
    /// <param name="tile">The tile to generate by default.</param>
    public void GenerateTiles(int width, int height, Tile tile)
    {
        for (float x = 0; x < width; x++)
        {
            for (float y = 0; y < height; y++)
            {
                //This if else check is specifically implemented for unit tests.
                //Tile is only null in the unit test file and allows to avoid other dependencies.
                //The main problem is that unit tests do not allow monobehaviour methods like Instantiate.
                //TODO: Will need to remove this later and replace with one block of code.
                if (tile != null)
                {
                    Tile clone = Instantiate(tile, new Vector3(x, y, transform.position.z), Quaternion.identity, transform);

                    //Tiles are named based on coordinates in world space for ease of testing.
                    clone.name = "Tile (" + x + ":" + y + ")";

                    //This sets the last tiles on the y axis to goal tiles. 
                    if (y == height - 1)
                    {
                        clone.Goal = true;
                    }

                    Tiles.Add(clone);
                }
                else
                {
                    GameObject clone = new GameObject();

                    clone.AddComponent<Tile>();
                    clone.transform.position = new Vector3(x, y, transform.position.z);
                    clone.transform.rotation = Quaternion.identity;
                    clone.transform.parent = transform;

                    clone.name = "Tile (" + x + ":" + y + ")";

                    if (y == height - 1)
                    {
                        clone.GetComponent<Tile>().Goal = true;
                    }

                    Tiles.Add(clone.GetComponent<Tile>());
                }
            }
        }

        //This if statement only runs if the is not null because the NSubstitute does not allow the Destroy method call.
        if (tile != null)
        {
            GameObject first = Tiles.First().gameObject;
            Destroy(first);
            Tiles.RemoveAt(0);
        }
    }
}