using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [HideInInspector] public CameraShake instance;
    private Vector3 originPos;
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

        originPos = transform.position;
        shakeCoroutine = null;
        isShake = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (shakeCoroutine == null)
        {
            shakeCoroutine = StartCoroutine(ShakeCam());
        }

        if (isShake)
        {
            transform.position = Random.insideUnitSphere * 1f + originPos;
        }
        else
        {
            transform.position = originPos;
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
