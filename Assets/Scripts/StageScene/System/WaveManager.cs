using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEditor;
using UnityEngine;


[Serializable]
public class WaveExit
{
    public string exit;
    public GameObject roomPos;
    public GameObject wavePool;
    public bool waveActivate;
}

[Serializable]
public class Arrows
{
    public GameObject parents;
    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;
    public GameObject waveRoom;
    public GameObject backGroundPrefabs;
    private GameObject backGround;
    
    // NOTE : 어떤 방에서의 위치정보, 각 웨이브의 몬스터와 그 출구가 어디인지 (string)을 저장하는 List
    public List<WaveExit> waveInfo = new List<WaveExit>();

    public Arrows arrows = new Arrows();

    [HideInInspector] public int waveCount;   // 현재 웨이브 숫자
    [HideInInspector] public bool isCleared;    // 방 클리어 여부    

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
        waveCount = 0;  
        isCleared = false;
        deactivateAllArrows();
        backGround = Instantiate(backGroundPrefabs);
        backGround.transform.position = new Vector3(6.5f,0,0);
        backGround.SetActive(true);
    }

    void Update()
    {
        InitiateWave();
        CheckingWaveClear();
    }
    
    private void InitiateWave()
    {
        if (!CutsceneManager.instance.isCutscene &&
            !waveInfo[waveCount].waveActivate &&
            waveInfo[waveCount].wavePool.transform.childCount != 0)
        {
            waveInfo[waveCount].waveActivate = true;
            isCleared = false;
        }
    }

    private void CheckingWaveClear()
    {
        if (waveInfo[waveCount].waveActivate && waveInfo[waveCount].wavePool.transform.childCount == 0 && isCleared == false)
        {

            isCleared = true;
            ExitArrow(waveInfo[waveCount].exit, waveInfo[waveCount].roomPos.transform.position);
            Debug.Log("웨이브 카운트 : " + waveCount);
            Debug.Log("방 1 클리어");
        }
    }
    
    // TODO : 현재 스테이지의 출구 string에 따라 화살표를 표시한다
    private void ExitArrow(string exit, Vector3 targetPos)
    {
        Debug.Log(exit);
        arrows.parents.transform.position = targetPos;
        arrows.parents.SetActive(true);
        if (exit == "Left")
        {
            StartCoroutine(arrowBlink(arrows.left));
        }
        else if (exit == "Right")
        {
            StartCoroutine(arrowBlink(arrows.right));
        }
        else if (exit == "Up")
        {
            StartCoroutine(arrowBlink(arrows.up));
        }
        else if (exit == "Down")
        {
            StartCoroutine(arrowBlink(arrows.down));
        }
        else if (exit == "None")
        {

        }
    }

    private IEnumerator arrowBlink(GameObject arrow)
    {
        while (arrows.parents.activeSelf)
        {
            arrow.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            arrow.SetActive(false); 
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void deactivateAllArrows()
    {
        arrows.parents.SetActive(false);
        arrows.up.SetActive(false);
        arrows.down.SetActive(false);
        arrows.left.SetActive(false);
        arrows.right.SetActive(false);
    }

    // NOTE : 이 함수는 스테이지를 다음 방으로 옮긴다.
    // 방을 옮기기 전에, 모든 화살표를 비활성화하고 플레이어를 옮긴다.
    // 그 다음 모든 방 객체를 옮기고 waveInfo를 새 스테이지의 값으로 초기화한다.

    public void MovingNextRoom(string dirStr)
    {
        deactivateAllArrows();
        if (dirStr.Contains(waveInfo[waveCount].exit))
        {
            // 플레이어와 waveRoom을 이동시킨다.
            // 보스방인 경우
            if (waveInfo.Count - 2 == waveCount)
            {
                Debug.Log("보스방 입장");
                CutsceneManager.instance.isCutscene = true;
                Player.instance.transform.position = new Vector3(waveInfo[waveCount + 1].roomPos.transform.position.x - 3f,
                                                                waveInfo[waveCount + 1].roomPos.transform.position.y,
                                                                waveInfo[waveCount + 1].roomPos.transform.position.z);
                if (waveCount + 1 < waveInfo.Count && waveInfo[waveCount + 1] != null)
                {
                    backGround.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                    waveRoom.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                }
            }

            else if (waveInfo[waveCount].exit == "Left")
            {
                Player.instance.transform.position = new Vector3(Player.instance.transform.position.x, 
                                                                 Player.instance.transform.position.y,
                                                                 Player.instance.transform.position.z);
                if (waveCount + 1 < waveInfo.Count && waveInfo[waveCount + 1] != null)
                {
                    backGround.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                    waveRoom.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                }
            }
            else if (waveInfo[waveCount].exit == "Right")
            {
                Player.instance.transform.position = new Vector3(Player.instance.transform.position.x,
                                                                 Player.instance.transform.position.y,
                                                                 Player.instance.transform.position.z);
                if (waveCount + 1 < waveInfo.Count && waveInfo[waveCount + 1] != null)
                {
                    backGround.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                    waveRoom.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                }
            }
            else if (waveInfo[waveCount].exit == "Up")
            {
                Player.instance.transform.position = new Vector3(Player.instance.transform.position.x,
                                                                 Player.instance.transform.position.y,
                                                                 Player.instance.transform.position.z);
                if (waveCount + 1 < waveInfo.Count && waveInfo[waveCount + 1] != null)
                {
                    backGround.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                    waveRoom.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                }
            }
            else if (waveInfo[waveCount].exit == "Down")
            {
                Player.instance.transform.position = new Vector3(Player.instance.transform.position.x,
                                                                 Player.instance.transform.position.y,
                                                                 Player.instance.transform.position.z);
                if (waveCount + 1 < waveInfo.Count && waveInfo[waveCount + 1] != null)
                {
                    backGround.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                    waveRoom.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                }
            }
            waveCount++;
            CamFollowByTransform.instance.MoveByTransform(waveInfo[waveCount].roomPos.transform.position);
            waveInfo[waveCount].waveActivate = true;
            isCleared = false;
        }
    }
}