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

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;
    public GameObject waveRoom;
    public GameObject arrow;    // TODO : 맵 클리어 시 출구 화살표
    
    // NOTE : 어떤 방에서의 위치정보, 각 웨이브의 몬스터와 그 출구가 어디인지 (string)을 저장하는 List
    public List<WaveExit> waveInfo = new List<WaveExit>();

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
            Debug.Log("웨이브 카운트 : " + waveCount);
            Debug.Log("방 1 클리어");
        }
    }
    
    // TODO : 현재 스테이지의 출구 string에 따라 화살표를 표시한다
    private void ExitArrow()
    {

    }

    // TODO : WaveRoom에는 4개의 벽이 동서남북으로 둘러쳐져 있다. 원하는 벽에 충돌했을 때 다음 방으로 넘기게 하려면 어떻게 해야 할까?
    // 또한 WaveRoom은 하위 자식 오브젝트를 가진 부모 오브젝트인데, 이 자체로는 Transform만 가지고 있다.
    // 또 각 Room마다 어느 부분에 화살표를 표시를 하고, 어디 부분으로 이동을 할지 (왼쪽, 오른쪽? 위 아래?)를 같이 넘겨주어야 한다. string이면 충분할 것 같은데..

    // 이 함수는 Wall.cs에서 호출된다. isCleared를 검사하고, true인 경우 다음을 수행한다.
    /*
     * 0. waveCount를 1 증가시키고, isCleared를 false로 바꾼다.
     * 1. 플레이어를 일정한 x 혹은 y좌표만큼 순간이동시킨다. (벽에 끼이지 않을 정도면 충분하다. 방을 이동하는 것이기에..)
     * 이 경우 어디로 이동시킬지는 string 파라메터를 받아서 조건문으로 정한다.
     * 2. waveRoom을 waveExit의 다음 roomPos로 이동시킨다.
     * 3. 각 웨이브의 wave?Activate를 true로 바꾼다. (몬스터들이 활동을 시작한다)
     * 4. 카메라를 움직여주어여 하는데, 부드럽게 움직여주어야 한다.
     */

    public void MovingNextRoom(string dirStr)
    {
        if (dirStr.Contains(waveInfo[waveCount].exit))
        {
            // 플레이어와 waveRoom을 이동시킨다.
            if (waveInfo[waveCount].exit == "Left")
            {
                Player.instance.transform.position = new Vector3(Player.instance.transform.position.x - 1.1f, 
                                                                 Player.instance.transform.position.y,
                                                                 Player.instance.transform.position.z);
                if (waveCount + 1 < waveInfo.Count && waveInfo[waveCount + 1] != null)
                {
                    waveRoom.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                }
            }
            else if (waveInfo[waveCount].exit == "Right")
            {
                Player.instance.transform.position = new Vector3(Player.instance.transform.position.x + 1.1f,
                                                                 Player.instance.transform.position.y,
                                                                 Player.instance.transform.position.z);
                if (waveCount + 1 < waveInfo.Count && waveInfo[waveCount + 1] != null)
                {
                    waveRoom.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                }
            }
            else if (waveInfo[waveCount].exit == "Up")
            {
                Player.instance.transform.position = new Vector3(Player.instance.transform.position.x,
                                                                 Player.instance.transform.position.y + 1.1f,
                                                                 Player.instance.transform.position.z);
                if (waveCount + 1 < waveInfo.Count && waveInfo[waveCount + 1] != null)
                {
                    waveRoom.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                }
            }
            else if (waveInfo[waveCount].exit == "Down")
            {
                Player.instance.transform.position = new Vector3(Player.instance.transform.position.x,
                                                                 Player.instance.transform.position.y - 1.1f,
                                                                 Player.instance.transform.position.z);
                if (waveCount + 1 < waveInfo.Count && waveInfo[waveCount + 1] != null)
                {
                    waveRoom.transform.position = waveInfo[waveCount + 1].roomPos.transform.position;
                }
            }
            waveCount++;
            waveInfo[waveCount].waveActivate = true;
            isCleared = false;
        }
    }
}