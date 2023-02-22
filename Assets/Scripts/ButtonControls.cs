using UnityEngine;
using UnityEngine.UI;

public class ButtonControls : MonoBehaviour
{
   public void NewTurn()
    {
        Unit.EndCurrentTurn();
    }
}
