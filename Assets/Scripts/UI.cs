using System.IO;
using System.Reflection;
using TMPro;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField]
    private Button DivideBtn;
    [SerializeField]
    private Button EndTurnBtn;
    [SerializeField]
    private TextMeshProUGUI WinTxt;
    [SerializeField]
    private TextMeshProUGUI LoseTxt;

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

    internal void DisplayButton(string button, bool shouldDisplay)
    {
        FieldInfo[] fields = GetType().GetDeclaredFields();
        Animator anim = null;

        foreach (FieldInfo field in fields)
        {
            if (field.GetType() == typeof(Animator))
            {
                if (field.Name == button)
                {
                    anim = field.GetValue(field) as Animator;
                }
            }
        }

        if (anim != null)
        {
            if (shouldDisplay)
            {
                anim.SetTrigger("Show");
            }
            else
            {
                anim.SetTrigger("Hide");
            }
        }
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
