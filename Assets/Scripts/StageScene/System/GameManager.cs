using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{


    [HideInInspector] public static GameManager instance;

    [HideInInspector] public Vector3 worldMousePos;
    [HideInInspector] public Vector3 screenMousePos;
    public Text gameOverText;
    public Image ether;
    public GameObject playerReadyToSkillAnimation;

    public Image health1;
    public Image health2;
    public Image health3;
    

    void Awake()
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
        SceneView.lastActiveSceneView.Focus();
        EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));
    }

    void Update()
    {
        UpdateMousePos();
        UpdateHealthAndEtherUI();
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
        health1.enabled = false;
        gameOverText.text = "Game Over !";
        enabled = false;
    }
}
