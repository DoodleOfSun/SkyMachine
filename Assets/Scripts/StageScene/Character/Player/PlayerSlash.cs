using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlash : MonoBehaviour
{
    //[HideInInspector] public static PlayerSlash instance;
    //private float attackValue;
    //private PolygonCollider2D slashCollider;
    //private Rigidbody2D rb2D;

    //private Animator animation;
    // Start is called before the first frame update

    
    void Start()
    {
        Init();
    }
    

    public void Init()
    {
        //rb2D = GetComponent<Rigidbody2D>();
        //slashCollider = GetComponent<PolygonCollider2D>(); 
        
        /*
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        */
    }

    public void Attack(float duration, GameObject go)
    {
        StartCoroutine(VFXAttack(duration, go));
    }

    private IEnumerator VFXAttack(float duration, GameObject go)
    {
        //gameObject.SetActive(true);
        // 필요하다면 이곳에 애니메이션 로직 입력

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
        go.gameObject.SetActive(false);
    }

    // 근접 히트박스를 활성화하는 경우, 이곳에서 적의 체력을 관리한다.
    // 이렇게 된 이유는 적의 Rigidbody2d는 실질적인 물리 계산에 사용되기 때문에 IsTriggered를 false로 해야만 하기 때문이다.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform != null && collision.transform.tag == "Enemy")
        {
            if (collision.transform.name.Contains("EtherDrone") || collision.transform.name.Contains("Test"))
            {
                Debug.Log(1111);
                EtherDrone ed = collision.GetComponent<EtherDrone>();
                ed.TakeDamage(DamageToEnemyHowMuch(this.transform.name));
            }
            if (collision.transform.name.Contains("EtherBot"))
            {
                EtherBot eb = collision.GetComponent<EtherBot>();
                eb.TakeDamage(DamageToEnemyHowMuch(this.transform.name));
            }
            if (collision.transform.name.Contains("Hester"))
            {
                Hester hs = collision.GetComponent<Hester>();
                if (hs.isStun)
                {
                    hs.TakeDamage(DamageToEnemyHowMuch(this.transform.name));
                }
            }
            if (collision.transform.name.Contains("EtherTower"))
            {
                EtherTower et = collision.GetComponent<EtherTower>();
                et.TakeDamage(DamageToEnemyHowMuch(this.transform.name));
            }
        }
    }

    // 공격 시, 히트박스 이름에 따라 그 데미지를 다르게 한다.
    private float DamageToEnemyHowMuch(string vfxName)
    {
        // 공격 히트박스
        if (vfxName.Contains("Slash"))
        {
            return Player.instance.attack;
        }
        // 레이저 히트박스
        else if (vfxName.Contains("Laser"))
        {
            return Player.instance.skill1Damage;
        }
        Debug.LogWarning("오류 : vfxName 찾을 수 없음. 0 반환");
        return 0;
    }
}