using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public enum GameManagerState
    {
        GamePlay,
        GamePaused,
        GameOver
    }

    [HideInInspector] public static GameManager instance;

    [HideInInspector] public Vector3 worldMousePos;
    [HideInInspector] public Vector3 screenMousePos;
    
    /*
    [HideInInspector] public bool wave1Activate;
    [HideInInspector] public bool wave2Activate;
    [HideInInspector] public bool wave3Activate;
    [HideInInspector] public bool wave4Activate;
    */

    public Image ether;
    public Image parryCoolTimeGauge;
    

    public GameObject playerReadyToSkillAnimation;

    public Image health1;
    public Image health2;
    public Image health3;

    public Image leftClickTutorial;
    public Text leftClickText;

    public Image rightClickTutorial;
    public Text rightClickText;

    public Image spaceBarTutorial;
    public Text spaceBarText;

    public GameObject PausedUI;
    


    public GameManagerState gmState;

    public string currentSceneName;

    private Coroutine timeStopCoroutine;

    public int playerHeartScore;
    public int killedEnemyScore;
    public int parriedBulletScore;


    // VFX 오브젝트 풀. 딕셔너리 자료구조 이용
    public List<VFXPrefab> vfxPrefabs; // 여러 VFX 프리팹 리스트
    private Dictionary<string, Queue<GameObject>> vfxPools = new Dictionary<string, Queue<GameObject>>(); // VFX 풀 딕셔너리
    public int poolSize = 10; // 기본 풀 크기

    [System.Serializable]
    public class VFXPrefab
    {
        public string name; // VFX 이름
        public GameObject prefab; // VFX 프리팹
    }



    void Awake()
    {
        AllInstantiate();
    }

    void Update()
    {
        
        currentSceneName = SceneManager.GetActiveScene().name; 
        if (currentSceneName == "ScoreScene")
        {
            return;
        }
        SystemKeyDetecting();
        ActingByState();
        ParryCoolTimeVisual();
        StartCoroutine(Tutorial());
    }

    private void AllInstantiate()
    {
        timeStopCoroutine = null;
        playerHeartScore = 3;
        killedEnemyScore = 0;
        parriedBulletScore = 0;

        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
        }

        playerReadyToSkillAnimation.SetActive(false);
        PausedUI.SetActive(false);
        SceneView.lastActiveSceneView.Focus();
        EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));
        Cursor.lockState = CursorLockMode.Confined;
        gmState = GameManagerState.GamePlay;
        parryCoolTimeGauge.enabled = false;
        parryCoolTimeGauge.fillAmount = 0;

        /*
        wave1Activate = false;
        wave2Activate = false;
        wave3Activate = false;
        */

        foreach (var vfx in vfxPrefabs)
        {
            InitializePool(vfx.name, vfx.prefab);
        }

        leftClickTutorial.enabled = false;
        leftClickText.enabled = false;

        rightClickTutorial.enabled = false;
        rightClickText.enabled = false;

        spaceBarTutorial.enabled = false;
        spaceBarText.enabled = false;

        /*
        if (currentSceneName != "Stage1")
        {
            leftClickTutorial.enabled = false;
            leftClickText.enabled = false;

            rightClickTutorial.enabled = false;
            rightClickText.enabled = false;

            spaceBarTutorial.enabled = false;
            spaceBarText.enabled = false;
        }

        else if (currentSceneName == "Stage1")
        {
            leftClickTutorial.enabled = true;
            leftClickText.enabled = true;

            rightClickTutorial.enabled = true;
            rightClickText.enabled = true;

            spaceBarTutorial.enabled = true;
            spaceBarText.enabled = true;
        }
        */
    }
    private void InitializePool(string vfxName, GameObject prefab)
    {
        if (!vfxPools.ContainsKey(vfxName))
        {
            Queue<GameObject> newPool = new Queue<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                GameObject vfx = Instantiate(prefab);
                vfx.SetActive(false);
                newPool.Enqueue(vfx);
            }

            vfxPools.Add(vfxName, newPool); // 풀 추가
        }
    }

    private void SystemKeyDetecting()
    {
        // 게임 일시정지, UI 활성화
        if (Input.GetButtonDown("Cancel") && gmState == GameManagerState.GamePlay)
        {
            UsingUIPaused();
        }

        // 게임 재진행, UI 비활성화
        else if (Input.GetButtonDown("Cancel") && gmState == GameManagerState.GamePaused)
        {
            CloseUIPaused();
        }

    }

    private bool isMoveToScoreScene = false;
    public string gameOveredSceneName = "";

    private void ActingByState()
    {
        switch (gmState)
        {
            case GameManagerState.GamePlay:
                UpdateMousePos();
                UpdateHealthAndEtherUI();
                Cursor.visible = false;
                break;
            case GameManagerState.GamePaused:
                Cursor.visible = true;
                break;
            case GameManagerState.GameOver:
                Cursor.visible = true;
                if (!isMoveToScoreScene)
                {
                    gameOveredSceneName = currentSceneName;
                    ScoreScene();
                    isMoveToScoreScene = true;
                }
                /*
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartThisScene();
                }
                */
                break;
        }
    }

    private void UsingUIPaused()
    {
        Time.timeScale = 0f;
        PausedUI.SetActive(true);
        gmState = GameManagerState.GamePaused;
    }

    private void CloseUIPaused()
    {
        Time.timeScale = 1f;
        PausedUI.SetActive(false);
        gmState = GameManagerState.GamePlay;
    }

    // 순간적으로 게임을 멈춤 ( 피격 시, 정예 적 처치 시 )
    // 외부에서 사용
    public void TimeStopLow(float time)
    {
        if (timeStopCoroutine == null)
        {
            timeStopCoroutine = StartCoroutine(TimeStopRigidity(time));
        }
    }

    private IEnumerator TimeStopRigidity(float time)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1f;
        timeStopCoroutine = null;
    }
    


    private void UpdateHealthAndEtherUI()
    {
        if (Player.instance == null)
        {
            return;
        }
        ether.fillAmount = Player.instance.ether;
        if (Player.instance.health == 2)
        {
            health3.enabled = false;
        }
        else if (Player.instance.health == 1)
        {
            health2.enabled = false;
        }
        /*
        else if (Player.instance.health == 0)
        { 
            Health1.enabled = false;
        }
        */

        if (Player.instance.isReadyToSkill1)
        {
            playerReadyToSkillAnimation.SetActive(true);
        }
        else if (!Player.instance.isReadyToSkill1)
        {
            playerReadyToSkillAnimation.SetActive(false);
        }
    }

    private void UpdateMousePos()
    {
        screenMousePos = Input.mousePosition;
        worldMousePos = Camera.main.ScreenToWorldPoint(screenMousePos);
    }

    // 
    public void GameOver()
    {
        gmState = GameManagerState.GameOver;
        health1.enabled = false;
        //enabled = false;
    }

    // 메인 화면으로 돌아가기
    public async void ReturnToMainScene()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        await Task.Delay(300);
        TitleManager.targetScene = "MainScene";
        SceneManager.LoadScene("LoadingScene");
    }

    // 해당 씬을 재시작하여 게임 재도전
    public async void RestartThisScene()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        await Task.Delay(300);
        TitleManager.targetScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("LoadingScene");
    }

    // 게임 종료, 유니티 에디터인 경우 게임 재생을 중지
    public async void ExitGame()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        await Task.Delay(300);
        // 유니티 에디터인 경우
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public IEnumerator DieVFX(string vfxName, Vector2 position)
    {
        GameObject vfx = GetVFX(vfxName);

        if (vfx != null)
        {
            vfx.transform.position = position;

            // 일정 시간 후 반환
            // 초록탄환
            if (vfxName.Contains("Green"))
            {
                yield return new WaitForSeconds(0.3f);
            }
            // 헤스터의 수류탄, 로켓
            else if (vfxName.Contains("Granade") || vfxName.Contains("Rocket"))
            {
                yield return new WaitForSeconds(0.6f);
            }
            ReturnVFX(vfxName, vfx);
        }

        yield return null;
    }

    private GameObject GetVFX(string vfxName)
    {
        // 1. vfxPools에서 조건에 맞는 키 찾기
        string foundKey = vfxPools.Keys.FirstOrDefault(key => vfxName.Contains(key));

        if (!string.IsNullOrEmpty(foundKey))
        {
            // 2. 해당 키에 매칭되는 VFX 가져오기
            if (vfxPools[foundKey].Count > 0)
            {
                GameObject vfx = vfxPools[foundKey].Dequeue();
                vfx.SetActive(true);
                return vfx;
            }
            else
            {
                // 풀이 비어있을 경우 새로 생성
                GameObject vfx = Instantiate(vfxPrefabs.Find(vfx => vfx.name.Contains(foundKey)).prefab);
                return vfx;
            }
        }

        // 3. 키를 찾지 못한 경우
        Debug.LogWarning($"VFX for {vfxName} not found!");
        return null;
    }

    private void ReturnVFX(string vfxName, GameObject vfx)
    {
        if (vfxPools.ContainsKey(vfxName))
        {
            vfx.SetActive(false);
            vfxPools[vfxName].Enqueue(vfx); // 풀에 다시 추가
        }
        else
        {
            //Debug.LogWarning($"VFX {vfxName} not found in pool!");
            Destroy(vfx); // 예외적으로 풀에 없으면 제거
        }
    }

    /*
    public void PlayNextScene()
    {
        TitleManager.targetScene = "Stage2";
        SceneManager.LoadScene("LoadingScene");
    }
    */

    public void ScoreScene()
    {
        StartCoroutine(ScoreSceneCoroutine());
    }

    private IEnumerator ScoreSceneCoroutine()
    {
        Cursor.visible = true;
        FadeInAndOut.instance.FadeFlag(false);
        yield return new WaitForSeconds(2f);
        TitleManager.targetScene = "ScoreScene";
        SceneManager.LoadScene("LoadingScene");
    }

    public void ParryCoolTimeVisual()
    {
        // TODO : ParryCoroutine이 null이 아닌 동안에는 패리 쿨타임 상태이므로 패리 게이지를 플레이어의 위치에 보여주어야 한다.
        if (Player.instance.parryCoroutine != null)
        {
            parryCoolTimeGauge.fillAmount += Time.fixedDeltaTime * 0.65f;
            parryCoolTimeGauge.enabled = true;
            parryCoolTimeGauge.rectTransform.position = Camera.main.WorldToScreenPoint(new Vector3(Player.instance.transform.position.x, 
                                                                                                   Player.instance.transform.position.y + 0.6f,
                                                                                                   0));
        }

        else
        {
            parryCoolTimeGauge.enabled = false;
            parryCoolTimeGauge.fillAmount = 0f;
        }
    }
    
    private IEnumerator Tutorial()
    {
        if (currentSceneName == "Stage1" && !CutsceneManager.instance.isCutscene && WaveManager.instance.waveCount <= 1 && Time.timeScale != 0)
        {
            if (WaveManager.instance.waveCount == 0)
            {

                leftClickTutorial.rectTransform.position = Camera.main.WorldToScreenPoint(new Vector3(Player.instance.transform.position.x - 0.6f,
                                                                                                   Player.instance.transform.position.y + 1f,
                                                                                                   0));
                rightClickTutorial.rectTransform.position = Camera.main.WorldToScreenPoint(new Vector3(Player.instance.transform.position.x + 0.6f,
                                                                                                   Player.instance.transform.position.y + 1f,
                                                                                                   0));
                leftClickText.text = "ATTACK";

                leftClickTutorial.enabled = true;
                leftClickText.enabled = true;
                rightClickTutorial.enabled = true;
                rightClickText.enabled = true;
                spaceBarTutorial.enabled = false;
                spaceBarText.enabled = false;

                if (Player.instance.isParryAiming)
                {

                    leftClickTutorial.rectTransform.position = Camera.main.WorldToScreenPoint(new Vector3(Player.instance.transform.position.x,
                                                                                                   Player.instance.transform.position.y + 1f,
                                                                                                   0));
                    leftClickText.text = "PARRY";

                    leftClickTutorial.enabled = true;
                    leftClickText.enabled = true;
                    rightClickTutorial.enabled = false;
                    rightClickText.enabled = false;
                    spaceBarTutorial.enabled = false;
                    spaceBarText.enabled = false;
                }

            }
            else if (WaveManager.instance.waveCount == 1)
            {
                spaceBarTutorial.rectTransform.position = Camera.main.WorldToScreenPoint(new Vector3(Player.instance.transform.position.x,
                                                                                                   Player.instance.transform.position.y + 1f,
                                                                                                   0));
                leftClickTutorial.enabled = false;
                leftClickText.enabled = false;
                rightClickTutorial.enabled = false;
                rightClickText.enabled = false;
                spaceBarTutorial.enabled = true;
                spaceBarText.enabled = true;
            }
        }
        else
        {
            leftClickTutorial.enabled = false;
            leftClickText.enabled = false;

            rightClickTutorial.enabled = false;
            rightClickText.enabled = false;

            spaceBarTutorial.enabled = false;
            spaceBarText.enabled = false;
        }

        yield return null;
    }


    public void SmallTimeStop()
    {
        StartCoroutine(SmallTimeStopCoroutine());
    }

    private IEnumerator SmallTimeStopCoroutine()
    {
        if (Time.timeScale == 1f)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(0.05f);
            Time.timeScale = 1f;
        }
        else
        {
            yield return null;
        }
    }
}