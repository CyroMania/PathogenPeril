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

    void Start()
    {
        List<Tile> tiles = FindObjectsOfType<Tile>().ToList();

        tile.gameObject.layer = 3;

        for (float x = 0; x < gridWidth; x++)
        {
            for (float y = 0; y < gridHeight; y++)
            {
                Tile clone = Instantiate(tile, new Vector3(x, y, transform.position.z), Quaternion.identity, transform);
                tiles.Add(clone);
            }
        }

        GameObject first = tiles.First().gameObject;
        Destroy(first);
        tiles.RemoveAt(0);
    }

}
