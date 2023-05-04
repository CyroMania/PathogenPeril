 using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private bool _isMoving;
    private bool _isZooming;
    private float _zoom = 5;
    private float _positionSpeedDampener = 12f;
    private float _zoomSpeedDampener = 3;

    private const float ZoomMax = 6.5f;
    private const float ZoomMin = 3.0f;

    /// <summary>
    /// Is true if the camera is moving.
    /// </summary>
    public bool IsMoving
    {
        get => _isMoving;
    }

    /// <summary>
    /// Is true if the camera is zooming.
    /// </summary>
    public bool IsZooming
    {
        get => _isZooming;
    }

    private void Start()
    {
        _zoom = GetComponent<Camera>().orthographicSize;
    }

    private void Update()
    {
        if (!UI.GameplayPaused)
        {
            Vector2 movementVector = Vector2.zero;

            //Geneneral Positional Movement

            //This setup is done with separate if blocks so that the player has direct control with each button.
            //this way if the player holds down two opposite keys they won't move at all.
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                movementVector += Vector2.up;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                movementVector += Vector2.down;
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                movementVector += Vector2.left;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                movementVector += Vector2.right;
            }

            //We check this to confirm its ready to Update UI Elements.
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
                //We must subtract for the zoom because scrolling is an inverted value.
                //When we zoom in, we are shrinking the camera's size but this results in a greater zoom for example.
                _zoom -= (Input.mouseScrollDelta.y * (_zoom / 10)) / _zoomSpeedDampener;

                //Check to see if the zoom is within the limits.
                if (_zoom > ZoomMax)
                {
                    _zoom = ZoomMax;
                    _isZooming = false;
                }
                else if (_zoom < ZoomMin)
                {
                    _zoom = ZoomMin;
                    _isZooming = false;
                }
                else
                {
                    _isZooming = true;
                }
            }
            else
            {
                _isZooming = false;
            }

            GetComponent<Camera>().orthographicSize = _zoom;
            transform.Translate(movementVector / _positionSpeedDampener);
        }
    }
}