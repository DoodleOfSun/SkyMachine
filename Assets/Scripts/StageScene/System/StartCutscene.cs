using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class StartCutscene : MonoBehaviour
{
    public enum CutState
    {
        Cut1,
        Cut2,
        Cut3,
    }

    private CutState cutState;
    public Animator animator;
    private Coroutine cutCorotine;
    public Text text;
    public GameObject shipImage;

    // Start is called before the first frame update
    void Start()
    {
        cutState = CutState.Cut1;
        cutCorotine = null;
        shipImage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        switch (cutState)
        {
            case CutState.Cut1:
                if (cutCorotine == null)
                {
                    animator.SetTrigger("Cutscene1");
                    cutCorotine = StartCoroutine(Cutscene());
                }
                if (text.enabled == true && Input.GetMouseButton(0))
                {
                    cutState = CutState.Cut2;
                    text.enabled = false;
                    cutCorotine = null;
                }
                break;
            case CutState.Cut2:
                if (cutCorotine == null)
                {
                    animator.SetTrigger("Cutscene2");
                    cutCorotine = StartCoroutine(Cutscene());
                }
                if (text.enabled == true && Input.GetMouseButton(0))
                {
                    cutState = CutState.Cut3;
                    text.enabled = false;
                    cutCorotine = null;
                }
                break;
            case CutState.Cut3:
                if (cutCorotine == null)
                {
                    shipImage.SetActive(true);
                    cutCorotine = StartCoroutine(Cutscene());
                }
                if (text.enabled == true && Input.GetMouseButton(0))
                {
                    cutCorotine = StartCoroutine(GameStart());
                }
                shipImage.transform.position = Vector3.MoveTowards(shipImage.transform.position, Vector3.zero, 0.5f);
                break;
        }
    }

    private IEnumerator GameStart()
    {
        FadeInAndOut.instance.FadeFlag(false);
        yield return new WaitForSeconds(3f);
        TitleManager.targetScene = "Stage1";
        SceneManager.LoadScene("LoadingScene");
    }

    private IEnumerator Cutscene()
    {
        text.enabled = false;
        yield return new WaitForSeconds(3f);
        text.enabled = true;
    }
}
