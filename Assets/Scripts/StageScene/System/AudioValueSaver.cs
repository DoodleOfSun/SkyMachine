using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioValueSaver : MonoBehaviour
{
    public static AudioValueSaver instance;

    public float bgmValue;
    public float sfxValue;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
