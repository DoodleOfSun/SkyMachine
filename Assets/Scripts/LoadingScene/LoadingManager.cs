using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TransitionNextScene(TitleManager.targetScene));
    }

    IEnumerator TransitionNextScene(string sceneName)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);

        ao.allowSceneActivation = false;

        while (!ao.isDone)
        {
            if (ao.progress >= 0.9f)
            {
                ao.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
