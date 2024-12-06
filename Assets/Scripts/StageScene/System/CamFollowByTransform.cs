using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollowByTransform : MonoBehaviour
{

    private Vector3 currentVelocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = Player.instance.transform.position;
        targetPos.z = -10f;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 0.1f);
        newPos.y = 0f;
        transform.position = newPos;
    }
}
