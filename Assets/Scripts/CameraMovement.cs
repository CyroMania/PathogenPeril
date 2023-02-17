using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private bool _isMoving;
    private float _zoom = 5;
    private float _positionSpeedDampener = 12f;
    private float _zoomSpeedDampener = 3;

    public bool IsMoving
    {
        get => _isMoving;
        set => _isMoving = value;
    }

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

        if (movementVector != Vector2.zero)
        {
            _isMoving = true;
        }
        else
        {
            _isMoving = false;
        }

        //Zoom Controls

        if (Input.mouseScrollDelta != Vector2.zero)
        {
            _zoom -= (Input.mouseScrollDelta.y * (_zoom / 10)) / _zoomSpeedDampener;
        }

        GetComponent<Camera>().orthographicSize = _zoom;
        transform.Translate(movementVector / _positionSpeedDampener);
    }
}
