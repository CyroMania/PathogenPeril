using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private const string BorderName = "Border";
    private const string FillName = "FillBar"; 

    private void Start()
    {
        GameObject fill = new GameObject(FillName);
        fill.AddComponent<CanvasRenderer>();
        fill.AddComponent<Image>();
        fill.transform.SetParent(gameObject.transform);
        RectTransform fillTransform = fill.GetComponent<RectTransform>();
        fillTransform.anchoredPosition = new Vector2(0.5f, 0.5f);
        fillTransform.anchorMin = new Vector2(0.0f, 0.0f);
        fillTransform.anchorMax = new Vector2(1.0f, 1.0f);

        GameObject border = new GameObject(BorderName);
        border.transform.SetParent(gameObject.transform);
        border.AddComponent<CanvasRenderer>();
        border.AddComponent<Image>();

        Image borderImage = border.GetComponent<Image>();
        borderImage.sprite = Resources.Load<Sprite>("StatBar");
        borderImage.SetNativeSize(); //this will provide the x and y size of the health bar

        //Set parent to have same size
        gameObject.GetComponent<RectTransform>().sizeDelta = border.GetComponent<RectTransform>().sizeDelta;
    }
}
