using NSubstitute;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

[TestFixture]
public class UITests
{
    //private const string _succeededUnitsProperty = "SucceededUnits";
    //private const string _gameplayPausedProperty = "GameplayPaused";

    //[UnityTest]
    //public IEnumerator Await_Invoked_SuccessfulUnitsSetToZero()
    //{
    //    GameObject canvas = new GameObject();
    //    UI ui = canvas.AddComponent<UI>();
    //    typeof(UI).GetProperty(_succeededUnitsProperty).SetValue(ui, -3);

    //    yield return new WaitForEndOfFrame(); //Await

    //    Assert.AreEqual(0, typeof(UI).GetProperty(_succeededUnitsProperty).GetValue(ui));
    //}

    //[UnityTest]
    //public IEnumerator Await_Invoked_GameplayIsFrozen()
    //{
    //    GameObject canvas = new GameObject();
    //    UI ui = canvas.AddComponent<UI>();
    //    typeof(UI).GetProperty(_gameplayPausedProperty).SetValue(ui, true);

    //    yield return new WaitForEndOfFrame(); //Await

    //    Assert.AreEqual(true, typeof(UI).GetProperty(_gameplayPausedProperty).GetValue(ui)  );
    //}

    [UnityTest]
    public IEnumerator Await_Invoked_AppropriateUIElementsAreMadeActiveOrInactive()
    {
        GameObject canvas = new GameObject();
        UI ui = canvas.AddComponent<UI>();
        GameObject _pausePanel = new GameObject();
        GameObject _finishedGamePanel = new GameObject();
        GameObject _helpMenuPanel = new GameObject();
        GameObject _divideBtn = new GameObject();
        _divideBtn.AddComponent<Button>();
        GameObject _endTurnBtn = new GameObject();
        _endTurnBtn.AddComponent<Button>();
        GameObject _winTxt = new GameObject();
        _winTxt.AddComponent<TextMeshProUGUI>();
        TextMeshProUGUI _loseTxt = new TextMeshProUGUI();
        TextMeshProUGUI _scoreTxt = new TextMeshProUGUI();

        IUI uiElements = Substitute.For<GameUI>(_pausePanel, _finishedGamePanel, _helpMenuPanel, _divideBtn.GetComponent<Button>(), _endTurnBtn.GetComponent<Button>(), _winTxt, _loseTxt, _scoreTxt);
        ui.UIService = uiElements;
        //typeof(UI).GetProperty(_gameplayPausedProperty).SetValue(ui, true);


        yield return new WaitForEndOfFrame(); //Await

        //Assert.AreEqual(true, typeof(UI).GetProperty("_scoreTxt").GetValue(ui));
    }

    //[UnityTest]
    //public IEnumerator EscapeKeyPressed_GameIsNotPaused_GameplayIsFrozen()
    //{
    //    GameObject canvas = new GameObject();
    //    UI ui = canvas.AddComponent<UI>();
    //    typeof(UI).GetProperty(_gameplayPausedProperty).SetValue(ui, true);
    //    IInput inputService = Substitute.For<IInput>();
    //    inputService.GetKeyDown(KeyCode.Escape).Returns(true);
    //    ui.InputService = inputService;

    //    yield return new WaitForEndOfFrame(); //Await

    //    Assert.AreEqual(true, typeof(UI).GetProperty(_gameplayPausedProperty).GetValue(ui));
    //    Assert.AreEqual(0, Time.timeScale);
    //}

    //[UnityTest]
    //public IEnumerator EscapeKeyPressed_GameIsPaused_GameplayIsNotFrozen()
    //{
    //    GameObject canvas = new GameObject();
    //    UI ui = canvas.AddComponent<UI>();
    //    typeof(UI).GetProperty(_gameplayPausedProperty).SetValue(ui, true);

    //    yield return new WaitForEndOfFrame(); //Await

    //    Assert.AreEqual(true, typeof(UI).GetProperty(_gameplayPausedProperty).GetValue(ui));
    //}

    //[UnityTest]
    //public IEnumerator EscapeKeyPressed_IsPlayerTurnAndUnitSelected_UnitDeselected()
    //{
    //    GameObject canvas = new GameObject();
    //    UI ui = canvas.AddComponent<UI>();
    //    typeof(UI).GetProperty(_gameplayPausedProperty).SetValue(ui, true);

    //    yield return new WaitForEndOfFrame(); //Await

    //    Assert.AreEqual(true, typeof(UI).GetProperty(_gameplayPausedProperty).GetValue(ui));
    //}
}