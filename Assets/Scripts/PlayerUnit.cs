using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Collider2D collider;

    public bool Selected { get; set; }

    protected override void Init(short maxHitPoints, short maxMovementPoints)
    {
        base.Init(maxHitPoints, maxMovementPoints);

        Start();
    }

    void Start()
    {
        collider = GetComponent<Collider2D>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 clickPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            int layerMask = 1 << (int)LayerMask.NameToLayer("Unit");
            RaycastHit2D hitInfo = Physics2D.Raycast(clickPosition, Vector2.zero, 0, layerMask);

            if (hitInfo.collider == collider)
            {
                CalculateCurrentTile();
                Queue<Tile> selectableTilesQueue = new Queue<Tile>();
                FindSelectableTiles(CurrentTile, selectableTilesQueue, 1);
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

    private void FindSelectableTiles(Tile tile, Queue<Tile> tileQueue, int distance)
    {
        foreach (Tile t in tile.NeighbouringTiles)
        {
            if (distance < MovementPoints && tileQueue.Contains(t) == false)
            {
                t.Reachable = true;
                tileQueue.Enqueue(tile);
                FindSelectableTiles(t, tileQueue, distance + 1);
            }
        }

        tileQueue.Dequeue();
    }
}
