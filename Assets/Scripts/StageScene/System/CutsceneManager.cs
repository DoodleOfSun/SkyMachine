using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager instance;

    [HideInInspector] public bool isCutscene;

    public Text text;
    public float typingSpeed;  // 텍스트 출력 속도

    public GameObject dialogue;      // 대화창 프리팹

    //public RectTransform BlackBarUp;
    //public RectTransform BlackBarDown;

    public string dialogueType;

    private int dialogueInt;
    private Coroutine dialogueCoroutine;
    private Coroutine typingCoroutine;
    private bool isTyped;       // 현재 글자가 타이핑이 되고 있는지를 검사. false인 동안 입력을 하고, 입력이 종료되었을 때만 true가 된다.

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        isCutscene = true;
        isTyped = false;

        dialogueCoroutine = null;
        typingCoroutine = null;
        dialogue.SetActive(false);

        // 게임 시작 시 대사 갯수
        // 이후 이 카운트는 유동적으로 관리된다.
        dialogueInt = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogueCoroutine == null && isCutscene)
            dialogueCoroutine = StartCoroutine(Cutscene(dialogueInt));
    }

    private IEnumerator Cutscene(int dialogueData)
    {
        int elapsedCutscene = 0;

        while (elapsedCutscene < dialogueData)
        {

            DisplayDialogueByType(elapsedCutscene);

            // 좌클릭이 입력되고, 타입이 종료되었을 때를 검사
            if (Input.GetMouseButtonDown(0) && isTyped)
            {
                isTyped = false;
                elapsedCutscene++;
            }

            yield return null; // 한 프레임 대기
        }

        dialogue.SetActive(false);
        isCutscene = false;
        dialogueCoroutine = null; // 코루틴 종료 상태 설정
    }

    private void DisplayDialogueByType(int data)
    {
        RectTransform rt = dialogue.GetComponent<RectTransform>();
        if (dialogueType == "Stage1" && typingCoroutine == null)
        {
            dialogue.SetActive(true);
            if (data == 0 && !isTyped)
            {
                rt.anchoredPosition = new Vector2(-500, 200);
                typingCoroutine = StartCoroutine(TypeText("When did all these drones show up?"));

            }
            else if (data == 1 && !isTyped)
            {
                rt.anchoredPosition = new Vector2(400, 150);
                typingCoroutine = StartCoroutine(TypeText("Access denied. Return home immediately."));
            }
            else if (data == 2 && !isTyped)
            {
                rt.anchoredPosition = new Vector2(-500, 200);
                typingCoroutine = StartCoroutine(TypeText("No way. Bring it on!"));
            }
        }
    }

    // 텍스트를 한 자 한 자 뜨게 만든다
    private IEnumerator TypeText(string fullText)
    {
        text.text = "";  // 시작할 때 텍스트를 초기화

        foreach (char letter in fullText.ToCharArray())
        {
            text.text += letter;  // 한 글자씩 추가
            yield return new WaitForSeconds(typingSpeed);  // 글자 간 시간 간격
        }

        isTyped = true;
        typingCoroutine = null;
    }

}
