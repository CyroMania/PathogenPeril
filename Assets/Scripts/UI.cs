using System.Reflection;
using TMPro;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField]
    private Button _divideBtn;
    [SerializeField]
    private Button _endTurnBtn;
    [SerializeField]
    private TextMeshProUGUI _winTxt;
    [SerializeField]
    private TextMeshProUGUI _loseTxt;
    [SerializeField]
    private TextMeshProUGUI _scoreTxt;

    private Animator _divideBtnAnim;
    private Animator _endTurnBtnAnim;
    private Animator _winTxtAnim;
    private Animator _loseTxtAnim;

    private const int RequiredSucceededUnits = 3;

    public static int SucceededUnits { get; set; } = 0;

    public static bool GameplayPaused { get; private set; } = false;

    private void Start()
    {
        _winTxt.gameObject.SetActive(false);
        _loseTxt.gameObject.SetActive(false);
        _divideBtnAnim = _divideBtn.GetComponent<Animator>();
        _endTurnBtnAnim = _endTurnBtn.GetComponent<Animator>();
        _winTxtAnim = _winTxt.GetComponent<Animator>();
        _loseTxtAnim = _loseTxt.GetComponent<Animator>();
        _scoreTxt.text = string.Concat(SucceededUnits, "/", RequiredSucceededUnits);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGameplay();
        }
    }

    public void NewTurn()
    {
        Unit.EndCurrentTurn();
    }

    public void UpdateScoreText(int increment)
    {
        SucceededUnits += increment;
        _scoreTxt.text = string.Concat(SucceededUnits, "/", RequiredSucceededUnits);

        if (SucceededUnits == RequiredSucceededUnits)
        {
            GameWon();
        }
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
            _divideBtn.interactable = true;
        }
        else
        {
            _divideBtn.interactable = false;
        }
    }

    internal void GameWon()
    {
        PauseGameplay();
        _winTxt.gameObject.SetActive(true);
        _winTxtAnim.SetTrigger("GameWon");
    }

    internal void GameLost()
    {
        PauseGameplay();
        _loseTxt.gameObject.SetActive(true);
        _loseTxtAnim.SetTrigger("GameLost");
    }

    public void PauseGameplay()
    {
        GameplayPaused = !GameplayPaused;

        if (!GameplayPaused)
        {
            _divideBtn.enabled = false;
            _endTurnBtn.enabled = false;
            Time.timeScale = 0;
        }
        else
        {
            _divideBtn.enabled = true;
            _endTurnBtn.enabled = true;
            Time.timeScale = 1;
        }
    }
}
