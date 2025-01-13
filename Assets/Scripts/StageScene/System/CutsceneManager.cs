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

    public GameObject BlackBarUp;
    public GameObject BlackBarDown;
    private Vector3 currentBlackBarUp;
    private Vector3 currentBlackBarDown;

    public string dialogueType;     // 대화의 타입. 없으면 None으로 설정할 것

    private int dialogueInt;
    private Coroutine dialogueCoroutine;
    private Coroutine typingCoroutine;
    private Coroutine letterboxCoroutine;
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
        letterboxCoroutine = null;
        dialogue.SetActive(false);
        BlackBarUp.SetActive(false);
        BlackBarDown.SetActive(false);
        currentBlackBarUp = BlackBarUp.transform.position;
        currentBlackBarDown = BlackBarDown.transform.position;
        // 게임 시작 시 대사 갯수
        // 이후 이 카운트는 유동적으로 관리된다.
        dialogueInt = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogueType == "None")
        {
            isCutscene = false;
            return;
        }
        if (letterboxCoroutine == null && isCutscene)
        {
            letterboxCoroutine = StartCoroutine(LetterBox());
        }
        if (dialogueCoroutine == null && isCutscene)
        {
            dialogueCoroutine = StartCoroutine(Cutscene(dialogueInt));
        }
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
            else if (Input.GetMouseButtonDown(1) && isTyped)
            {
                isTyped = false;
                break;
            }

            yield return null;
        }

        // 컷신 종료
        if (letterboxCoroutine == null)
        {
            letterboxCoroutine = StartCoroutine(LetterBoxReturn());
        }
        dialogue.SetActive(false);
        isCutscene = false;
        dialogueCoroutine = null;
    }

    // HACK : Player의 포지션의 초기화 순서보다 CutSceneManager가 더 빨리 실행되는 문제 때문에 대화창의 위치가 하드코딩되어 있다.
    private void DisplayDialogueByType(int data)
    {
        RectTransform rt = dialogue.GetComponent<RectTransform>();
        if (dialogueType == "Stage1" && typingCoroutine == null)
        {
            dialogue.SetActive(true);
            if (data == 0 && !isTyped)
            {
                rt.anchoredPosition = new Vector2(-450, 200);
                typingCoroutine = StartCoroutine(TypeText("When did all these drones show up?"));

            }
            else if (data == 1 && !isTyped)
            {
                rt.anchoredPosition = new Vector2(400, 150);
                typingCoroutine = StartCoroutine(TypeText("Access denied. Return home immediately."));
            }
            else if (data == 2 && !isTyped)
            {
                rt.anchoredPosition = new Vector2(-450, 200);
                typingCoroutine = StartCoroutine(TypeText("No way. Bring it on! "));
            }
        }
        else
        {
            return;
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

    // 레터박스 효과
    private IEnumerator LetterBox()
    {
        Vector2 targetPosUp = new Vector2(960, 1120);
        Vector2 targetPosDown = new Vector2(960, -40);
        BlackBarUp.SetActive(true);
        BlackBarDown.SetActive(true);

        while (Vector3.Distance(BlackBarUp.transform.position, targetPosUp) > 0.1f &&
               Vector3.Distance(BlackBarDown.transform.position, targetPosDown) > 0.1f) // 목표에 도달할 때까지
        {
            BlackBarUp.transform.position = Vector2.MoveTowards(
            BlackBarUp.transform.position, targetPosUp, 300f * Time.deltaTime);
            // 아래쪽 바 이동
            BlackBarDown.transform.position = Vector2.MoveTowards(
                BlackBarDown.transform.position, targetPosDown, 300f * Time.deltaTime);
            yield return null;
        }
        letterboxCoroutine = null;
    }

    private IEnumerator LetterBoxReturn()
    {
        
        Vector2 targetPosUp = currentBlackBarUp;
        Vector2 targetPosDown = currentBlackBarDown;

        while (Vector3.Distance(BlackBarUp.transform.position, targetPosUp) > 0.1f &&
               Vector3.Distance(BlackBarDown.transform.position, targetPosDown) > 0.1f) // 목표에 도달할 때까지
        {
            BlackBarUp.transform.position = Vector2.MoveTowards(
            BlackBarUp.transform.position, targetPosUp, 300f * Time.deltaTime);
            // 아래쪽 바 이동
            BlackBarDown.transform.position = Vector2.MoveTowards(
                BlackBarDown.transform.position, targetPosDown, 300f * Time.deltaTime);
            yield return null;
        }

        ResetLetterBox();
        letterboxCoroutine = null;

    }

    private void ResetLetterBox()
    {
        BlackBarUp.transform.position = currentBlackBarUp;
        BlackBarDown.transform.position = currentBlackBarDown;
        BlackBarUp.SetActive(false);
        BlackBarDown.SetActive(false);
        letterboxCoroutine = null;
    }
}
