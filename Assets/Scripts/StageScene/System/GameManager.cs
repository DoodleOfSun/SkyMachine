using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{


    [HideInInspector] public static GameManager instance;

    [HideInInspector] public Vector3 worldMousePos;
    [HideInInspector] public Vector3 screenMousePos;
    public Text gameOverText;
    public Image Ether;

    public Image Health1;
    public Image Health2;
    public Image Health3;
    

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
    }

    void Update()
    {
        UpdateMousePos();
        UpdateHealthAndEtherUI();
    }

    private void UpdateHealthAndEtherUI()
    {
        Ether.fillAmount = Player.instance.ether;
        Debug.Log(Player.instance.health);
        if (Player.instance.health == 2)
        {
            Health3.enabled = false;
        }
        else if (Player.instance.health == 1)
        {
            Health2.enabled = false;
        }
        /*
        else if (Player.instance.health == 0)
        { 
            Health1.enabled = false;
        }
        */
    }

    private void UpdateMousePos()
    {
        screenMousePos = Input.mousePosition;
        worldMousePos = Camera.main.ScreenToWorldPoint(screenMousePos);
    }

    public void GameOver()
    {
        Health1.enabled = false;
        gameOverText.text = "Game Over !";
        enabled = false;
    }
}
