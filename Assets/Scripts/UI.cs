using System;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Button DivideBtn;
    public Button EndTurnBtn;

    private Animator _divideBtnAnim;
    private Animator _endTurnBtnAnim;


    private void Start()
    {
        _divideBtnAnim = DivideBtn.GetComponent<Animator>();
        _endTurnBtnAnim= EndTurnBtn.GetComponent<Animator>();
    }

    public void NewTurn()
    {
        Unit.EndCurrentTurn();
    }

    internal void DisplayButtons()
    {
        _divideBtnAnim.ResetTrigger("UnitDeselected");
        _divideBtnAnim.SetTrigger("UnitSelected");
    }

    internal void HideButtons()
    {
        _divideBtnAnim.ResetTrigger("UnitSelected");
        _divideBtnAnim.SetTrigger("UnitDeselected");
    }

    internal void CheckButtonsUsable(short currentEnergy, short maxEnergy)
    { 
        if (currentEnergy == maxEnergy)
        {
            DivideBtn.interactable = true;
        }
        else
        {
            DivideBtn.interactable = false;
        }
    }
}
