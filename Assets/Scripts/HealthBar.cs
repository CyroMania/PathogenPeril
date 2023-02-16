using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private const string BorderName = "Border";
    private const string FillName = "FillBar";
    private void Start()
    {
        GameObject fill = new GameObject(FillName);
        fill.transform.SetParent(gameObject.transform);
        fill.AddComponent<CanvasRenderer>();
        fill.AddComponent<Image>();

        GameObject border = new GameObject(BorderName);
        border.transform.SetParent(gameObject.transform);
        border.AddComponent<CanvasRenderer>();
        border.AddComponent<Image>();

        Image borderImage = border.GetComponent<Image>();

        //set border sprite
        borderImage.SetNativeSize(); //this will provide the x and y size of the health bar

        //Set parent to have same size
        gameObject.GetComponent<RectTransform>().sizeDelta = border.GetComponent<RectTransform>().sizeDelta;
    }
}
