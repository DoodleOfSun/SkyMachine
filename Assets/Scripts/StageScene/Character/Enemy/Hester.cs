using BulletPro;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Hester : MovingObject
{
    private enum EnemyState
    {
        Pause,
        Idle,
        Guard,
        Counter,
        Stun
    }

    [HideInInspector] public static Hester instance;
    public float hp;                // 체력
    public int stunValueLimit;         // 스턴치
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
    public float waveLevel;             // 배치된 웨이브 레벨

    [HideInInspector] public bool isStun;   // 스턴 상태인지 외부에서 접근

    //public GameObject allBulletEmitterTransform;    // 모든 Bullet Emitter의 부모 게임오브젝트인데, 위치 초기화를 위해서 사용한다.
    public BulletEmitter idleBulletEmitter;  // 대기 상태 시 사용하는 Bullet Emitter
    public BulletEmitter counterBulletEmitter;  // 카운터 상태 시 사용하는 Bullet Emitter

    public GameObject spriteAndAnimation;

    // VFX
    public GameObject barrierVFXPrefabs;
    private GameObject barrierVFX;
    public GameObject rageVFXPrefabs;
    private GameObject rageVFX;
    public GameObject stunVFXPrefabs;
    private GameObject stunVFX;


    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color currentColor;

    private bool isWallStuck;
    private bool isRandomMoved;

    private EnemyState enemyState;

    private bool isDamagedWhileGuard;

    private Vector2 targetPosition;

    private int currentGuardCount;
    private float currentStunValue;
    private float currentSpeed;

    private Coroutine attackCoroutine;
    private Coroutine movingCoroutine;
    private Coroutine rageCoroutine;

    protected override void Start()
    {
        AllInstantiate();
    }

    void FixedUpdate()
    {
        CheckingStun();
        CheckingGameOver();
        EnemyRotate();
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

        targetPosition = Vector2.zero;

        enemyState = EnemyState.Pause;

        isDamagedWhileGuard = false;

        currentGuardCount = 0;
        currentStunValue = 0;
        currentSpeed = moveSpeed;
        idleBulletEmitter.Pause();
        counterBulletEmitter.Pause();
        spriteRenderer = spriteAndAnimation.GetComponent<SpriteRenderer>();
        animator = spriteAndAnimation.GetComponent<Animator>();
        isStun = false;
        attackCoroutine = null;
        movingCoroutine = null;
        rageCoroutine = null;
        isWallStuck = false;
        isRandomMoved = false;
        // 방어막 VFX 오브젝트
        barrierVFX = Instantiate(barrierVFXPrefabs);
        barrierVFX.gameObject.SetActive(false);
        
        // 분노 VFX 오브젝트
        rageVFX = Instantiate(rageVFXPrefabs);
        rageVFX.gameObject.SetActive(false);

        // 기절 VFX 오브젝트
        stunVFX = Instantiate(stunVFXPrefabs);
        stunVFX.gameObject.SetActive(false);
        base.Start();
    }

    private void CheckingGameOver()
    {
        if (GameManager.instance.gmState == GameManager.GameManagerState.GameOver)
        {
            enemyState = EnemyState.Pause;
        }
    }

    // 적 객체의 스프라이트가 플레이어를 향해 각도를 전환함
    private void EnemyRotate()
    {
        if (Player.instance.transform.position.x >= this.transform.position.x)
        {
            spriteAndAnimation.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
        else
        {
            spriteAndAnimation.transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
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
            case EnemyState.Pause:
                
                break;
            case EnemyState.Idle:

                if (attackCoroutine == null)
                {
                    attackCoroutine = StartCoroutine(IdleAttack(0.7f));
                }

                if (movingCoroutine == null)
                {
                    if (isWallStuck)
                    {
                        movingCoroutine = StartCoroutine(WallStuckingEscape());
                    }
                    else if (!isWallStuck)
                    {
                        movingCoroutine = StartCoroutine(RandomMoving());
                    }
                }

                //idleBulletEmitter.Play();
                //counterBulletEmitter.Pause();
                break;
            case EnemyState.Guard:
                break;
            case EnemyState.Counter:

                if (attackCoroutine == null)
                {
                    attackCoroutine = StartCoroutine(IdleAttack(0.3f));
                }
                if (rageCoroutine == null)
                {
                    rageCoroutine = StartCoroutine(RageAttack(0.7f));
                }
                break;
            case EnemyState.Stun:
                stunVFX.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1f, this.transform.position.z + -1);
                break;
        }
        isDamagedWhileGuard = false;
    }

    private void CheckingCutscene()
    {
        if (!CutsceneManager.instance.isCutscene)
        {
            enemyState = EnemyState.Idle;
        }
    }

    // BUG : 이 함수가 isCutscene의 초기화보다 더 빠르게 실행되어서 컷신 도중에 먼저 움직여버린다.
    // NOTE : 함수를 CutsceneManager에서 호출시켜서 해결
    // 이 밑으로, 각 상태에서 사용하는 함수를 정의한다
    // Pause에서 플레이어의 진행도를 감지
    public void CheckingWaveType()
    {
        if (WaveManager.instance.waveInfo[WaveManager.instance.waveCount].waveActivate && WaveManager.instance.waveCount == waveLevel && !CutsceneManager.instance.isCutscene)
        {
            enemyState = EnemyState.Idle;
        }
    }


    // 대기상태 시 기본 공격, limitedTime은 다시 공격하고 대기하는 동안의 시간
    private IEnumerator IdleAttack(float limitedTime)
    {
        float elapsedTime = 0f;

        animator.SetTrigger("Attack");
        // 실제로 탄막을 쏘고 Pause로 돌리는데 0.2f가 걸리게 한다. (이유는 BulletEmitter에서 공격속도를 0.2F로 조정해놓았기 때문에.)

        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        idleBulletEmitter.Play();

        while (elapsedTime < 0.2f)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }

        idleBulletEmitter.Stop();

        // 초기화하고 다시 공격할떄까지 대기

        elapsedTime = 0f;

        while (elapsedTime < limitedTime)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }

        attackCoroutine = null;
    }

    private IEnumerator RageAttack(float limitedTime)
    {
        float elapsedTime = 0f;
        // 실제로 탄막을 쏘고 Pause로 돌리는데 0.2f가 걸리게 한다. (이유는 BulletEmitter에서 공격속도를 0.2F로 조정해놓았기 때문에.)

        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        counterBulletEmitter.Play();

        while (elapsedTime < 0.2f)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }

        counterBulletEmitter.Stop();

        // 초기화하고 다시 공격할떄까지 대기

        elapsedTime = 0f;

        while (elapsedTime < limitedTime)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }

        rageCoroutine = null;
    }



    // 이 함수는 가드 상태에서 피격당하면 스턴치를 쌓는다. 이 함수는 일정 시간동안 공격당하지 않으면 상태를 Idle로 전환시킨다.
    // GuardCount 만큼 피격당하면 플레이어를 크게 밀쳐내고 Counter 상태로 전환시킨다.
    private void Guard()
    {
        // 가드 횟수 검사 후 플레이어를 밀쳐내고 카운터 공격 실행
        if (currentGuardCount >= guardCountLimit)
        {
            // 이곳에 플레이어 넉백시키기 함수 실행
            StartCoroutine(Player.instance.Dashing(counterKnockBackValue, counterKnockBackTime, true));
            Debug.Log("강력한 카운터 공격 실행");
            ChangeStateGuardToCounter();
            currentGuardCount = 0;
        }

        // 가드 횟수와 스턴치를 쌓고 애니메이션 재생
        else
        {
            // 가드 애니메이션 실행
            animator.SetTrigger("Attack");
            StartCoroutine(GuardVFX());
            currentGuardCount++;
            currentStunValue++;
            Debug.Log(currentStunValue);
            // 플레이어를 밀침
            StartCoroutine(Player.instance.Dashing(guardKnockBackValue, guardKnockBackTime, true));

            // 애니메이션 재생

        }
    }

    private IEnumerator GuardVFX()
    {
        barrierVFX.transform.position = this.transform.position;
        barrierVFX.SetActive(true);
        float elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        barrierVFX.SetActive(false);
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
        rageVFX.transform.position = this.transform.position;
        rageVFX.SetActive(true);
        
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

        rageVFX.SetActive(false);
        enemyState = EnemyState.Idle;
    }



    // 스턴 -> 통상
    private void ChangeStateStunToIdle(EnemyState changeState)
    {
        enemyState = changeState;
    }

    private IEnumerator RandomMoving()
    {
        float randomX = Random.Range(-2f, 2f);
        float randomY = Random.Range(-2f, 2f);
        // isRandomMoved가 false일 때 새로운 위치 지정
        if (!isRandomMoved)
        {
            // 새로운 랜덤 위치 지정
            targetPosition = new Vector2(
                transform.position.x + randomX,
                transform.position.y + randomY);
            isRandomMoved = true; // 이동 시작 상태로 변경
        }

        // 현재 위치에서 목표 위치로 이동
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f) // 목표에 도달할 때까지
        {
            if (isWallStuck || enemyState == EnemyState.Guard || enemyState == EnemyState.Counter)
            {
                break;
            }
            else
            {
                AttemptMove(randomX, randomY);
                //transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null; // 다음 프레임까지 대기
            }
        }

        // 목표 위치에 도달한 경우
        isRandomMoved = false; // 다시 새로운 위치 지정 가능
        yield return new WaitForSeconds(1f); // 다음 이동 전 대기 (선택적)
        movingCoroutine = null;
    }


    private IEnumerator WallStuckingEscape()
    {
        float xDir = (Player.instance.transform.position - transform.position).normalized.x;
        float yDir = (Player.instance.transform.position - transform.position).normalized.y;
        targetPosition = new Vector2(
            transform.position.x + xDir,
            transform.position.y + yDir);

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            if (enemyState == EnemyState.Guard || enemyState == EnemyState.Stun)
            {
                yield return null;
                break;
            }
            AttemptMove(xDir, yDir);
            yield return null; // 다음 프레임까지 대기
        }

        isRandomMoved = false; // 다시 새로운 위치 지정 가능
        isWallStuck = false;
        yield return new WaitForSeconds(1f); // 다음 이동 전 대기 (선택적)
        movingCoroutine = null;

    }

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

                        break;
                }
            }
            else if (collision.transform.tag == "Wall")
            {
                //Debug.Log("Oncolision");
                isWallStuck = true;
            }
        }
    }

    // 추가적인 버그를 방지하기 위해 OnCollisionStay의 경우도 추가해주었다.
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision != null && collision.transform.tag == "Wall")
        {
            //Debug.Log("Oncolision");
            isWallStuck = true;
        }
    }
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
        stunVFX.transform.position = this.transform.position;
        stunVFX.SetActive(true);
        currentStunValue = 0;
        enemyState = EnemyState.Stun;

        idleBulletEmitter.Kill();
        counterBulletEmitter.Kill();
        isStun = true;

        // 이곳에 애니메이션 로직 입력

        float elapsedTime = 0f;

        while (elapsedTime < stunDurationLimit)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        stunVFX.SetActive(false);
        isStun = false;
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
