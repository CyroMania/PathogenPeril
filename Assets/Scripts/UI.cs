using System;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private Animator _divideBtnAnim;
    private Button _divideBtn;

    private void Start()
    {
        _divideBtnAnim = gameObject.GetComponentInChildren<Animator>();
        _divideBtn = gameObject.GetComponentInChildren<Button>();
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
            _divideBtn.enabled = true;
        }
        else
        {
            _divideBtn.enabled = false;
        }
    }
}
