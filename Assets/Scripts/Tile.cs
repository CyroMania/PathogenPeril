using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private bool _reachable = false;
    [SerializeField]
    private bool _inhabited = false;
    [SerializeField]
    private bool _current = false;
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

    public List<Tile> NeighbouringTiles 
    {
        get { return _neighbourTiles; }
        private set
        {
            _neighbourTiles = value;
        }
    }

    void Start()
    {
        NeighbouringTiles = FindNeighbouringTiles();
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (_current)
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
        else
        {
            renderer.material.color = Color.white;
        }
    }

    public void ResetTile(string ignoreProperty = "")
    {
        List<PropertyInfo> properties = GetType().GetDeclaredProperties()
            .Where(prop => prop.Name != nameof(NeighbouringTiles)).ToList();

        foreach (PropertyInfo property in properties)
        {
            if (property.Name != ignoreProperty)
            {
                if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(this, false);
                }
            }
        }
    }

    private List<Tile> FindNeighbouringTiles()
    {
        Vector2 size = new Vector2(0.5f, 0.5f);
        List<Tile> list = new List<Tile>();
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
                        list.Add(collider.gameObject.GetComponent<Tile>());
                    }
                }
            }
        }

        return list;
    }
}
