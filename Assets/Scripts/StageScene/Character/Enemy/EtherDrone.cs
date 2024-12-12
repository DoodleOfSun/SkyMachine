using BulletPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 이 클래스는 에테르 드론을 어떻게 제어할것인지에 대한 스크립트이다.
// 에테르 드론은 보스 적보다 강하지 않은 일반 필드 몬스터로 기획되어야 한다.
public class EtherDrone : MovingObject
{

    private enum EnemyState
    {
        Idle,
    }


    [HideInInspector] public static EtherDrone instance;

    public float hp;                // 체력
    public Transform bulletEmitterTransform;    // BulletEmitter 트랜스폼 위치
    public BulletEmitter idleBulletEmitter;  // 대기 상태 시 사용하는 Bullet Emitter
    public float knockBackSpeed;        // 넉백 시 밀려나는 속도
    public float knockBackTime;         // 넉백 시 밀려나는 시간
    public float ether;                 // 가지고 있는 에테르, 죽으면 플레이어에게 부여된다.

    public Animator animator;

    private EnemyState enemyState;
    private Coroutine dieCoroutine;

    //private float currentStunValue;
    private float currentSpeed;
    private bool isShooting;

    protected override void Start()
    {
        AllInstantiate();
    }

    private void AllInstantiate()
    {

        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
        }

        enemyState = EnemyState.Idle;

        isShooting = true;
        dieCoroutine = null;

        //currentStunValue = 0;
        currentSpeed = moveSpeed;
        idleBulletEmitter.Pause();
        base.Start();
    }

    void FixedUpdate()
    {
        ActivateEnemyByStateUpdate();
    }

    private void ActivateEnemyByStateUpdate()
    {
        idleBulletEmitter.patternOrigin = bulletEmitterTransform.transform;
        switch (enemyState)
        {
            case EnemyState.Idle:
                if (isShooting)
                {
                    idleBulletEmitter.Play();
                }
                break;
        }
    }

    // 이 밑으로, 각 상태에서 사용하는 함수를 정의한다
    private void ChangeStateStunToIdle(EnemyState changeState)
    {
        enemyState = changeState;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("에너미 데미지 받음.");
        hp -= damage;
        if (hp <= 0)
        {
            isShooting = false;
            if (dieCoroutine == null)
            {
                dieCoroutine = StartCoroutine(Kill());
            }
        }
        else
        {
            animator.SetTrigger("Damaged");
            StartCoroutine(KnockBack(knockBackSpeed, knockBackTime));
        }
    }

    private IEnumerator Kill()
    {
        animator.SetTrigger("Die");
        Debug.Log("죽음");
        bulletEmitterTransform.gameObject.SetActive(false);
        idleBulletEmitter.Pause();
        Player.instance.EtherIncreseByKillEnemy(ether);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    // 원하는 좌표와 넉백 거리, 넉백 시간을 받아 플레이어가 존재하는 방향과 반대인 방향으로 받은 힘만큼 객체를 이동시킨다.
    public IEnumerator KnockBack(float distance, float knockBackTime)
    {

        float elapsedTime = 0f;

        moveSpeed = knockBackSpeed;

        float xDir = 0;
        float yDir = 0;

        xDir = (distance * (Player.instance.transform.position - transform.position).normalized.x) * -1f;
        yDir = (distance * (Player.instance.transform.position - transform.position).normalized.y) * -1f;

        while (elapsedTime < knockBackTime)
        {
            AttemptMove(xDir, yDir);
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        moveSpeed = currentSpeed;
    }

    // Bullet Receiver에서 이 함수를 호출한다.
    public void InteractiveWithPlayerBullet()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                Debug.Log("대기 상태 피격");
                TakeDamage(Player.instance.attack);
                break;
        }
    }
}
