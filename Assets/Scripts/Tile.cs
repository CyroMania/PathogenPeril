using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// The objects in the game that the units position and move themselves on.
/// </summary>
public class Tile : MonoBehaviour
{
    private static readonly Vector2 _size = new Vector2(0.5f, 0.5f);

    //These are serialized for debugging purposes.
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
    [SerializeField]
    private List<Tile> _neighbourTiles;
    private Renderer _renderer;


    /// <summary>
    /// True if it's the current tile of the currently selected player unit.
    /// </summary>
    public bool Current
    {
        get => _current;
        set => _current = value;
    }

    /// <summary>
    /// True if unit is selected and another unit inhabits a reachable tile.
    /// </summary>
    public bool Inhabited
    {
        get => _inhabited;
        set => _inhabited = value;
    }

    /// <summary>
    /// True if the unit has enough movement points to reach the target tile.
    /// </summary>
    public bool Reachable
    {
        get => _reachable;
        set => _reachable = value;
    }


    /// <summary>
    /// True if the tile represents the win condition for the player units.
    /// </summary>
    public bool Goal
    {
        get => _goal;
        set => _goal = value;
    }

    /// <summary>
    /// True if the tile is within the selected unit's visibility range.
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set => _visible = value;
    }

    /// <summary>
    /// All tiles that are directly adjacent to the current tile.
    /// </summary>
    public List<Tile> NeighbouringTiles
    {
        get => _neighbourTiles;
        private set => _neighbourTiles = value;
    }

    private void Start()
    {
        NeighbouringTiles = FindNeighbouringTiles();
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        //This determines the colour of tile based on different state priority
        //TODO: Refactor code into a method and provide calls to it to remove this from the update function
        if (Unit.IsPlayerTurn)
        {
            if (_goal)
            {
                if (_reachable)
                {
                    _renderer.material.color = Vector4.Lerp(Color.red, Color.yellow, 0.7f);
                }
                else
                {
                    _renderer.material.color = Color.red;
                }
            }
            else if (_current)
            {
                _renderer.material.color = Color.blue;
            }
            else if (_inhabited)
            {
                _renderer.material.color = Color.black;
            }
            else if (_reachable)
            {
                _renderer.material.color = Color.yellow;
            }
            else if (_visible)
            {
                _renderer.material.color = Color.white;
            }
            else
            {
                _renderer.material.color = Color.grey;
            }
        }
    }

    /// <summary>
    /// Resets all of a tiles declared properties with restrictions.
    /// </summary>
    /// <param name="ignoredProperties">The properties to ignore or not reset.</param>
    public void ResetTile(string[] ignoredProperties) 
    {
        //We use reflection and retrieve declared properties to enumerate over
        List<PropertyInfo> properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
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
            if (ignoredProperties.Contains(property.Name))
            {
                continue;
            }

            if (property.PropertyType == typeof(bool))
            {
                property.SetValue(this, false);
            }
        }
    }

    /// <summary>
    /// Finds all directly adjacent tiles to the current tile.
    /// </summary>
    /// <returns>A list of tiles.</returns>
    private List<Tile> FindNeighbouringTiles()
    {
        List<Tile> tiles = new List<Tile>();
        int layerMask = 1 << LayerMask.NameToLayer("Tile");

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                //this check is performant and guarantees an adjacent tile because each one will have at least an x or y value of zero
                //as we are not including corner neighbours. We still need to compare that they are not equal otherwise we will have
                // a reference to the current tile in its own neighbourTiles list.
                if (x != y && x * y == 0)
                {
                    Collider2D collider = Physics2D.OverlapBox(transform.position + new Vector3(x, y), size, 0f, layerMask);

                    //This is true when we are at the edge of the board.
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