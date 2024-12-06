using BulletPro;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class EnemyTest : MovingObject
{
    private enum EnemyState
    {
        Idle,
        Guard,
        Counter,
        Stun
    }

    [HideInInspector] public static EnemyTest instance;
    public float hp;                // 체력
    public float stunValueLimit;         // 스턴치
    public int guardCountLimit;          // 가드 횟수
    public float guardDurationLimit;     // 최대 가드 지속시간
    public float counterDurationLimit;   // 최대 카운터 지속시간
    public float stunDurationLimit;         // 최대 스턴 지속시간
    public float smallStunDurationLimit;     // 소경직 지속시간
    public float guardKnockBackValue;   // 가드 시 밀쳐내는 힘
    public float guardKnockBackTime;    // 가드 시 밀쳐내는 시간
    public float counterKnockBackValue;  // 카운터 공격 개시 시 밀쳐내는 힘
    public float counterKnockBackTime;   // 카운터 공격 개시 시 밀쳐내는 시간
    public float knockBackSpeed;        // 넉백 시 밀려나는 속도
    public float knockBackTime;         // 넉백 시 밀려나는 시간

    //public GameObject allBulletEmitterTransform;    // 모든 Bullet Emitter의 부모 게임오브젝트인데, 위치 초기화를 위해서 사용한다.
    public BulletEmitter idleBulletEmitter;  // 대기 상태 시 사용하는 Bullet Emitter
    public BulletEmitter counterBulletEmitter;  // 카운터 상태 시 사용하는 Bullet Emitter

    private EnemyState enemyState;

    private bool isDamagedWhileGuard;

    private int currentGuardCount;
    private float currentStunValue;
    private float currentSpeed;

    protected override void Start()
    {
        AllInstantiate();
    }

    void FixedUpdate()
    {
        CheckingStun();
        ActivateEnemyByStateUpdate();
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

        currentGuardCount = 0;
        currentStunValue = 0;
        currentSpeed = moveSpeed;
        idleBulletEmitter.Pause();
        counterBulletEmitter.Pause();
        base.Start();
    }

    // 이 함수는 실시간으로 실행시켜야하는 함수가 존재할 때, 상태에 따라 실행시킨다.
    // 또, 이 함수에서 BulletEmitter들을 제어한다.
    private void ActivateEnemyByStateUpdate()
    {

        //allBulletEmitterTransform.transform.position = transform.position;

        idleBulletEmitter.patternOrigin = transform;
        counterBulletEmitter.patternOrigin = transform;

        switch (enemyState)
        {
            case EnemyState.Idle:
                idleBulletEmitter.Play();
                counterBulletEmitter.Pause();
                break;
            case EnemyState.Guard:
                idleBulletEmitter.Pause();
                counterBulletEmitter.Pause();
                break;
            case EnemyState.Counter:
                idleBulletEmitter.Pause();
                counterBulletEmitter.Play();
                break;
            case EnemyState.Stun:
                idleBulletEmitter.Pause();
                counterBulletEmitter.Pause();
                break;
        }
        isDamagedWhileGuard = false;
    }

    // 이 밑으로, 각 상태에서 사용하는 함수를 정의한다

    // 이 함수는 가드 상태에서 피격당하면 스턴치를 쌓는다. 이 함수는 일정 시간동안 공격당하지 않으면 상태를 Idle로 전환시킨다.
    // GuardCount 만큼 피격당하면 플레이어를 크게 밀쳐내고 Counter 상태로 전환시킨다.
    private void Guard()
    {
        // 가드 애니메이션 실행
        // 가드 횟수 검사 후 플레이어를 밀쳐내고 카운터 공격 실행
        if(currentGuardCount >= guardCountLimit)
        {
            // 이곳에 플레이어 넉백시키기 함수 실행
            StartCoroutine(Player.instance.KnockBack(counterKnockBackValue, counterKnockBackTime));
            Debug.Log("강력한 카운터 공격 실행");
            ChangeStateGuardToCounter();
            currentGuardCount = 0;
        }
        // 가드 횟수와 스턴치를 쌓고 애니메이션 재생
        else
        {
            currentGuardCount++;
            currentStunValue++;

            // 플레이어를 밀침
            //StartCoroutine(Player.instance.KnockBack(guardKnockBackValue, guardKnockBackTime));

            // 애니메이션 재생

        }

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
        Debug.Log("상태 바꾸기");
        while (elapsedTime < limitedDuration)
        {
            // 만약 이 함수가 종료되기 전에 어떠한 이유로 enemyState가 currentState가 아니게 되었을 때, 시간 여부와 관계없이 함수를 강제로 종료시킨다.
            if (enemyState != currentState)
            {
                Debug.Log("현재 State : " + enemyState + "이므로 상태 바꾸기, ChangeStateGuardToIdleByDuration 함수 강제 종료");
                yield break;
            }
            // 그게 아닌 경우, 가드 중 피격을 한번이라도 했는지를 검사한다.
            // 이 bool 변수는 FixedUpdate에서 계속 false로 초기화된다.
            // 이후 OnTriggerEnter2D에서 한번이라도 true가 된다면 elapsedTime을 0f로 초기화한다.
            else if (isDamagedWhileGuard)
            {
                Debug.Log("가드 중 피격, 시간 초기화");
                elapsedTime = 0f;
            }
            // 그 외에는 시간이 흘러가게 한다.
            else
            {
                elapsedTime += Time.fixedDeltaTime;
            }
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("상태 바꾸기 종료");
        enemyState = EnemyState.Idle;
    }

    // 가드 -> 카운터
    // 가드에서 카운터로 변경되었을 때 한번 실행되는 함수
    private void ChangeStateGuardToCounter()
    {
        StartCoroutine(Counter());
    }

    // 카운터는 counterDurationLimit만큼 지속된다.
    // 그리고 현재 상태가 다른 함수에 의해 Counter가 아닌 다른 상태인 경우 그냥 이 함수를 조기에 종료시킨다
    // 이유는 대개 카운터에서 다른 상태가 된다면 스턴밖에 없는데, 스턴 그거 무시하고 Idle로 돌려줄 이유가 없음.
    private IEnumerator Counter()
    {
        enemyState = EnemyState.Counter;
        float elapsedTime = 0f;
        while (elapsedTime < counterDurationLimit)
        {
            if (enemyState != EnemyState.Counter)
            {
                yield break;
            }
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        enemyState = EnemyState.Idle;
    }

    // 스턴 -> 통상
    private void ChangeStateStunToIdle(EnemyState changeState)
    {
        enemyState = changeState;
    }

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

    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌을 하는 경우
        if (collision.transform != null)
        {
            if (collision.transform.tag == "PlayerDanmaku")
            {
                switch (enemyState)
                {
                    // 대기 상태에서 공격을 당한 경우
                    case EnemyState.Idle:
                        ChangeStateIdleToGuard();
                        Debug.Log("대기 상태 충돌");
                        break;
                    // 가드 상태에서 공격을 당한 경우
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
    }
    */

    public void TakeDamage(float damage)
    {
        Debug.Log("에너미 데미지 받음.");
        hp -= damage;
        StartCoroutine(KnockBack(knockBackSpeed, knockBackTime));
        if (hp <= 0)
        {
            Kill();
        }
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


    private void Kill()
    {
        idleBulletEmitter.Kill();
        counterBulletEmitter.Kill();
        Destroy(gameObject);
    }

    // 카운터 공격 중 스킬로 피격당하면 잠시 경직됨 (소경직)
    /*
    public void TakeSkill()
    {
        currentStunValue += 5;
        if (enemyState == EnemyState.Counter)
        {
            StartCoroutine(SmallStun());
        }
    }
    */

    // 스킬로 피격당했을 경우 소경직이 아니라 스턴을 주는 로직을 넣어봄.
    public void TakeSkill()
    {
        StartCoroutine(Stun());
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
        counterBulletEmitter.Kill();

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

    // 소경직 로직
    private IEnumerator SmallStun()
    {
        enemyState = EnemyState.Stun;
        counterBulletEmitter.Pause();

        // 이곳에 애니메이션 로직 입력

        float elapsedTime = 0f;

        while (elapsedTime < smallStunDurationLimit)
        {
            if (currentStunValue >= stunValueLimit)
            {
                yield break;
            }
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        counterBulletEmitter.Play();
        enemyState = EnemyState.Counter;
    }
}