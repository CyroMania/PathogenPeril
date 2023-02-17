using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    private const string BorderName = "Border";
    private const string FillName = "FillBar";

    private static readonly Vector2 zeroVector = Vector2.zero;
    private static readonly Vector2 oneVector = Vector2.one;
    private static readonly Vector2 sizeOffset = new Vector2(0.25f, 0.25f);
    private static readonly Vector2 statBarFromBorderOffset = new Vector2(0.99f, 0.95f);

    private void Start()
    {
        GameObject fill = new GameObject(FillName);
        fill.transform.SetParent(gameObject.transform);
        fill.AddComponent<CanvasRenderer>();
        fill.AddComponent<Image>();
        fill.GetComponent<Image>().color = Color.red;

        GameObject border = new GameObject(BorderName);
        border.transform.SetParent(gameObject.transform);
        border.AddComponent<CanvasRenderer>();
        border.AddComponent<Image>();

        Image borderImage = border.GetComponent<Image>();
        borderImage.sprite = Resources.Load<Sprite>("StatBar");
        borderImage.SetNativeSize(); //this will provide the x and y size of the health bar
        border.GetComponent<RectTransform>().sizeDelta *= sizeOffset;

        gameObject.GetComponent<RectTransform>().sizeDelta = border.GetComponent<RectTransform>().sizeDelta * statBarFromBorderOffset;

        RectTransform fillTransform = fill.GetComponent<RectTransform>();
        fillTransform.offsetMin = zeroVector;
        fillTransform.offsetMax = zeroVector;
        fillTransform.anchoredPosition = new Vector2(0.5f, 0.5f);
        fillTransform.anchorMin = zeroVector;
        fillTransform.anchorMax = oneVector;

        Slider barSlider = gameObject.GetComponent<Slider>();
        barSlider.transition = Selectable.Transition.None;
        barSlider.interactable = false;
        barSlider.fillRect = fillTransform;
        barSlider.value = 1;
    }
}
