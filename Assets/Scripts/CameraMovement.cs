using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float _speedDampener = 12f;

    private void Update()
    {
        Vector2 movementVector = Vector2.zero;

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

        transform.Translate(movementVector / _speedDampener);
    }
}
