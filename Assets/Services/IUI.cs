using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IUI
{
    public void SetText(string field, string value);

    public void SetActive(string field, bool active);

    public void SetAnimBool(string field, string animValue, bool animBool);

    public bool GetAnimBool(string field, string animValue);

    public void SetAnimTrigger(string field, string animValue);

    public void ResetAnimTrigger(string field, string animValue);
}

public class GameUI : IUI
{
    private GameObject _pauseMenuPanel;
    private GameObject _finishedGamePanel;
    private GameObject _helpMenuPanel;

    private Button _divideBtn;
    private Button _endTurnBtn;

    private TextMeshProUGUI _winTxt;
    private TextMeshProUGUI _loseTxt;
    private TextMeshProUGUI _scoreTxt;

    public GameUI(GameObject pausePanel,
        GameObject finishedGamePanel, 
        GameObject helpMenuPanel,
        Button divideBtn,
        Button endTurnBtn,
        TextMeshProUGUI winTxt,
        TextMeshProUGUI loseTxt,
        TextMeshProUGUI scoreTxt)
    {
        _pauseMenuPanel = pausePanel;
        _finishedGamePanel = finishedGamePanel;
        _helpMenuPanel = helpMenuPanel;
        _divideBtn = divideBtn;
        _endTurnBtn = endTurnBtn;
        _winTxt = winTxt;
        _loseTxt = loseTxt;
        _scoreTxt = scoreTxt;
    }

    public bool GetAnimBool(string field, string animValue)
    {
        FieldInfo gameObject = GetDeclaredField(field);
        Animator animator = null;

        if (gameObject.GetValue(this) as GameObject != null)
        {
            animator = (gameObject.GetValue(this) as GameObject).GetComponent<Animator>();
        }
        else
        {
            Debug.Log("Animator couldn't be found on gameObject " + field + ".");
        }

        return animator.GetBool(animValue);
    }

    public void ResetAnimTrigger(string field, string animValue)
    {
        FieldInfo gameObject = GetDeclaredField(field);

        if (gameObject.GetValue(this) as Button != null)
        {
            Animator animator = (gameObject.GetValue(this) as Button).GetComponent<Animator>();
            animator.ResetTrigger(animValue);
        }
        else
        {
            Debug.Log("Couldn't find animator on " + field + " gameObject.");
        }
    }

    public void SetActive(string field, bool active)
    {
        FieldInfo uiElement = GetDeclaredField(field);

        if (uiElement.GetValue(this) as GameObject != null)
        {
            GameObject uiText = uiElement.GetValue(this) as GameObject;
            uiText.SetActive(active);
        }
        else if (uiElement.GetValue(this) as TextMeshProUGUI != null)
        {
            GameObject parent = (uiElement.GetValue(this) as TextMeshProUGUI).gameObject;
            parent.SetActive(active);
        }
        else if (uiElement.GetValue(this) as Button != null)
        {
            GameObject parent = (uiElement.GetValue(this) as Button).gameObject;
            parent.SetActive(active);
        }
        else
        {
            Debug.Log("Couldn't convert " + field + " to game object.");
        }
    }

    public void SetAnimBool(string field, string animBool, bool value)
    {
        FieldInfo gameObject = GetDeclaredField(field);

        if (gameObject.GetValue(this) as GameObject != null)
        {
            Animator animator = (gameObject.GetValue(this) as GameObject).GetComponent<Animator>();
            animator.SetBool(animBool, value);
        }
        else
        {
            Debug.Log("Couldn't convert find animator on " + field + " gameObject.");
        }
    }

    public void SetText(string field, string value)
    {
        FieldInfo textElement = GetDeclaredField(field);

        if (textElement.GetValue(this) as TextMeshProUGUI != null)
        {
            TextMeshProUGUI uiText = textElement.GetValue(this) as TextMeshProUGUI;
            uiText.text = value;
        }
        else
        {
            Debug.Log("Couldn't convert to TextMeshPro Element.");
        }
    }

    public void SetAnimTrigger(string field, string animValue)
    {
        FieldInfo gameObject = GetDeclaredField(field);

        if (gameObject.GetValue(this) as Button != null)
        {
            Animator animator = (gameObject.GetValue(this) as Button).gameObject.GetComponent<Animator>();
            animator.SetTrigger(animValue);
        }
        else if (gameObject.GetValue(this) as TextMeshProUGUI)
        {
            Animator animator = (gameObject.GetValue(this) as TextMeshProUGUI).gameObject.GetComponent<Animator>();
            animator.SetTrigger(animValue);
        }
        else
        {
            Debug.Log("Couldn't find animator on " + field + " gameObject.");
        }
    }

    private FieldInfo GetDeclaredField(string field)
    {
        return typeof(GameUI).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }
}