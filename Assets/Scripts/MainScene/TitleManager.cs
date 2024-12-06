using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public static string targetScene; // 로드할 씬 이름 저장

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadingNextScene()
    {
        targetScene = "Stage1";
        SceneManager.LoadScene("LoadingScene");
    }

    public void LoadingSampleScene()
    {
        targetScene = "SampleScene";
        SceneManager.LoadScene("LoadingScene");
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}