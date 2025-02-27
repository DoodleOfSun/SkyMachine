using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreRecordManager : MonoBehaviour
{
    public static ScoreRecordManager instance;

    [HideInInspector] public string gameStateStr;
    [HideInInspector] public string gameOveredSceneName;

    [HideInInspector] public int parriedBulletScore;
    [HideInInspector] public int killedEnemyScore;
    [HideInInspector] public int playerHeartScore;


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
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

        if (GameManager.instance != null)
        {
            gameStateStr = GameManager.instance.gmState.ToString();
            gameOveredSceneName = GameManager.instance.gameOveredSceneName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        RestoringScore();
    }

    private void RestoringScore()
    {
        gameStateStr = GameManager.instance.gmState.ToString();
        gameOveredSceneName = GameManager.instance.gameOveredSceneName;
        parriedBulletScore = GameManager.instance.parriedBulletScore;
        killedEnemyScore = GameManager.instance.killedEnemyScore;
        playerHeartScore = GameManager.instance.playerHeartScore;
    }
}