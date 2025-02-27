using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeByCalling : MonoBehaviour
{
    [HideInInspector] public static CameraShakeByCalling instance;
    public Transform originPos;
    private Coroutine shakeCoroutine;
    private bool isShake;
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
        originPos.position += new Vector3(0,0, -10);
        shakeCoroutine = null;
        isShake = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (WaveManager.instance.waveCount >= 9)
        {
            if (isShake)
            {
                transform.position = Random.insideUnitSphere * 1f + originPos.position;
            }
            else
            {
                transform.position = originPos.position;
            }
        }
    }

    public void ShakeCamCalling()
    {
        if (shakeCoroutine == null)
        {
            shakeCoroutine = StartCoroutine(ShakeCam());
        }
    }

    private IEnumerator ShakeCam()
    {
        yield return new WaitForSeconds(2.5f);
        isShake = true;
        yield return new WaitForSeconds(1.5f);
        isShake = false;
        shakeCoroutine = null;
    }
}
