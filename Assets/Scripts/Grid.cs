using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{

    [SerializeField]
    private int gridWidth = 10;

    [SerializeField]
    private int gridHeight = 10;

    [SerializeField]
    private Tile tile;

    private void Start()
    {
        List<Tile> tiles = FindObjectsOfType<Tile>().ToList();

        tile.gameObject.layer = LayerMask.NameToLayer("Tile");

        for (float x = 0; x < gridWidth; x++)
        {
            for (float y = 0; y < gridHeight; y++)
            {
                Tile clone = Instantiate(tile, new Vector3(x, y, transform.position.z), Quaternion.identity, transform);
                clone.name = "Tile (" + x + ":" + y + ")";

                if (y == gridHeight - 1)
                {
                    clone.Goal = true;
                }

                tiles.Add(clone);
            }
        }

        GameObject first = tiles.First().gameObject;
        Destroy(first);
        tiles.RemoveAt(0);
    }
}
