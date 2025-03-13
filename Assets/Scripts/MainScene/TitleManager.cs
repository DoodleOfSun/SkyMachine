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

    private void LoadingNextScene()
    {
        StartCoroutine(LoadingNextSceneCoroutine());
    }

    private IEnumerator LoadingNextSceneCoroutine()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        yield return new WaitForSeconds(0.3f);
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
    
    public void OpenOption()
    {
        StartCoroutine(OpenOptionCoroutine());
    }

    private IEnumerator OpenOptionCoroutine()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        yield return new WaitForSeconds(0.3f);
        PausedUI.SetActive(true);
    }

    private void CloseOption()
    {
        StartCoroutine(CloseOptionCoroutine());
    }

    private IEnumerator CloseOptionCoroutine()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        yield return new WaitForSeconds(0.3f);
        PausedUI.SetActive(false);
    }
    
    private void ExitGame()
    {
        StartCoroutine(ExitGameCoroutine());
    }

    private IEnumerator ExitGameCoroutine()
    {
        AudioManager.instance.PlayingSFX("UIClick");
        yield return new WaitForSeconds(0.3f);
        Application.Quit();
    }
}