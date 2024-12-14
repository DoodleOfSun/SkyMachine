using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

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
        // 유니티 에디터인 경우
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}