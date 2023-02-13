using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private float _zoom = 5;

    [SerializeField]
    private float _positionSpeedDampener = 12f;

    [SerializeField]
    private float _zoomSpeedDampener = 3;

    private void Start()
    {
        _zoom = GetComponent<Camera>().orthographicSize;
    }

    private void Update()
    {
        Vector2 movementVector = Vector2.zero;

        //Geneneral Positional Movement

        if (Input.GetKey(KeyCode.W))
        {
            movementVector += Vector2.up;
        }

        if (Input.GetKey(KeyCode.S))
        {
            movementVector += Vector2.down;
        }

        if (Input.GetKey(KeyCode.A))
        {
            movementVector += Vector2.left;
        }

        if (Input.GetKey(KeyCode.D))
        {
            movementVector += Vector2.right;
        }

        //Zoom Controls

        if (Input.mouseScrollDelta != Vector2.zero)
        {
            _zoom -= Input.mouseScrollDelta.y / _zoomSpeedDampener;
        }

        GetComponent<Camera>().orthographicSize = _zoom;
        transform.Translate(movementVector / _positionSpeedDampener);
    }
}
