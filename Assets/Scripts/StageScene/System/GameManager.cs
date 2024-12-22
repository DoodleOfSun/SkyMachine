using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private enum GameManagerState
    {
        GamePlay,
        GamePaused,
        GameOver
    }

    [HideInInspector] public static GameManager instance;

    [HideInInspector] public Vector3 worldMousePos;
    [HideInInspector] public Vector3 screenMousePos;
    public Text gameOverText;
    public Image ether;
    public GameObject playerReadyToSkillAnimation;

    public Image health1;
    public Image health2;
    public Image health3;

    public GameObject PausedUI;

    private GameManagerState gmState;



    void Awake()
    {
        AllInstantiate();
    }

    void Update()
    {
        SystemKeyDetecting();
        ActingByState();
    }

    private void AllInstantiate()
    {
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


    private void UpdateHealthAndEtherUI()
    {
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
        else
        {
            playerReadyToSkillAnimation.SetActive(false);
        }
    }

    private void UpdateMousePos()
    {
        screenMousePos = Input.mousePosition;
        worldMousePos = Camera.main.ScreenToWorldPoint(screenMousePos);
    }

    public void GameOver()
    {
        gmState = GameManagerState.GameOver;
        health1.enabled = false;
        gameOverText.text = "Game Over !";
        //enabled = false;
    }

    public void ReturnToMainScene()
    {
        TitleManager.targetScene = "MainScene";
        SceneManager.LoadScene("LoadingScene");
    }

    public void RestartThisScene()
    {
        TitleManager.targetScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("LoadingScene");
    }

}
