using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private bool _isMoving;
    private bool _isZooming;
    private float _zoom = 5;
    private float _positionSpeedDampener = 12f;
    private float _zoomSpeedDampener = 3;
    private static bool _gameplayPaused = false;

    private const float _zoomMax = 6.5f;
    private const float _zoomMin = 3.0f;

    public bool IsMoving
    {
        get => _isMoving;
    }

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
        if (!_gameplayPaused)
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

            //We check this to confirm its ready to Update UI Elements
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
                //We must subtract for the zoom because the Y-Value is inverted for scrolling in our case
                //When we zoom in, we are shrinking the camera's size but this results in a greater zoom for example
                _zoom -= (Input.mouseScrollDelta.y * (_zoom / 10)) / _zoomSpeedDampener;

                if (_zoom > _zoomMax)
                {
                    _zoom = _zoomMax;
                    _isZooming = false;
                }
                else if (_zoom < _zoomMin)
                {
                    _zoom = _zoomMin;
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

    public static void GameIsPaused()
    {
        _gameplayPaused = true;
    }
}
