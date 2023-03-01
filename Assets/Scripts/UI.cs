using UnityEngine;

public class UI : MonoBehaviour
{
    private Animator _divideBtnAnim;

    private void Start()
    {
        _divideBtnAnim = gameObject.GetComponentInChildren<Animator>();
    }

    public void NewTurn()
    {
        Unit.EndCurrentTurn();
    }

    public void DisplayButtons()
    {
        _divideBtnAnim.SetTrigger("UnitSelected");
    }
}
