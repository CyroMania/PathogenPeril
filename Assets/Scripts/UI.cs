using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Button DivideBtn;
    public Button EndTurnBtn;
    public TextMeshProUGUI WinTxt;
    public TextMeshProUGUI LoseTxt;

    private Animator _divideBtnAnim;
    private Animator _endTurnBtnAnim;
    private Animator _WinTxtAnim;
    private Animator _LoseTxtAnim;


    private void Start()
    {
        _divideBtnAnim = DivideBtn.GetComponent<Animator>();
        _endTurnBtnAnim = EndTurnBtn.GetComponent<Animator>();
        _WinTxtAnim = WinTxt.GetComponent<Animator>();
        _LoseTxtAnim = LoseTxt.GetComponent<Animator>();
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

    internal void GameWon()
    {
        PauseGameplay();
        _WinTxtAnim.SetTrigger("GameWon");
    }

    internal void GameLost()
    {
        PauseGameplay();
        _LoseTxtAnim.SetTrigger("GameLost");
    }

    private void PauseGameplay()
    {
        Time.timeScale = 0;
    }
}
