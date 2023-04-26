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

    public List<Tile> tiles = new List<Tile>();

    private void Start()
    {
        GenerateTiles(gridWidth, gridHeight, tile);
    }

    public void GenerateTiles(int width, int height, Tile tile)
    {


        for (float x = 0; x < width; x++)
        {
            for (float y = 0; y < height; y++)
            {
                if (tile != null)
                {
                    Tile clone = Instantiate(tile, new Vector3(x, y, transform.position.z), Quaternion.identity, transform);

                    clone.name = "Tile (" + x + ":" + y + ")";

                    if (y == height - 1)
                    {
                        clone.Goal = true;
                    }

                    tiles.Add(clone);

                    GameObject first = tiles.First().gameObject;
                    Destroy(first);
                    tiles.RemoveAt(0);
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

                    tiles.Add(clone.GetComponent<Tile>());
                }
            }
        }


    }
}
