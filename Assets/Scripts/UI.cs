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
        WinTxt.gameObject.SetActive(false);
        LoseTxt.gameObject.SetActive(false);
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
            if (field.Name == button)
            {
                anim = (Animator)field.GetValue(this);
                break;
            }
        }

        if (anim != null)
        {
            anim.gameObject.GetComponent<Button>().interactable = shouldDisplay;

            if (shouldDisplay)
            {
                anim.ResetTrigger("Hide");
                anim.SetTrigger("Show");
            }
            else
            {
                anim.ResetTrigger("Show");
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
        WinTxt.gameObject.SetActive(true);
        _WinTxtAnim.SetTrigger("GameWon");
    }

    internal void GameLost()
    {
        PauseGameplay();
        LoseTxt.gameObject.SetActive(true);
        _LoseTxtAnim.SetTrigger("GameLost");
    }

    private void PauseGameplay()
    {
        Time.timeScale = 0;
    }
}
