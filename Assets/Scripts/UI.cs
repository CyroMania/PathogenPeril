using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manipulates ui components based on specific input.
/// </summary>
public class UI : MonoBehaviour
{
    internal const int RequiredSucceededUnits = 3;
    private const string ShowWindowAnimBool = "ShowWindow";

    private static bool _gameOver;

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

    //Needed for Tests
    public ITime TimeService { get; set; }
    public IInput InputService { get; set; }
    public IUI UIService { get; set; }

    /// <summary>
    /// The number of units that have made it into the blood stream.
    /// </summary>
    internal static int SucceededUnits { get; set; }

    /// <summary>
    /// True if the gameplay is currently paused.
    /// </summary>
    internal static bool GameplayPaused { get; private set; }

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

        //nameof Keyword allows us to create more maintainable code without creating lots of constants.
        UIService.SetActive(nameof(_helpMenuPanel), true);
        UIService.SetActive(nameof(_finishedGamePanel), false);
        UIService.SetActive(nameof(_pauseMenuPanel), true);
        UIService.SetActive(nameof(_winTxt), false);
        UIService.SetActive(nameof(_loseTxt), false);

        UIService.SetText(nameof(_scoreTxt), string.Concat(SucceededUnits, "/", RequiredSucceededUnits));
    }

    private void Update()
    {
        if (!_gameOver && !_helpMenuPanel.activeSelf)
        {
            if (InputService.GetKeyDown(KeyCode.Escape))
            {
                //This section handles deselection of a unit if one is selected.
                if (Unit.IsPlayerTurn && Unit.CheckAnyPlayerUnitSelected())
                {
                    PlayerUnit.DeselectAllUnits();
                    Unit.ResetAllTiles(new string[] { nameof(Tile.Goal), nameof(Tile.Visible) });
                    return;
                }

                //This shows the pause menu if hidden, or hides it if it displayed.
                if (!GameplayPaused)
                {
                    UIService.SetAnimBool(nameof(_pauseMenuPanel), ShowWindowAnimBool, true);
                }
                else
                {
                    UIService.SetAnimBool(nameof(_pauseMenuPanel), ShowWindowAnimBool, false);
                }

                PauseGameplay();
            }
        }
    }

    /// <summary>
    /// Begins a new turn.
    /// </summary>
    public void NewTurn()
    {
        Unit.EndCurrentTurn();
    }

    /// <summary>
    /// Increases the onscreen score by some value.
    /// </summary>
    /// <param name="increment">the number of points to increase the score by.</param>
    public void UpdateScoreText(int increment)
    {
        SucceededUnits += increment;
        UIService.SetText(nameof(_scoreTxt), string.Concat(SucceededUnits, "/", RequiredSucceededUnits));

        if (SucceededUnits == RequiredSucceededUnits)
        {
            GameWon();
        }
    }

    /// <summary>
    /// Pauses or unpauses the gameplay.
    /// </summary>
    public void PauseGameplay()
    {
        //This will reverse the current state.
        GameplayPaused = !GameplayPaused;

        if (!GameplayPaused)
        {
            //Closes the window incase it is open.
            if (UIService.GetAnimBool(nameof(_pauseMenuPanel), ShowWindowAnimBool))
            {
                UIService.SetAnimBool(nameof(_pauseMenuPanel), ShowWindowAnimBool, true);
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

    /// <summary>
    /// Controls whether the help info menu appears or not.
    /// </summary>
    /// <param name="display">Shows the menu if true, hides the menu if false.</param>
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
            if (!UIService.GetAnimBool(nameof(_pauseMenuPanel), ShowWindowAnimBool))
            {
                GameplayPaused = false;
            }

            UIService.SetActive(nameof(_helpMenuPanel), false);
        }
    }

    /// <summary>
    /// Controls whether a specific button appears or not.
    /// </summary>
    /// <param name="button">The button to show or hide.</param>
    /// <param name="shouldDisplay">Shows the menu if true, hides the menu if false.</param>
    internal void DisplayButton(string button, bool shouldDisplay)
    {
        if (shouldDisplay)
        {
            UIService.ResetAnimTrigger(button, "Hide");
            UIService.SetAnimTrigger(button, "Show");
        }
        else
        {
            UIService.ResetAnimTrigger(button, "Show");
            UIService.SetAnimTrigger(button, "Hide");
        }
    }

    /// <summary>
    /// Confirms buttons are usable with the given parameters.
    /// </summary>
    /// <param name="currentEnergy">The remaining energy of the current selected unit.</param>
    /// <param name="maxEnergy">The selected unit's max energy level.</param>
    /// <param name="fullySurrounded">True if the unit has no available neighbouring tiles.</param>
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

    /// <summary>
    /// Manipulates the UI if the player wins the game.
    /// </summary>
    private void GameWon()
    {
        _gameOver = true;
        PauseGameplay();
        UIService.SetActive(nameof(_winTxt), true);
        UIService.SetActive(nameof(_finishedGamePanel), true);
        UIService.SetAnimTrigger(nameof(_winTxt), "GameWon");
    }

    /// <summary>
    /// Manipulates the UI if the player loses the game.
    /// </summary>
    internal void GameLost()
    {
        _gameOver = true;
        PauseGameplay();
        UIService.SetActive(nameof(_loseTxt), true);
        UIService.SetActive(nameof(_finishedGamePanel), true);
        UIService.SetAnimTrigger(nameof(_loseTxt), "GameLost");
    }
}