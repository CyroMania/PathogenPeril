using UnityEngine;

public interface IInput
{
    public bool GetKeyDown(KeyCode key);
}
public class GameInput : IInput
{
    public bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }
}