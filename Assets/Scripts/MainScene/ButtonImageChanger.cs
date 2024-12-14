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

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData data)
    {
        image.sprite = pressedButtonSprite;
    }

    public void OnPointerUp(PointerEventData data)
    {
        image.sprite = normalButtonSprite;
    }
}
