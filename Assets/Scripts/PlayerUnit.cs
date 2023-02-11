using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    private Camera _mainCamera;
    private Collider2D _collider;
    [SerializeField]
    private bool _isMoving;
    [SerializeField]
    private Stack<Tile> _path;

    public bool Selected { get; set; }

    protected override void Init(short maxHitPoints, short maxMovementPoints)
    {
        base.Init(maxHitPoints, maxMovementPoints);

        Start();
    }

    void Start()
    {
        _isMoving = false;
        _collider = GetComponent<Collider2D>();
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (TargetTile != null)
        {
            if (!_isMoving)
            {
                _path = TileMovement.FindTilePath(CurrentTile, TargetTile, new Stack<Tile>());
                MovementPoints -= (short)_path.Count;
                _isMoving = true;
                return;
            }
            else
            {
                TileMovement.MoveToTile(this, _path);
                if (_path.Count == 0) 
                {
                    _isMoving = false;
                    SetTargetTileToCurrentTile();
                    ResetAllTiles(ignoreProperty: nameof(Tile.Current));
                    FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                }

                return;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 clickPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

            if (!Selected)
            {
                int unitMask = 1 << (int)LayerMask.NameToLayer("Unit");
                RaycastHit2D unitHitInfo = Physics2D.Raycast(clickPosition, Vector2.zero, 0, unitMask);

                if (unitHitInfo.collider == _collider)
                {
                    Selected = true;
                    ResetAllTiles();
                    CalculateCurrentTile();
                    FindSelectableTiles(CurrentTile, new Queue<Tile>(), 1);
                }

                return;
            }

            int tileMask = 1 << LayerMask.NameToLayer("Tile");
            RaycastHit2D tileHitInfo = Physics2D.Raycast(clickPosition, Vector2.zero, 0, tileMask);

            if (tileHitInfo.collider != null)
            {
                Tile selectedTile = tileHitInfo.collider.gameObject.GetComponent<Tile>();
                if (selectedTile.Reachable && !selectedTile.Current)
                {
                    TargetTile = selectedTile;
                    return;
                }
            }
        } 
    }

    private void CalculateCurrentTile()                   
    {
        int layerMask = 1 << LayerMask.NameToLayer("Tile");
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, Vector2.zero, 0, layerMask);
        GameObject target = hitInfo.collider.gameObject;

        Debug.Log(hitInfo.transform.name);

        if (target.layer == 3)
        {
            CurrentTile = target.GetComponent<Tile>();
            CurrentTile.Current = true;
        }
    }

    private void FindSelectableTiles(Tile tile, Queue<Tile> selectableTiles, int distance)
    {
        foreach (Tile t in tile.NeighbouringTiles)
        {
            if (distance <= MovementPoints && selectableTiles.Contains(t) == false)
            {
                t.Reachable = true;
                selectableTiles.Enqueue(tile);
                FindSelectableTiles(t, selectableTiles, distance + 1);
            }
        }

        if (selectableTiles.Count > 0)
        {
            selectableTiles.Dequeue();
        }
    }

    private void SetTargetTileToCurrentTile()
    {
        CurrentTile.Current = false;
        TargetTile.Current = true;
        CurrentTile = TargetTile;
        TargetTile = null;
    }
}
