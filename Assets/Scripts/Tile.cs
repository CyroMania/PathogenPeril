using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private bool _goal = false;
    [SerializeField]
    private bool _current = false;
    [SerializeField]
    private bool _inhabited = false;
    [SerializeField]
    private bool _reachable = false;
    [SerializeField]
    private bool _visible = false;

    private new Renderer renderer;

    [SerializeField]
    private List<Tile> _neighbourTiles;


    public bool Current
    {
        get => _current;
        set => _current = value;
    }

    public bool Inhabited
    {
        get => _inhabited;
        set => _inhabited = value;
    }

    public bool Reachable
    {
        get => _reachable;
        set => _reachable = value;
    }

    public bool Goal 
    {
        get => _goal;
        set => _goal = value;
    }

    public bool Visible
    {
        get => _visible;
        set => _visible = value;
    }

    public List<Tile> NeighbouringTiles
    {
        get => _neighbourTiles;
        private set => _neighbourTiles = value;
    }

    void Start()
    {
        NeighbouringTiles = FindNeighbouringTiles();
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (Unit.IsPlayerTurn)
        {
            if (_goal)
            {
                renderer.material.color = Color.red;
            }
            else if (_current)
            {
                renderer.material.color = Color.blue;
            }
            else if (_inhabited)
            {
                renderer.material.color = Color.black;
            }
            else if (_reachable)
            {
                renderer.material.color = Color.yellow;
            }
            else if (_visible)
            {
                renderer.material.color = Color.white;
            }
            else
            {
                renderer.material.color = Color.grey;
            }
        }
    }

    public void ResetTile(params string[] ignoredProperties)
    {
        List<PropertyInfo> properties = GetType().GetDeclaredProperties()
            .Where(prop => prop.Name != nameof(NeighbouringTiles)).ToList();

        if (ignoredProperties.Length == 0)
        {
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(this, false);
                }
            }

            return;
        }

        foreach (PropertyInfo property in properties)
        {
            foreach (string ignoredProp in ignoredProperties)
            {
                if (property.Name != ignoredProp)
                {
                    if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(this, false);
                    }
                }
            }
        }
    }

    private List<Tile> FindNeighbouringTiles()
    {
        Vector2 size = new Vector2(0.5f, 0.5f);
        List<Tile> tiles = new List<Tile>();
        int layerMask = 1 << LayerMask.NameToLayer("Tile");

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != y && x * y == 0)
                {
                    Collider2D collider = Physics2D.OverlapBox(transform.position + new Vector3(x, y), size, 0f, layerMask);

                    if (collider != null)
                    {
                        tiles.Add(collider.gameObject.GetComponent<Tile>());
                    }
                }
            }
        }

        return tiles;
    }
}
