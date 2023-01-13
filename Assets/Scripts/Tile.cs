using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private bool Reachable = false;
    private bool Inhabited = false;
    private Renderer renderer;

    [SerializeField]
    private List<Tile> tiles;
    public bool Current { get; set; } = false;

    public List<Tile> NeighbouringTiles 
    {
        get { return tiles; }
        private set
        {
            tiles = value;
        }
    }


    void Start()
    {
        NeighbouringTiles = FindNeighbouringTiles();
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (Current)
        {
            renderer.material.color = Color.blue;
        }
        else if (Reachable)
        {
            renderer.material.color = Color.yellow;
        }
        else if (Inhabited)
        {
            renderer.material.color = Color.black;
        }
        else
        {
            renderer.material.color = Color.white;
        }
    }

    private void Reset()
    {
        Reachable = false;
        Inhabited = false;
        Current = false;
    }

    private List<Tile> FindNeighbouringTiles()
    {
        Vector2 size = new Vector2(0.5f, 0.5f);
        List<Tile> list = new List<Tile>();
        int layerMask = 1 << (int)LayerMask.NameToLayer("Tile");

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != y && x * y == 0)
                {
                    Collider2D collider = Physics2D.OverlapBox(transform.position + new Vector3(x, y), size, 0f, layerMask);

                    if (collider != null)
                    {
                        list.Add(collider.gameObject.GetComponent<Tile>());
                    }
                }
            }
        }

        return list;
    }
}
