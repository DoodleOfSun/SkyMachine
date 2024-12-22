using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.UI;

public class ButtonImageChanger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Sprite normalButtonSprite;
    public Sprite pressedButtonSprite;

    private Image image;
    private Text text;
    private Color currentColor;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        text = this.GetComponentInChildren<Text>();

        if (text != null)
        {
            currentColor = text.color;
        }
    }

    public void OnPointerDown(PointerEventData data)
    {
        image.sprite = pressedButtonSprite;
        if (text != null)
        {
            text.color = Color.black;
        }
    }

    public void OnPointerUp(PointerEventData data)
    {
        image.sprite = normalButtonSprite;
        if (text != null)
        {
            text.color = currentColor;
        }
    }
}
