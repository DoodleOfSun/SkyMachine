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
    [HideInInspector] public Transform lockedTarget;
    public Text gameOverText;

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
        lockedTarget = RaycastingCameraToWorld();
    }

    private Transform RaycastingCameraToWorld()
    {
        screenMousePos = Input.mousePosition;
        worldMousePos = Camera.main.ScreenToWorldPoint(screenMousePos);

        RaycastHit2D[] hit = Physics2D.RaycastAll(worldMousePos, Vector2.zero);

        // Tag가 Enemy일 경우 록 온
        foreach (RaycastHit2D enemyHit in hit)
        {
            if (enemyHit.collider != null && enemyHit.collider.tag == "Enemy")
            {
                return enemyHit.transform;
            }
        }
        return null;
    }

    public void GameOver()
    {
        gameOverText.text = "Game Over !";
        enabled = false;
    }
}
