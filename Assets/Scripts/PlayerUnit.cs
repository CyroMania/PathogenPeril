using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    [SerializeField]
    private Camera _mainCamera;
    [SerializeField]
    private Collider2D collider;
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
        collider = GetComponent<Collider2D>();
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (TargetTile != null)
        {
            if (!_isMoving)
            {
                _path = TileMovement.FindTilePath(CurrentTile, TargetTile, new Stack<Tile>());
                _isMoving = true;
                return;
            }
            else
            {
                TileMovement.MoveToTile(this, _path);

                if (_path.Count == 0) 
                {
                    _isMoving = false;
                    CurrentTile = TargetTile;
                    TargetTile = null;
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 clickPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

            if (!Selected)
            {
                int unitMask = 1 << (int)LayerMask.NameToLayer("Unit");
                RaycastHit2D unitHitInfo = Physics2D.Raycast(clickPosition, Vector2.zero, 0, unitMask);

                if (unitHitInfo.collider == collider)
                {
                    Selected = true;
                    CalculateCurrentTile();
                    Queue<Tile> selectableTiles = new Queue<Tile>();
                    FindSelectableTiles(CurrentTile, selectableTiles, 1);
                }

                return;
            }

            int tileMask = 1 << (int)LayerMask.NameToLayer("Tile");
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
        int layerMask = 1 << (int)LayerMask.NameToLayer("Tile");
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, Vector2.zero, 0, layerMask);
        GameObject target = hitInfo.collider.gameObject;

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
}
