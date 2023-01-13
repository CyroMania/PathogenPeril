using Unity.VisualScripting;
using UnityEngine;

public class PlayerUnit : Unit
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Collider2D collider;

    public PlayerUnit(short maxHitPoints, short maxMovementPoints)
        : base(maxHitPoints, maxMovementPoints)
    {
    }

    public bool Selected { get; set; }

    private void Start()
    {
        collider = GetComponent<Collider2D>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 clickPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            int layerMask = 1 << (int)LayerMask.NameToLayer("Unit");
            RaycastHit2D hitInfo = Physics2D.Raycast(clickPosition, Vector2.zero, 0, layerMask);

            if (hitInfo.collider == collider)
            {
                CalculateCurrentTile();
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
}
