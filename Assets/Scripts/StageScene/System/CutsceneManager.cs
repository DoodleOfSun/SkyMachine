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
    public int bossWave;            // 보스 웨이브의 번호
    public GameObject talkingEnemy;

    private Vector3 currentBlackBarUp;
    private Vector3 currentBlackBarDown;

    public string dialogueType;     // 대화의 타입. 없으면 None으로 설정할 것

    private int dialogueInt;        // 대화 창의 개수 (대사가 3개면 3개)
    private Coroutine dialogueCoroutine;
    private Coroutine typingCoroutine;
    private Coroutine letterboxCoroutine;
    private bool isTyped;       // 현재 글자가 타이핑이 되고 있는지를 검사. false인 동안 입력을 하고, 입력이 종료되었을 때만 true가 된다.
    private bool isBossDialogue;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    private void Init()
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
        isBossDialogue = false;

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

        if (WaveManager.instance.waveCount != bossWave && dialogueCoroutine == null && isCutscene)
        {
            dialogueCoroutine = StartCoroutine(Cutscene(dialogueInt));
        }

        
        // 보스 전 도달시 컷씬 진행
        if (WaveManager.instance.waveCount == bossWave && dialogueCoroutine == null && !isBossDialogue)
        {
            isBossDialogue = true;
            dialogueInt = 4;
            dialogueCoroutine = StartCoroutine(Cutscene(dialogueInt));
        }

        CheckingWaveType();
    }

    private void CheckingWaveType()
    {
        if (GameManager.instance.currentSceneName == "Stage1")
        {
            Hester.instance.CheckingWaveType();
        }
        else if (GameManager.instance.currentSceneName == "Stage2")
        {
            Prey.instance.CheckingWaveType();
            PreyMachine.instance.CheckingWaveType();
        }
    }

    private IEnumerator Cutscene(int dialogueData)
    {
        int elapsedCutscene = 0;
        isCutscene = true;

        // 컷신 시작, 레터박스 하강
        if (letterboxCoroutine == null)
        {
            letterboxCoroutine = StartCoroutine(LetterBox());
        }

        while (elapsedCutscene < dialogueData)
        {
            DisplayDialogueByType(elapsedCutscene);
            // 좌클릭이 입력되고, 타입이 종료되었을 때를 검사
            if (Input.GetMouseButtonDown(0) && isTyped)
            {
                isTyped = false;
                elapsedCutscene++;
            }
            else if (Input.GetMouseButtonDown(1) && isTyped && letterboxCoroutine == null)
            {
                isTyped = false;
                break;
            }

            yield return null;
        }

        // 컷신 종료, 레터박스 상승
        if (letterboxCoroutine == null)
        {
            letterboxCoroutine = StartCoroutine(LetterBoxReturn());
        }


        dialogue.SetActive(false);
        RectTransform rt = dialogue.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        isCutscene = false;
        dialogueCoroutine = null;
    }

    private void DisplayDialogueByType(int data)
    {
        RectTransform rt = dialogue.GetComponent<RectTransform>();
        
        // 1스테이지 시작 시 대사
        if (dialogueType == "Stage1" && typingCoroutine == null && WaveManager.instance.waveCount == 0)
        {
            dialogue.SetActive(true);
            if (data == 0 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Player.instance.transform.position);
                //typingCoroutine = StartCoroutine(TypeText("When did all these drones show up?"));
                typingCoroutine = StartCoroutine(TypeText("하늘에 드론이 이렇게 깔리다니."));
            }
            else if (data == 1 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(talkingEnemy.transform.position);

                //typingCoroutine = StartCoroutine(TypeText("Access denied. Return home immediately."));
                typingCoroutine = StartCoroutine(TypeText("접근 금지. 돌아가십시오."));
            }
            else if (data == 2 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Player.instance.transform.position);
                //typingCoroutine = StartCoroutine(TypeText("No way. Bring it on! "));
                typingCoroutine = StartCoroutine(TypeText("그렇겐 안 되지. 덤벼!"));
            }
        }

        // 1스테이지 보스전 시작 시 대사
        if (dialogueType == "Stage1" && typingCoroutine == null && WaveManager.instance.waveCount == CutsceneManager.instance.bossWave)
        {
            dialogue.SetActive(true);

            if (data == 0 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Player.instance.transform.position);
                Debug.Log(rt.anchoredPosition);
                typingCoroutine = StartCoroutine(TypeText("이건 뭐야? 일조권 침해라고."));
            }
            else if (data == 1 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Hester.instance.transform.position);

                typingCoroutine = StartCoroutine(TypeText("배야. 넌 상상도 못할 정도로 큰!"));
            }
            else if (data == 2 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Player.instance.transform.position);
                typingCoroutine = StartCoroutine(TypeText("날지 못한다면, 저기에 타 있지 그래?"));
            }
            else if (data == 3 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Hester.instance.transform.position);
                typingCoroutine = StartCoroutine(TypeText("..까불고 있네, 터트려주지!"));
            }
        }

        // 2스테이지 시작 시 대사
        if (dialogueType == "Stage2" && typingCoroutine == null && WaveManager.instance.waveCount == 0)
        {
            dialogue.SetActive(true);
            if (data == 0 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Player.instance.transform.position);
                //typingCoroutine = StartCoroutine(TypeText("When did all these drones show up?"));
                typingCoroutine = StartCoroutine(TypeText("여기구나."));
            }
            else if (data == 1 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Player.instance.transform.position);

                //typingCoroutine = StartCoroutine(TypeText("Access denied. Return home immediately."));
                typingCoroutine = StartCoroutine(TypeText("톱니바퀴 돌아가는 소리가 시끄럽네."));
            }
            else if (data == 2 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Player.instance.transform.position);
                //typingCoroutine = StartCoroutine(TypeText("No way. Bring it on! "));
                typingCoroutine = StartCoroutine(TypeText("빠르게 끝내주지!"));
            }
        }

        // 2스테이지 보스전 시작 시 대사
        
        if (dialogueType == "Stage2" && typingCoroutine == null && WaveManager.instance.waveCount == CutsceneManager.instance.bossWave)
        {
            dialogue.SetActive(true);

            if (data == 0 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Player.instance.transform.position);
                Debug.Log(rt.anchoredPosition);
                typingCoroutine = StartCoroutine(TypeText("찾았다. 이제 부수기만 하면 되겠어."));
            }
            else if (data == 1 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Prey.instance.transform.position);

                typingCoroutine = StartCoroutine(TypeText("*콜록* 부..부수게 두지는 - "));
            }
            else if (data == 2 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Player.instance.transform.position);
                typingCoroutine = StartCoroutine(TypeText("아, 숙녀분. 거기 위험하니까 비켜줄래?"));
            }
            else if (data == 3 && !isTyped)
            {
                rt.anchoredPosition = AdjustScreenPos(Prey.instance.transform.position);
                typingCoroutine = StartCoroutine(TypeText("부수게 두지는 않겠어."));
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
        fullText = fullText + " ";
        text.text = "";  // 시작할 때 텍스트를 초기화

        foreach (char letter in fullText.ToCharArray())
        {
            text.text += letter;  // 한 글자씩 추가
            yield return new WaitForSeconds(typingSpeed);  // 글자 간 시간 간격
        }

        isTyped = true;
        typingCoroutine = null;
    }

    private Vector3 AdjustScreenPos(Vector3 pos)
    {
        Vector2 targetPos = Camera.main.WorldToScreenPoint(new Vector3(pos.x,
                                                                   pos.y + 1f,
                                                                   pos.z
                                                                   ));
        // 화면 중심 좌표 계산
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        // 화면 중심 기준으로 보정
        Vector2 adjustedPos = targetPos - screenCenter;
        return adjustedPos;
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