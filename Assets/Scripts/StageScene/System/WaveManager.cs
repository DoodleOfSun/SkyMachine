using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Player.instance.transform.position.x >= this.transform.position.x && !GameManager.instance.wave1Activate && this.transform.name == "Wave1Trigger")
        {
            GameManager.instance.wave1Activate = true;
            enabled = false;    
        }
        else if (Player.instance.transform.position.x >= this.transform.position.x && !GameManager.instance.wave2Activate && this.transform.name == "Wave2Trigger")
        {
            GameManager.instance.wave2Activate = true;
            enabled = false;
        }
        else if (Player.instance.transform.position.x >= this.transform.position.x && !GameManager.instance.wave3Activate && this.transform.name == "Wave3Trigger")
        {
            GameManager.instance.wave3Activate = true;
            enabled = false;
        }
    }
}