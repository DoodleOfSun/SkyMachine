using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlash : MonoBehaviour
{
    //private float attackValue;
    private PolygonCollider2D slashCollider;
    private Rigidbody2D rb2D;

    //private Animator animation;
    // Start is called before the first frame update

    /*
    void Start()
    {

        rb2D = GetComponent<Rigidbody2D>();
        slashCollider = GetComponent<PolygonCollider2D>();
    }
    */

    public void Init(float attack)
    {
        rb2D = GetComponent<Rigidbody2D>();
        slashCollider = GetComponent<PolygonCollider2D>();
        //attackValue = attack;
    }

    public void attack(float duration)
    {
        StartCoroutine(meleeAttack(duration));
    }

    private IEnumerator meleeAttack(float duration)
    {
        //gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);
        /*
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        */
        // 근접공격 종료
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        
        if (collision.transform != null && collision.transform.tag == "Enemy")
        {
            Debug.Log(collision.tag);
            //GameManager.instance.currentAttackValue = attackValue;
        }
        
    }
}