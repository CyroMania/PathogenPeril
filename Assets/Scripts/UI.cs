using System;
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
    [SerializeField]
    private GameObject _pauseMenuPanel;
    [SerializeField]
    private GameObject _finishedGamePanel;

    private Animator _divideBtnAnim;
    private Animator _endTurnBtnAnim;
    private Animator _winTxtAnim;
    private Animator _loseTxtAnim;
    private Animator _pauseMenuAnim;

    private static bool _gameOver;

    public readonly int RequiredSucceededUnits = 3;

    public static int SucceededUnits { get; set; }

    public static bool GameplayPaused { get; private set; }



    private void Start()
    {
        _gameOver = false;
        SucceededUnits = 0;
        GameplayPaused = false;
        _finishedGamePanel.SetActive(false);
        _pauseMenuPanel.SetActive(true);
        _winTxt.gameObject.SetActive(false);
        _loseTxt.gameObject.SetActive(false);
        _divideBtnAnim = _divideBtn.GetComponent<Animator>();
        _endTurnBtnAnim = _endTurnBtn.GetComponent<Animator>();
        _winTxtAnim = _winTxt.GetComponent<Animator>();
        _loseTxtAnim = _loseTxt.GetComponent<Animator>();
        _pauseMenuAnim = _pauseMenuPanel.GetComponent<Animator>();
        _scoreTxt.text = string.Concat(SucceededUnits, "/", RequiredSucceededUnits);
    }

    private void Update()
    {
        if (!_gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Unit.IsPlayerTurn && Unit.CheckAnyPlayerUnitSelected())
                {
                    PlayerUnit.DeselectAllUnits();
                    Unit.ResetAllTiles(new string[] { nameof(Tile.Goal), nameof(Tile.Visible) });
                    return;
                }

                if (!GameplayPaused)
                {
                    _pauseMenuAnim.SetBool("ShowWindow", true);
                }
                else
                {
                    _pauseMenuAnim.SetBool("ShowWindow", false);
                }

                PauseGameplay();
            }
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

    public void PauseGameplay()
    {
        GameplayPaused = !GameplayPaused;

        if (!GameplayPaused)
        {
            //Closes the window incase it is open.
            if (_pauseMenuAnim.GetBool("ShowWindow"))
            {
                _pauseMenuAnim.SetBool("ShowWindow", false);
            }

            _divideBtn.gameObject.SetActive(true);
            _endTurnBtn.gameObject.SetActive(true);
            Time.timeScale = 1;
        }
        else
        {
            _divideBtn.gameObject.SetActive(false);
            _endTurnBtn.gameObject.SetActive(false);
            Time.timeScale = 0;
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
        _gameOver = true;
        PauseGameplay();
        _winTxt.gameObject.SetActive(true);
        _winTxtAnim.SetTrigger("GameWon");
        _finishedGamePanel.SetActive(true);
    }

    internal void GameLost()
    {
        _gameOver = true;
        PauseGameplay();
        _loseTxt.gameObject.SetActive(true);
        _loseTxtAnim.SetTrigger("GameLost");
        _finishedGamePanel.SetActive(true);
    }
}
