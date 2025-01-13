using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollowByTransform : MonoBehaviour
{

    private Vector3 currentVelocity;
    public static CamFollowByTransform instance;
    // Start is called before the first frame update
    void Start()
    {
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
        /*
        Vector3 targetPos = Player.instance.transform.position;
        targetPos.z = -10f;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 0.3f);
        newPos.y = 0f;
        transform.position = newPos;
        */
    }

    public IEnumerator MoveByTransform(Vector3 targetPos)
    {
        //transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z); 
        while (Vector3.Distance(this.transform.position, targetPos) > 0.1f)
        {
            targetPos.z = -10f;
            Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 0.3f);
            newPos.y = 0f;
            transform.position = newPos;
            yield return null;
        }
    }
}
