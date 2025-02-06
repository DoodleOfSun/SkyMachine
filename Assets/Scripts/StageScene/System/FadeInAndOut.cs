using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInAndOut : MonoBehaviour
{
    [HideInInspector] public static FadeInAndOut instance;
    private Image fadeImage;
    public bool isFadeIn;

    private Coroutine fadeCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        fadeImage = GetComponent<Image>();
        fadeImage.enabled = false;
        fadeCoroutine = null;
        isFadeIn = true;        // 씬은 시작 될 때 페이드인 하며 시작되므로 true로 한다.
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeCoroutine == null)
        {
            if (isFadeIn)
            {
                fadeCoroutine = StartCoroutine(FadeIn());
            }
            else if (!isFadeIn)
            {
                fadeCoroutine = StartCoroutine(FadeOut());
            }
        }
    }

    private IEnumerator FadeIn()
    {
        fadeImage.enabled = true;
        Color color = fadeImage.color;
        while (color.a >= 0.05f)
        {
            color.a -= 0.01f;
            fadeImage.color = color;
            yield return new WaitForFixedUpdate();
        }

        color.a = 0;
        fadeImage.color = color;
    }

    private IEnumerator FadeOut()
    {
        fadeImage.enabled = true;
        Color color = fadeImage.color;
        while (color.a <= 0.95f)
        {
            color.a += 0.01f;
            fadeImage.color = color;
            yield return new WaitForFixedUpdate();
        }

        color.a = 1;
        fadeImage.color = color;
    }

    // 파라메터가 true인 경우 FadeIn한다
    // false인 경우 FadeOut한다.
    // 또한 이 함수가 실행이 될 때만 fadeCoroutine을 null로 초기화해서 중복 실행을 방지한다.
    public void FadeFlag(bool fadeType)
    {
        isFadeIn = fadeType;
        fadeCoroutine = null;
    }
}
