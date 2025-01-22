using BulletPro;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MovingObject : MonoBehaviour
{
    
    public float moveSpeed;
    protected CircleCollider2D circleCollider;
    protected Sprite sprite;
    //protected Quaternion originalRotate;
    //protected Animator animator

    protected Rigidbody2D rb2D;

    protected Vector3 currentVelocity;

    protected virtual void Start()
    {
        //originalRotate = transform.localRotation;
        //animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        sprite = GetComponent<Sprite>();
    }

    // 오브젝트 움직임 함수
    protected virtual void AttemptMove(float xDir, float yDir)
    {
        StartCoroutine(Move(xDir, yDir));
    }

    protected IEnumerator Move(float xDir, float yDir)
    {
        UsingRb2D(xDir, yDir);
        //UsingSmoothDamp(xDir, yDir);
        yield return null;
    }
    
    // NOTE : 정상작동하나, 기초적이다.
    private void UsingRb2D(float xDir, float yDir)
    {
        float horizontal = rb2D.position.x + xDir;
        float vertical = rb2D.position.y + yDir;

        Vector3 targetPos = new Vector3(horizontal, vertical, 0f);
        Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPos, Time.fixedDeltaTime * moveSpeed);

        rb2D.MovePosition(nextPos);
    }

    // HACK : 부드러운 이동을 위한 SmoothDamp를 사용한 이동 로직
    // 다른 이동 함수들과 호환이 되지 않는다 (Ex. 공격, 스킬, 회전 등)
    private void UsingSmoothDamp(float xDir, float yDir)
    {
        moveSpeed = moveSpeed * 50;
        // 이동 방향 벡터 계산
        Vector3 direction = new Vector3(xDir, yDir, 0f).normalized;

        // 목표 위치 계산 (현재 위치에 이동 방향을 moveSpeed로 곱한 값)
        Vector3 targetPosition = transform.position + direction * moveSpeed * Time.fixedDeltaTime;

        // SmoothDamp로 부드럽게 이동
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 0.8f);
        
    }

    protected IEnumerator UsingSmoothDampByPos(Vector3 targetPos)
    {
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
