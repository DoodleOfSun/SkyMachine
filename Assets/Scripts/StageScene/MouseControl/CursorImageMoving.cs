using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class CursorImageMoving : MonoBehaviour
{
    public Image magicCircleImage;      // 마법진 이미지
    public float circleRotateSpeed;    // 마법진 회전 속도

    private Image cursorImage;          // 커서 이미지 인스턴스 (이 스크립트가 포함된 오브젝트의 Image Component)

    void Start()
    {
        cursorImage = GetComponent<Image>();
        cursorImage.enabled = true;
        magicCircleImage.enabled = false;
    }


    void FixedUpdate()
    {
        if (Player.instance.isParryAiming && Player.instance.parryCoroutine == null)
        {
            cursorImage.enabled = false;
            magicCircleImage.enabled = true;

            // 마법진 회전
            magicCircleImage.rectTransform.localEulerAngles += new Vector3(0, 0, circleRotateSpeed * Time.fixedDeltaTime);
        }
        else
        {
            cursorImage.enabled = true;
            magicCircleImage.enabled = false;
        }
        

        Vector2 nextPos = new Vector2(GameManager.instance.screenMousePos.x - 960f, GameManager.instance.screenMousePos.y - 540f);
        cursorImage.rectTransform.anchoredPosition = Vector3.MoveTowards(Vector3.zero, nextPos, Mathf.Infinity);
        
    }
}
