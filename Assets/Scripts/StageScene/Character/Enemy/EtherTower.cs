using BulletPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EtherTower : MovingObject
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

    public Animator animator;

    private EnemyState enemyState;

    private Coroutine dieCoroutine;
    private Coroutine attackCoroutine;

    private Vector2 targetPosition;

#pragma warning disable CS0414
    [SerializeField] private Coroutine wave1ActivateCoroutine;

    private SpriteRenderer spriteRenderer;
    private Color currentColor;

    //private float currentStunValue;
    private float currentSpeed;
    private bool isShooting;
    private bool isActive;


#pragma warning disable CS0414
    private bool isRandomMoved;


    private bool isWallStuck;

    // Start is called before the first frame update
    protected override void Start()
    {
        AllInstantiate();
    }

    private void AllInstantiate()
    {

        enemyState = EnemyState.Pause;
        isRandomMoved = false;
        targetPosition = Vector2.zero;
        isShooting = true;
        isWallStuck = false;
        isActive = false;

        //currentStunValue = 0;
        currentSpeed = moveSpeed;
        idleBulletEmitter.Pause();

        dieCoroutine = null;
        wave1ActivateCoroutine = null;
        attackCoroutine = null;

        spriteRenderer = spriteAndAnimation.GetComponent<SpriteRenderer>();
        currentColor = spriteRenderer.color;

        base.Start();
    }


    void FixedUpdate()
    {
        CheckingGameOver();
        //EnemyRotate();
        ActivateEnemyByStateUpdate();
    }

    // Pause에서 플레이어의 진행도를 감지
    private void CheckingWaveType()
    {
        // 테스트용
        
        if (GameManager.instance.currentSceneName == "SampleScene" && isActive == false)
        {
            isActive = true;
            enemyState = EnemyState.Idle;
        }
        
        else if (WaveManager.instance.waveInfo[WaveManager.instance.waveCount].waveActivate && WaveManager.instance.waveCount == waveLevel && !CutsceneManager.instance.isCutscene && isActive == false)
        {
            isActive = true;
            enemyState = EnemyState.Idle;
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

    private void ActivateEnemyByStateUpdate()
    {
        idleBulletEmitter.patternOrigin = bulletEmitterTransform.transform;
        switch (enemyState)
        {
            case EnemyState.Pause:
                CheckingWaveType();
                break;

            case EnemyState.Idle:
                if (isShooting && attackCoroutine == null)
                {
                    attackCoroutine = StartCoroutine(Attack(3f));
                }
                break;

            case EnemyState.Damaged:
                idleBulletEmitter.Pause();
                break;
        }
    }

    private IEnumerator Attack(float limitedTime)
    {
        Debug.Log("EtehrTower 공격");
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

    public void TakeDamage(float damage)
    {
        if (enemyState == EnemyState.Pause)
        {
            return;
        }

        AudioManager.instance.PlayingSFX("DamageMetalHeavy");
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Damaged");
            //StartCoroutine(DamagedBlinkBlack(3f, 0.1f));
            StartCoroutine(ChangeStateDamagedToIdleByDuration(GetAnimationClipLength("EtherTowerDamaged")));
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
        AudioManager.instance.PlayingSFX("Explosion");
        isShooting = false;
        if (dieCoroutine == null)
        {
            dieCoroutine = StartCoroutine(Kill());
        }
    }

    private IEnumerator Kill()
    {
        GameManager.instance.killedEnemyScore++;
        animator.SetTrigger("Die");
        Debug.Log("죽음");
        bulletEmitterTransform.gameObject.SetActive(false);
        idleBulletEmitter.Pause();
        Player.instance.EtherIncreseByKillEnemy(ether);
        yield return new WaitForSeconds(GetAnimationClipLength("EtherTowerDie") + 0.5f);
        Destroy(gameObject);
    }
}
