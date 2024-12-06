using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    
    public float moveSpeed;
    protected CircleCollider2D circleCollider;
    protected Sprite sprite;
    protected int wallLayerMask;
    //protected Quaternion originalRotate;
    //protected Animator animator

    protected Rigidbody2D rb2D;


    protected virtual void Start()
    {
        //originalRotate = transform.localRotation;
        //animator = GetComponent<Animator>();
        wallLayerMask = LayerMask.NameToLayer("InvisibleWall");
        rb2D = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        sprite = GetComponent<Sprite>();
    }

    // 오브젝트 움직임 함수
    protected virtual void AttemptMove(float xDir, float yDir)
    {
        //Move(xDir, yDir);
        StartCoroutine(Move(xDir, yDir));
    }

    protected IEnumerator Move(float xDir, float yDir)
    {
        float horizontal = rb2D.position.x + xDir;
        float vertical = rb2D.position.y + yDir;

        Vector3 targetPos = new Vector3(horizontal, vertical, 0f);
        Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPos, Time.fixedDeltaTime * moveSpeed);

        rb2D.MovePosition(nextPos);

        yield return null;
    }
}
