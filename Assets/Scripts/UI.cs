using System;
using System.Reflection;
using TMPro;
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
    [SerializeField]
    private GameObject _helpMenuPanel;

    private static bool _gameOver;

    public readonly int RequiredSucceededUnits = 3; 


    //Needed for Tests
    public ITime TimeService { get; set; }
    public IInput InputService { get; set; }
    public IUI UIService { get; set; }

    public static int SucceededUnits { get; set; }

    public static bool GameplayPaused { get; private set; }

    private void Awake()
    {
        Unit.StaticsSetup = false;
        _gameOver = false;
        SucceededUnits = 0;
        GameplayPaused = true;

        if (TimeService == null)
        {
            TimeService = new GameTime();
        }

        if (InputService == null)
        {
            InputService = new GameInput();
        }

        if (UIService == null)
        {
            UIService = new GameUI(_pauseMenuPanel, _finishedGamePanel, _helpMenuPanel,
                _divideBtn, _endTurnBtn, _winTxt, _loseTxt, _scoreTxt);
        }

        UIService.SetActive(nameof(_helpMenuPanel), true);
        UIService.SetActive(nameof(_finishedGamePanel), false);
        UIService.SetActive(nameof(_pauseMenuPanel), true);
        UIService.SetActive(nameof(_winTxt), false);
        UIService.SetActive(nameof(_loseTxt), false);

        UIService.SetText("_scoreTxt", string.Concat(SucceededUnits, "/", RequiredSucceededUnits));
    }

    private void Update()
    {
        if (!_gameOver && !_helpMenuPanel.activeSelf)
        {
            if (InputService.GetKeyDown(KeyCode.Escape))
            {
                if (Unit.IsPlayerTurn && Unit.CheckAnyPlayerUnitSelected())
                {
                    PlayerUnit.DeselectAllUnits();
                    Unit.ResetAllTiles(new string[] { nameof(Tile.Goal), nameof(Tile.Visible) });
                    return;
                }

                if (!GameplayPaused)
                {
                    UIService.SetAnimBool(nameof(_pauseMenuPanel), "ShowWindow", true);
                }
                else
                {
                    UIService.SetAnimBool(nameof(_pauseMenuPanel), "ShowWindow", false);
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
        UIService.SetText(nameof(_scoreTxt), string.Concat(SucceededUnits, "/", RequiredSucceededUnits));

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
            if (UIService.GetAnimBool(nameof(_pauseMenuPanel), "ShowWindow"))
            {
                UIService.SetAnimBool(nameof(_pauseMenuPanel), "ShowWindow", true);
            }

            UIService.SetActive(nameof(_divideBtn), true);
            UIService.SetActive(nameof(_endTurnBtn), true);
            Time.timeScale = TimeService.SetScale(1);
        }
        else
        {
            UIService.SetActive(nameof(_divideBtn), false);
            UIService.SetActive(nameof(_endTurnBtn), false);
            Time.timeScale = TimeService.SetScale(0);
        }
    }

    public void DisplayInfoMenu(bool display)
    {
        if (display)
        {
            GameplayPaused = true;
            UIService.SetActive(nameof(_helpMenuPanel), true);
        }
        else
        {
            //If the game is not already paused with an open pause menu, we can unpause the game
            if (!UIService.GetAnimBool(nameof(_pauseMenuPanel), "ShowWindow"))
            {
                GameplayPaused = false;
            }

            UIService.SetActive(nameof(_helpMenuPanel),false);
        }
    }

    internal void DisplayButton(string button, bool shouldDisplay)
    {
        FieldInfo[] fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
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

    internal void CheckButtonsUsable(short currentEnergy, short maxEnergy, bool fullySurrounded)
    {
        if (currentEnergy == maxEnergy && !fullySurrounded)
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
        UIService.SetActive(nameof(_winTxt), true);
        UIService.SetAnimTrigger(nameof(_winTxt),"GameWon");
        UIService.SetActive(nameof(_finishedGamePanel), true);
    }

    internal void GameLost()
    {
        _gameOver = true;
        PauseGameplay();
        UIService.SetActive(nameof(_loseTxt), true);
        UIService.SetAnimTrigger(nameof(_loseTxt), "GameLost");
        UIService.SetActive(nameof(_finishedGamePanel), true);
    }
}
