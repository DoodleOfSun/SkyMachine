using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.transform.tag == "Player" && WaveManager.instance.isCleared)
        {
            WaveManager.instance.MovingNextRoom(this.transform.name);
        }
    }
}