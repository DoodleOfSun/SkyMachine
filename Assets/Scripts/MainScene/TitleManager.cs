using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class TitleManager : MonoBehaviour
{
    public static string targetScene; // 로드할 씬 이름 저장
    public GameObject PausedUI;

    // Start is called before the first frame update
    void Start()
    {
        PausedUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (PausedUI.activeSelf && Input.GetButtonDown("Cancel"))
        {
            CloseOption();
        }
    }

    private async void LoadingNextScene()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        await Task.Delay(300);
        StartCoroutine(LoadingNextSceneCoroutine());
    }

    private IEnumerator LoadingNextSceneCoroutine()
    {
        FadeInAndOut.instance.FadeFlag(false);
        yield return new WaitForSeconds(2f);
        targetScene = "StartCutscene";
        SceneManager.LoadScene("LoadingScene");
    }

    public void LoadingSampleScene()
    {
        targetScene = "SampleScene";
        SceneManager.LoadScene("LoadingScene");
    }

    public async void OpenOption()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        await Task.Delay(300);
        PausedUI.SetActive(true);
    }

    private async void CloseOption()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        await Task.Delay(300);
        PausedUI.SetActive(false);
    }

    private async void ExitGame()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        await Task.Delay(300);
        // 유니티 에디터인 경우
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}