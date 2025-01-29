using BulletPro;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 이 클래스는 에테르 드론을 어떻게 제어할것인지에 대한 스크립트이다.
// 에테르 드론은 보스 적보다 강하지 않은 일반 필드 몬스터로 기획되어야 한다.
public class EtherDroneTest : MovingObject
{

    private enum EnemyState
    {
        Pause,      // 일시정지 상태. EnemyManager에서 Player가 일정 Transform을 통과하면 상태를 해제한다.
        Idle,
        Damaged
    }


    public float hp;                // 체력
    public Transform bulletEmitterTransform;    // BulletEmitter 트랜스폼 위치
    public BulletEmitter idleBulletEmitter;  // 대기 상태 시 사용하는 Bullet Emitter
    public float knockBackSpeed;        // 넉백 시 밀려나는 속도
    public float knockBackTime;         // 넉백 시 밀려나는 시간
    public float ether;                 // 가지고 있는 에테르, 죽으면 플레이어에게 부여된다.
    //public string waveType = "";             // 웨이브 유형 String
    public int waveLevel;

    public GameObject spriteAndAnimation;

    private bool isActive;

    public Animator animator;

    private EnemyState enemyState;
    private Coroutine dieCoroutine;

    private Coroutine movingCoroutine;
    private Vector2 targetPosition;

    private SpriteRenderer spriteRenderer;
    private Color currentColor;

#pragma warning disable CS0414
    [SerializeField] private Coroutine wave1ActivateCoroutine;

    

    //private float currentStunValue;
    private float currentSpeed;
    private bool isShooting;
    private bool isRandomMoved;
    private bool isWallStuck;

    protected override void Start()
    {
        AllInstantiate();
    }

    private void AllInstantiate()
    {

        enemyState = EnemyState.Pause;
        isRandomMoved = false;
        targetPosition = Vector2.zero;

        movingCoroutine = null;
        isShooting = true;
        dieCoroutine = null;
        isWallStuck = false;
        isActive = false;

        //currentStunValue = 0;
        currentSpeed = moveSpeed;
        idleBulletEmitter.Pause();

        spriteRenderer = spriteAndAnimation.GetComponent<SpriteRenderer>();
        currentColor = spriteRenderer.color;

        wave1ActivateCoroutine = null;
        base.Start();
    }

    void FixedUpdate()
    {
        CheckingGameOver();
        EnemyRotate();
        ActivateEnemyByStateUpdate();
    }

    private void ActivateEnemyByStateUpdate()
    {
        idleBulletEmitter.patternOrigin = bulletEmitterTransform.transform;
        switch (enemyState)
        {
            case EnemyState.Pause:
                CheckingWaveType();
                break;

            case EnemyState.Idle:
                if (isShooting)
                {
                    idleBulletEmitter.Play();
                    if (movingCoroutine == null)
                    {
                        if (isWallStuck)
                        {
                            movingCoroutine = StartCoroutine(WallStuckingEscape());
                        }
                        else if(!isWallStuck)
                        {
                            movingCoroutine = StartCoroutine(RandomMoving());
                        }
                    }
                }
                break;

            case EnemyState.Damaged:
                idleBulletEmitter.Pause();
                break;
        }
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


    // 이 밑으로, 각 상태에서 사용하는 함수를 정의한다

    // Pause에서 플레이어의 진행도를 감지
    private void CheckingWaveType()
    {
        /*
        if (WaveManager.instance.waveInfo[WaveManager.instance.waveCount].waveActivate && WaveManager.instance.waveCount == waveLevel && !CutsceneManager.instance.isCutscene && isActive == false)
        {
            isActive = true;
            enemyState = EnemyState.Idle;
        }
        */
        if (isActive == false)
        {
            isActive = true;
            enemyState = EnemyState.Idle;
        }
    }

    // Wave 2, 해당 타입의 드론이 앞으로 전진한 후 Idle로 전환한다.
    private IEnumerator MovingForward(float distance)
    {
        Vector2 targetPos = new Vector2(this.transform.position.x + distance, this.transform.position.y);
        moveSpeed = 10;

        while (Vector3.Distance(this.transform.position, targetPos) > 0.1f)
        {
            AttemptMove(distance, 0f);
            yield return null;
        }

        moveSpeed = currentSpeed;
        enemyState = EnemyState.Idle;
    }

    private void ChangeStateStunToIdle(EnemyState changeState)
    {
        enemyState = changeState;
    }

    // Damaged - > Idle
    private IEnumerator ChangeStateDamagedToIdleByDuration(float limitedDuration)
    {
        float elapsedTime = 0f;
        enemyState = EnemyState.Damaged;

        while (elapsedTime < limitedDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        enemyState = EnemyState.Idle;
    }

    public void TakeDamage(float damage)
    {
        if (enemyState == EnemyState.Pause)
        {
            return;
        }

        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Damaged");
            StartCoroutine(DamagedBlinkBlack(3f, 0.1f));
            StartCoroutine(ChangeStateDamagedToIdleByDuration(GetAnimationClipLength("EtherDroneDamaged")));
            StartCoroutine(KnockBack(knockBackSpeed, knockBackTime));
        }
    }
    private IEnumerator DamagedBlinkBlack(float blinkCount, float blinkDuration)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = Color.black;    // 검은색 전환
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = currentColor;  // 원래 색상으로 복원
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private float GetAnimationClipLength(string clipName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length + 0.25f; // Damaged 애니메이션 길이 반환
            }
        }
        Debug.LogWarning("애니메이션 클립을 찾을 수 없습니다: " + clipName);
        return 0f; // 찾지 못한 경우 0 반환
    }


    private void Die()
    {
        isShooting = false;
        if (dieCoroutine == null)
        {
            moveSpeed = 0f;
            dieCoroutine = StartCoroutine(Kill());
        }
    }

    private IEnumerator Kill()
    {
        GameManager.instance.killedEnemyScore++;
        animator.SetTrigger("Die");
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
                TakeDamage(Player.instance.attack);
                break;
        }
    }

    // 드론의 무작위 이동
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
            if (isWallStuck || enemyState == EnemyState.Damaged)
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
            if (enemyState == EnemyState.Damaged)
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

    // 드론의 물리적 충돌시 시행하는 로직
    // 드론의 이동 중 벽에 막혀서 움직임이 끼이는 경우 isWallStuck을 true로 전환한다.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.transform.tag == "Wall")
        {
            //Debug.Log("Oncolision");
            isWallStuck = true;
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
}