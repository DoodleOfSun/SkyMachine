using BulletPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 이 클래스는 에테르 드론을 어떻게 제어할것인지에 대한 스크립트이다.
// 에테르 드론은 보스 적보다 강하지 않은 일반 필드 몬스터로 기획되어야 한다.
// 따라서 카운터 State를 적용시키지 않았고, 다른 적들보다도 hp가 약하다.
public class EtherDrone : MovingObject
{

    private enum EnemyState
    {
        Idle,
        Guard,
        Stun
    }


    [HideInInspector] public static EtherDrone instance;

    public float hp;                // 체력
    public float stunValueLimit;         // 스턴치
    public float guardDurationLimit;     // 최대 가드 지속시간
    public float stunDurationLimit;         // 최대 스턴 지속시간
    public Transform bulletEmitterTransform;    // BulletEmitter 트랜스폼 위치
    public BulletEmitter idleBulletEmitter;  // 대기 상태 시 사용하는 Bullet Emitter
    public float knockBackSpeed;        // 넉백 시 밀려나는 속도
    public float knockBackTime;         // 넉백 시 밀려나는 시간

    public Animator animator;

    private EnemyState enemyState;

    private bool isDamagedWhileGuard;

    private float currentStunValue;
    private float currentSpeed;

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

        isDamagedWhileGuard = false;

        currentStunValue = 0;
        currentSpeed = moveSpeed;
        idleBulletEmitter.Pause();
        base.Start();
    }

    void FixedUpdate()
    {
        CheckingStun();
        ActivateEnemyByStateUpdate();
    }

    private void ActivateEnemyByStateUpdate()
    {
        idleBulletEmitter.patternOrigin = bulletEmitterTransform.transform;
        switch (enemyState)
        {
            case EnemyState.Idle:
                idleBulletEmitter.Play();
                break;
            case EnemyState.Guard:
                idleBulletEmitter.Pause();
                break;
            case EnemyState.Stun:
                idleBulletEmitter.Pause();
                break;
        }

        isDamagedWhileGuard = false;
    }

    // 이 밑으로, 각 상태에서 사용하는 함수를 정의한다

    // 이 함수는 가드 상태에서 피격당하면 스턴치를 쌓는다. 이 함수는 일정 시간동안 공격당하지 않으면 상태를 Idle로 전환시킨다.
    // GuardCount 만큼 피격당하면 플레이어를 크게 밀쳐내고 Counter 상태로 전환시킨다.
    private void Guard()
    {
        currentStunValue++;
    }

    // 상태를 전환시켜주는 함수들

    // 통상 -> 가드
    private void ChangeStateIdleToGuard()
    {
        // 대기에서 가드 상태로 전환, (공격을 가드함) << 이 함수를 한번 실행하고, 바로 가드 상태로 전환한다.
        Guard();
        enemyState = EnemyState.Guard;
        StartCoroutine(ChangeStateGuardToIdleByDuration(EnemyState.Guard, guardDurationLimit));
    }

    // 가드 -> 통상
    private IEnumerator ChangeStateGuardToIdleByDuration(EnemyState currentState, float limitedDuration)
    {

        float elapsedTime = 0f;
        while (elapsedTime < limitedDuration)
        {
            // 만약 이 함수가 종료되기 전에 어떠한 이유로 enemyState가 currentState가 아니게 되었을 때, 시간 여부와 관계없이 함수를 강제로 종료시킨다.
            if (enemyState != currentState)
            {
                yield break;
            }
            // 그게 아닌 경우, 가드 중 피격을 한번이라도 했는지를 검사한다.
            // 이 bool 변수는 FixedUpdate에서 계속 false로 초기화된다.
            // 이후 OnTriggerEnter2D에서 한번이라도 true가 된다면 elapsedTime을 0f로 초기화한다.
            else if (isDamagedWhileGuard)
            {
                elapsedTime = 0f;
            }
            // 그 외에는 시간이 흘러가게 한다.
            else
            {
                elapsedTime += Time.fixedDeltaTime;
            }
            yield return new WaitForFixedUpdate();
        }
        enemyState = EnemyState.Idle;
    }


    private void CheckingStun()
    {
        // 스턴치 검사
        if (currentStunValue >= stunValueLimit)
        {
            StartCoroutine(Stun());
        }
    }

    // 스턴치 초과로 인한 스턴
    private IEnumerator Stun()
    {
        Debug.Log("가드 브레이크! 스턴!");

        currentStunValue = 0;
        enemyState = EnemyState.Stun;

        idleBulletEmitter.Kill();

        // 이곳에 애니메이션 로직 입력

        float elapsedTime = 0f;

        while (elapsedTime < stunDurationLimit)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("스턴 종료");
        ChangeStateStunToIdle(EnemyState.Idle);
    }

    private void ChangeStateStunToIdle(EnemyState changeState)
    {
        enemyState = changeState;
    }

    public void TakeDamage(float damage)
    {
        animator.SetTrigger("Damaged");
        Debug.Log("에너미 데미지 받음.");
        hp -= damage;
        StartCoroutine(KnockBack(knockBackSpeed, knockBackTime));
        if (hp <= 0)
        {
            Kill();
        }
    }

    private void Kill()
    {
        animator.SetTrigger("Die");
        idleBulletEmitter.Kill();
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
                ChangeStateIdleToGuard();
                Debug.Log("대기 상태 피격");
                break;
            case EnemyState.Guard:
                Debug.Log("가드 상태 충돌");
                isDamagedWhileGuard = true;
                Guard();
                break;
            case EnemyState.Stun:
                TakeDamage(Player.instance.attack);
                break;
        }
    }
}
