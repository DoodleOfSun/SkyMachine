using BulletPro;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Player : MovingObject
{

    private enum AttackState
    {
        UpSlash,
        Downslash,
    }

    [HideInInspector] public static Player instance;

    // 기초 능력치 관련 변수
    public float attack;
    public int health;
    public float attackDuration;        // 공격판정 지속시간
    public float invincibleTime;        // 무적시간
    public float ether;                 // 에테르
    public float etherAttackCost;       // 공격 시 사용하는 에테르 코스트
    public float etherSkill1Cost;        // 스킬 사용 시 사용하는 에테르 코스트
    public float etherParryCost;        // 패리 사용 시 사용하는 에테르 코스트

    // 스킬 사용 관련 변수

    //[HideInInspector] public int zamielCountInt;        // 자미엘 카운트. 일정 수치가 되면 발사한다
    //public float zamielAttack;          // 자미엘의 공격력

    [HideInInspector] public bool isReadyToSkill1;           // 자미엘 카운트가 zamielReloadInt만큼 쌓였는지를 확인한다. 다음 좌클릭은 반드시 자미엘을 사격하고 zamielCountInt를 0으로 되돌린다.
    public int blinkCount;              // 피격판정 시 스프라이트 깜빡거림 횟수

    public float attackDashingTime;                     // 공격 시 전진하는 시간
    public float attackDashingSpeed;             // 공격 시 전진 속도
    public float attackDashingDistance; // 공격할 때 전진할 거리
    public float focusSpeed;            // 공격 패리 조준 모드일 때 이동속도 (통상보다 저하됨)
    public float blinkDuration;             // 피격 시 스프라이트가 깜빡거리는 시간
    public float knockBackSpeed;            // 플레이어가 데미지를 받았을 때 밀쳐지는 속도
    public float damagedKnockBackTime;      // 플레이어가 데미지를 받았을 때 밀쳐지는 시간
    public float bindingPosTime;            // 집중 액션 시 플레이어가 움직이지 못하는 시간
    public float etherIncreaseTime;         // 에테르가 한 사이클 차는데 걸리는 시간

    public GameObject playerHitCircle;      // 플레이어 피탄점. 각도 계산에 사용하는 자식 객체임.
    public GameObject playerAttackBox;      // 피탄점의 자식 객체로, 피탄점의 각도에 따라 원을 그리며 움직임.
    public GameObject playerSpriteAndAnimation;     // 플레이어의 스프라이트와 애니메이션 오브젝트
    
    

    public BulletEmitter be;                // 불렛이미터. 이것은 내가 공격을 할 때만 잠깐 활성화된다.
    public BulletReceiver br;               // 불렛리시버. 여기서 탄막에 피격당하는지를 검사한다.

    public Coroutine parryCoroutine;           // 패리 재사용 대기시간 코루틴 중복 방지를 위한 코루틴 변수
    private Coroutine bindingPosCoroutine;      // 집중 액션 시 플레이어가 움직이지 못하게 하는 코루틴 변수
    private Coroutine etherIncreaseByTimeCoroutine;

    private bool isMoving;                  // WASD 입력을 검사한다.
    private bool isAttack;                  // 좌클릭을 검사한다. 단 isParryAiming이 false일때만 true를 대입시킨다.
    private bool isContinueCombo;          // 공격 콤보가 이미 진행중인지를 검사한다.
    private bool isRotateBinding;             // 플레이어가 좌클릭으로 공격 중일 때, 혹은 그러지 않을 때 좌표를 원점으로 고정시킬지, 그러지 않을지를 코루틴 함수에서 검사하고 이 여부를 저장한다.
    private bool isKnockBack;               // 플레이어가 피격당해 넉백 상태인지를 검사한다.
    private bool isInvincible;              // 플레이어가 무적 상태인지를 검사한다.
    [HideInInspector] public bool isParryAiming;             // 우클릭이 눌러진 상태를 검사한다
    [HideInInspector] public bool isReadyToParry;            // 우클릭이 눌러진 상태에서 좌클릭을 눌렀을 때를 검사한다. true일 경우 패링을 시행한다.
    private bool isFocusing;                // 현재 쉬프트를 누르고 있는지를 검사한다.
    private bool isPositionBinding;         // 지금 플레이어의 움직임을 막아야 하는지를 검사한다.


    private float horizontal;
    private float vertical;
    private float currentSpeed;                 // 객체의 기본 속도를 저장
    private Animator animator;                  // 애니메이터
    private AttackState attackState;            // 공격 상태 전환 열거
    private SpriteRenderer spriteRenderer;      // 스프라이트 렌더러
    private Color currentColor;                 // 기본 컬러셋 저장
    private float etherLimit;

    // 변수 초기화
    protected override void Start()
    {
        AllInstantiate();
        base.Start();
    }

    private void Update()
    {
        AllPlayerInput();
    }
   
    private void FixedUpdate()
    {
        PlayerHitCircleRotate(GameManager.instance.worldMousePos);
        CheckingSkill1Count();
        PlayerMovingOrIdleRotate();
        CheckingPlayerFocus();
        AllPlayerMoving();
        EtherIncreaseByTime();
        horizontal = 0;
        vertical = 0;
    }

    // 이 함수는 플레이어의 변수들을 초기화한다.
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


        horizontal = 0;
        vertical = 0;
        //zamielCountInt = 0;

        currentSpeed = moveSpeed;
        isAttack = false;
        isParryAiming = false;
        isReadyToParry = false;
        isReadyToSkill1 = false;
        isContinueCombo = true;
        isRotateBinding = true;
        isKnockBack = false;
        isInvincible = false;
        parryCoroutine = null;
        bindingPosCoroutine = null;
        isPositionBinding = false;
        etherLimit = ether;
        etherIncreaseByTimeCoroutine = null;

        //isDash = false;
        
        animator = playerSpriteAndAnimation.GetComponent<Animator>();
        attackState = AttackState.UpSlash;
        spriteRenderer = playerSpriteAndAnimation.GetComponent<SpriteRenderer>();
        currentColor = spriteRenderer.color;
        be.Pause();
    }

    // 이 함수는 모든 플레이어의 Input 명령을 총괄한다.
    // 이 함수에서 플레이어가 입력한 명령은 이하 로직에서 bool 변수로 감지하고 처리할 것이다.
    private void AllPlayerInput()
    {

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            animator.SetTrigger("IdleToMove");
            isMoving = true;
        }
        else
        {
            animator.SetTrigger("MoveToIdle");
            isMoving = false;
        }

        /*
        if (Input.GetButtonDown("Jump") && !isEvade)
        {
            isEvade = true;
        }
        */
        
        // Shift를 누르면 집중 모드
        if (Input.GetButton("Fire3"))
        {
            //animator.SetTrigger("IdleToMagicMove");
            //animator.SetTrigger("MoveToMagicMove");
            isFocusing = true;
        }
        else
        {
            //animator.SetTrigger("MagicMoveToIdle");
            //animator.SetTrigger("MagicMoveToMove");
            isFocusing = false;
        }



        // 스킬 사용 부분
        if (Input.GetButtonDown("Jump") && isReadyToSkill1 && !isParryAiming)
        {
            animator.SetTrigger("Magic");
            if (bindingPosCoroutine == null)
            {
                bindingPosCoroutine = StartCoroutine(BindingPositionFocusing());
            }
            ActivatingSkill1();
        }


        /*
        // 자미엘 발사 검사
        // 패링모드중이지 않을 때는 그냥 좌클릭으로 쏘면된다.
        // !isParryAiming 추가
        if (Input.GetMouseButtonDown(0) && isReadyToSkill1 && !isParryAiming)
        {
            ActivatingSkill1();
        }
        */

        // 패링 조준 중일때는 공격을 하면 안되고 패리를 해야 한다.
        // 좌클릭 상호작용 부분
        if (Input.GetMouseButtonDown(0) && !isAttack)
        {
            /*
            // 원거리 공격
            if(isFocusing && !isParryAiming)
            {
                animator.SetTrigger("Magic");

                if (bindingPosCoroutine == null)
                {
                    bindingPosCoroutine = StartCoroutine(BindingPositionFocusing());
                }
            }
            */

            // 근접 공격
            //if (!isParryAiming && !isFocusing)
            if(!isParryAiming)
            {
                isAttack = true;
            }

            // 여기가 우클릭 눌렀을때 좌클릭 눌러서 패링 터진상태
            // 또한 쿨타임 돌아서 parryCoroutine이 null 일때만 실행함.
            else if (isParryAiming && parryCoroutine == null)
            {
                Parry();
            }
        }

        
        // 우클릭을 눌렀을 때 조준 시작
        // 여기도 마찬가지로 쿨 돌기 전까지는 true로 안바꿔줌.
        if (Input.GetMouseButton(1) && !isParryAiming && parryCoroutine == null)
        {
            isParryAiming = true;
        }
        // 우클릭을 뗐을 시 조준 비활성화
        else if (Input.GetMouseButtonUp(1) && isParryAiming)
        {
            isParryAiming = false;
        }
        
    }

    // 잠시동안 각도와, 위치를 변경하지 않게 해주는 함수
    private IEnumerator BindingPositionFocusing()
    {
        float elapsedTime = 0f;
        isPositionBinding = true;
        isRotateBinding = false;
        while (elapsedTime < bindingPosTime)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        isPositionBinding = false;
        isRotateBinding = true;
        bindingPosCoroutine = null;
    }

    // 패리 쿨타임 함수
    // 혹은 이 함수에서 다른 조건을 줘서 조건을 바꿀 수도 있다.
    private IEnumerator DisableParryTemporarily()
    {
        yield return new WaitForSeconds(5f);
        Debug.Log("패리 쿨타임 끝 5초");
        isParryAiming = false;
        parryCoroutine = null;
    }

    // 이 함수는 모든 플레이어의 움직임에 의해 검사한 bool 함수를 기반으로 실질적인 물리 함수를 처리한다.
    private void AllPlayerMoving()
    {

        // 회피
        /*
        if (isEvade)
        {
            //StartCoroutine(Evade(horizontal, vertical));
        }
        */

        // 기본 이동
        // canContinueCombo가 True인 동안 이동시키지 않는다. 
        // 넉백 상태일 때에도 이동시키지 않는다.
        // 위치 고정상태일 때에도 이동시키지 않는다.
        if (isContinueCombo && !isKnockBack && !isPositionBinding)
        {
            AttemptMove(horizontal, vertical);
        }

        // 순수 좌클릭
        if (isAttack && !isParryAiming)
        {
            Attack();
        }

    }

    protected override void AttemptMove(float xDir, float yDir)
    {
        //Dash(xDir, yDir);
        base.AttemptMove(xDir, yDir);
    }
    
    
    // 플레이어 피탄점 객체 회전
    private void PlayerHitCircleRotate(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y - transform.position.y, dir.x - transform.position.x) * Mathf.Rad2Deg;
        playerHitCircle.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }

    // 플레이어 스프라이트 객체를 WASD 입력에 따라 회전
    // 이 함수는 지금 플레이어가 이동중일때만 회전시킨다. 그렇지 않을 경우 다시 eularAngle을 원점으로 돌린다.
    // 공격 애니메이션 진행중에는 이 함수에 의해 각도를 변경시키지 않는다.
    // 집중 모드 및 패링 모드 등 집중 상태에서는 이 함수로 각도를 변경시키지 않는다.
    private void PlayerMovingOrIdleRotate()
    {
        if (isRotateBinding)
        {
            if (isMoving && !isParryAiming)
            {
                if (horizontal == 1 && vertical == 0)
                {
                    playerSpriteAndAnimation.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                }
                else if (horizontal == 1 && vertical == 1)
                {
                    playerSpriteAndAnimation.transform.eulerAngles = new Vector3(0f, 0f, 45f);
                }
                // 위
                else if (horizontal == 0 && vertical == 1)
                {
                    playerSpriteAndAnimation.transform.eulerAngles = new Vector3(0f, 0f, 90f);
                }

                // 여기서부터 왼쪽. y값을 180로 전환. 135도 지점
                else if (horizontal == -1 && vertical == 1)
                {
                    playerSpriteAndAnimation.transform.eulerAngles = new Vector3(180f, 0f, 225f);
                }
                else if (horizontal == -1 && vertical == 0)
                {
                    playerSpriteAndAnimation.transform.eulerAngles = new Vector3(180f, 0f, 180f);
                }
                else if (horizontal == -1 && vertical == -1)
                {
                    playerSpriteAndAnimation.transform.eulerAngles = new Vector3(180f, 0f, 135f);
                }

                // 다시 오른쪽. 270도 지점
                else if (horizontal == 0 && vertical == -1)
                {
                    playerSpriteAndAnimation.transform.eulerAngles = new Vector3(0f, 0f, 270f);
                }
                else if (horizontal == 1 && vertical == -1)
                {
                    playerSpriteAndAnimation.transform.eulerAngles = new Vector3(0f, 0f, 315f);
                }
            }
            else
            {
                playerSpriteAndAnimation.transform.eulerAngles = Vector3.zero;
            }
        }
    }

    // 무적 시간동안 damageColliderLaser을 비활성화하고 회피하는 방향으로 attackDashingSpeed*Time.detaTime만큼이동시킨다.
    // 방향은 키보드로 입력받는다.
    /*
    private IEnumerator Evade(float xDir, float yDir)
    {
        // 무적상태. 지금부터 아래 반복문 종료까지 레이저 무적상태 true
        Player.instance.isPlayerLaserInvincible = true;
        isEvade = true;
        float elapsedTime = 0f;

        moveSpeed = attackDashingSpeed;


        xDir = xDir * evadeDistance;
        yDir = yDir * evadeDistance;            // 다른 rigidbody나 델타타임은 다 계산해주고 있으므로 회피방향x회피거리 만 AttemptMove에 넘겨주면 됨


        while (elapsedTime < attackDashingTime)
        {
            AttemptMove(xDir, yDir);
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 레이저 무적상태 false
        isEvade = false;
        moveSpeed = currentSpeed;
        Player.instance.isPlayerLaserInvincible = false;
    }
    */


    // 공격에 딜레이를 줘서 연타 시 애니메이션이 똑바로 재생되지 않는 문제를 해결하는 함수
    // 간단하게 시간이 지나면 canContinueCombo를 true로 만들어서 조건문에서 활용하게 한다.
    private IEnumerator WaitForNextComboInput()
    {
        yield return new WaitForSeconds(attackDuration);
        isContinueCombo = true;  // 다시 콤보 입력 가능
    }

    // 플레이어의 좌표가 다시 원점으로 돌아가는 것을 방지하는 함수
    // 이 함수를 사용함으로써, 공격 사이사이에 난데없이 플레이어 객체의 각도가 Vector3.zero가 되는 것을 방지한다.
    // 이 함수에서, 좌클릭이 감지되었을 시 타이머를 초기화해서 isRotateBinding이 바로 true가 되지 않게 한다.
    // 그 시간은 attackDuration으로 한다.
    // 또한 인스펙터 창에서 애니메이션 재생 시간과 attackDuration의 시간이 얼추 맞아 떨어져야 자연스러운 게임 경험을 제공할 수 있다.
    private IEnumerator WaitingForBindingRotation()
    {
        isRotateBinding = false;
        float elaspedTime = 0f;
        while (elaspedTime < attackDuration)
        {
            if (isAttack)
            {
                elaspedTime = 0f;
            }
            else
            {
                elaspedTime += Time.fixedDeltaTime;
            }

            yield return new WaitForFixedUpdate();
        }
        isRotateBinding = true;
    }

    // 공격에 전진성을 부여하는 함수
    // 회피 로직으로 구현하였으나, 공격할 때 많이 불편할 거 같으므로 evadDistance를 attackDashingDistance로 바꾸고 인스펙터에서 조정
    private IEnumerator DashingWhileAttack()
    {
        float elapsedTime = 0f;

        moveSpeed = attackDashingSpeed;

        float xDir = 0;
        float yDir = 0;


        xDir = horizontal + attackDashingDistance * (playerAttackBox.transform.position - transform.position).normalized.x;
        yDir = vertical + attackDashingDistance * (playerAttackBox.transform.position - transform.position).normalized.y;

        RotateByAction(playerAttackBox.transform.position - transform.position);

        while (elapsedTime < attackDashingTime)
        {
            AttemptMove(xDir, yDir);
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 히트박스 인스턴스는 이동이 끝난 후 생성한다.
        /*
        playerSlashInstance.gameObject.SetActive(true);
        
        playerSlashInstance.transform.position = playerAttackBox.transform.position;
        playerSlashInstance.transform.rotation = playerHitCircle.transform.rotation;
        playerSlashInstance.attack(attackDuration);
        */

        // 이제 플레이어는 히트박스로 콜라이더에 공격하지 않고 검격을 날린다.
        StartCoroutine(PlayerSwordSlashDanmaku());

        moveSpeed = currentSpeed;
    }

    // 검격
    private IEnumerator PlayerSwordSlashDanmaku()
    {
        be.Play();
        yield return new WaitForSeconds(attackDuration);

        be.Reinitialize();
    }

    // 액션 행동 시 각도 조정 함수. 추후 리팩토링 할 때 각도 바꾸는 로직들은 다 이걸로 대체하는 편이 좋을 것 같으나, 일단 그 작업은 보류
    private void RotateByAction(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //transform.rotation = Quaternion.Euler(0, 0, angle);
        if (angle >= 90 && angle <= 180 || angle <= -90 && angle >= - 180)
        {
            playerSpriteAndAnimation.transform.rotation = Quaternion.Euler(180f, 0, -angle);
        }
        else
        {
            playerSpriteAndAnimation.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
    }

    /*
    private void CheckingZamielCount()
    {
        // 자미엘이 준비되면, bool 변수를 true로 한다.
        // 자미엘은 zamielReloadInt를 넘어서 장전될수없다.
        if (zamielCountInt >= etherSkill1Cost)
        {
            zamielCountInt = etherSkill1Cost;
            isReadyToSkill1 = true;
        }
    }*/

    // 스킬 1이 사용준비 되었는지 확인한다. 에테르가 skill1Cost보다 크거나 같으면 준비되었다.
    private void CheckingSkill1Count()
    {
        if (ether >= etherSkill1Cost)
        {
            //ether = etherSkill1Cost;
            isReadyToSkill1 = true;
        }
    }


    // 스킬1 사용 함수
    private void ActivatingSkill1()
    {
        if (ether >= etherSkill1Cost)
        {
            Debug.Log("스킬 발사");
            Vector2 direction = (playerAttackBox.transform.position - transform.position).normalized;
            RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, direction, 1000);


            // hit로 하고싶은거 로직
            foreach (RaycastHit2D takeZamiel in hit)
            {
                if (takeZamiel.transform != null && takeZamiel.transform.tag == "Enemy")
                {
                    Debug.Log(takeZamiel.transform.name);
                    EnemyTest.instance.TakeSkill();
                }
                else if (takeZamiel.transform == null)
                {
                    Debug.Log("아무것도 안맞음");
                }
            }

            ether -= etherSkill1Cost;
            isReadyToSkill1 = false;
        }

    }

    // 플레이어가 집중 상태일 때 이동속도를 저하시키고, 특정 애니메이션을 재생시킨다.
    // 집중 상태는 플레이어가 원거리 공격을 조준중이거나, 마법진을 소환할 때 두가지가 있다.
    private void CheckingPlayerFocus()
    {

        if (isParryAiming && parryCoroutine == null || isFocusing)
        {
            moveSpeed = focusSpeed;
            /*
            animator.SetTrigger("IdleToMagicMove");
            animator.SetTrigger("MoveToMagicMove");
            */
            if (isParryAiming)
            {
                animator.SetTrigger("IdleToMagicMove");
                animator.SetTrigger("MoveToMagicMove");

                playerSpriteAndAnimation.transform.eulerAngles = new Vector3(0f, 180f, 0f);
                if (playerHitCircle.transform.eulerAngles.z >= 0f && playerHitCircle.transform.eulerAngles.z < 180f)
                {
                    playerSpriteAndAnimation.transform.eulerAngles = new Vector3(0f, 180f, 0f);
                }
                else
                {
                    playerSpriteAndAnimation.transform.eulerAngles = Vector3.zero;
                }

            }
            playerHitCircle.GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            moveSpeed = currentSpeed;

            animator.SetTrigger("MagicMoveToIdle");
            animator.SetTrigger("MagicMoveToMove");

            playerHitCircle.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    // 이 함수는 각 객체에서 플레이어가 피격당했다고 판단되었을 때 호출되어지고 데미지를 받아 플레이어의 체력을 소모시킨다.
    public void PlayerDamaged(float damage)
    {
        // 무적 상태를 검사한다.
        if (isInvincible)
        {
            // empty
        }
        else
        {
            // 이곳에 피격 애니메이션 로직
            //health -= damage;

            if (ether > 0)
            {
                ether -= damage;
            }
            else
            {
                health -= 1;
            }
            StartCoroutine(DamagedBlinkBlack());
            //StartCoroutine(KnockBack(attackDashingDistance, damagedKnockBackTime));
            StartCoroutine(DamagedInvincibility());
            CheckingIfGameOver();
        }
    }

    private IEnumerator DamagedBlinkBlack()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = Color.black;    // 검은색 전환
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = currentColor;  // 원래 색상으로 복원
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    // 이 함수는 체력이 줄 때 마다 플레이어의 체력이 다 소진되었는지를 검사하고 애니메이션을 재생한 뒤 오브젝트를 비활성화시킨다.
    private void CheckingIfGameOver()
    {
        if (health == 0)
        {
            GameManager.instance.GameOver();
            gameObject.SetActive(false);
        }
    }

    // 원하는 좌표와 넉백 거리, 좌표를 받아, 그 방향과 반대인 방향으로 받은 힘만큼 객체를 이동시키고, 각도를 잠시 변화시킨다.
    public IEnumerator KnockBack(float distance, float knockBackTime)
    {

        float elapsedTime = 0f;

        isKnockBack = true;
        moveSpeed = knockBackSpeed;

        float xDir = 0;
        float yDir = 0;

        xDir = (horizontal + distance * (playerAttackBox.transform.position - transform.position).normalized.x) * -1f;
        yDir = (vertical + distance * (playerAttackBox.transform.position - transform.position).normalized.y) * -1f;

        RotateByAction(playerAttackBox.transform.position - transform.position);

        while (elapsedTime < knockBackTime)
        {
            AttemptMove(xDir, yDir);
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isKnockBack = false;
        moveSpeed = currentSpeed;
    }

    // 피격 시 isInvincible을 잠시 true로 전환한다. 시간은 invincibleTime으로 제어한다.
    // isInvincible은 PlayerDamaged 함수에서 검사받는다. (그것이 false일 때만 데미지를 계산한다)
    private IEnumerator DamagedInvincibility()
    {
        isInvincible = true;
        float elapsedTime = 0f;

        while (elapsedTime < invincibleTime)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isInvincible = false;
    }

    // 이 밑으로 에테르 관련 함수를 정의한다.
    // 에테르는 공격, 스킬, 피격 시에 소모되는 전투 자원이다. 플레이어는 에테르를 이용해 게임을 전략적으로 풀어나갈 수 있다.

    // 에테르가 시간에 따라 조금씩 차오른다.
    private void EtherIncreaseByTime()
    {
        if (etherIncreaseByTimeCoroutine == null)
        {
            etherIncreaseByTimeCoroutine = StartCoroutine(EtherIncrease());
        }
    }

    private IEnumerator EtherIncrease()
    {
        yield return new WaitForSeconds(etherIncreaseTime);
        Debug.Log("시간에 따른 에테르 증가");
        ether += 0.1f;
        etherIncreaseByTimeCoroutine = null;
    }

    // 적을 격파하면 적이 가지고 있는 에테르를 흡수한다.
    public void EtherIncreseByKillEnemy(float reward)
    {
        Debug.Log("적의 에테르 흡수");
        ether += reward;
    }


    // 공격 함수
    private void Attack()
    {
        if (ether >= etherAttackCost)
        {
            if (isContinueCombo == true)
            {
                switch (attackState)
                {
                    case AttackState.UpSlash:
                        animator.SetTrigger("Attack1");
                        attackState++;
                        break;
                    case AttackState.Downslash:
                        animator.SetTrigger("Attack2");
                        attackState = AttackState.UpSlash;
                        break;
                }
                ether -= etherAttackCost;
                isAttack = false;
                isContinueCombo = false;
                StartCoroutine(DashingWhileAttack());
                StartCoroutine(WaitForNextComboInput());
                StartCoroutine(WaitingForBindingRotation());
            }
        }
    }

    // 패리 함수
    private void Parry()
    {
        if (ether >= etherParryCost)
        {
            ether -= etherParryCost;
            isReadyToParry = true;
            parryCoroutine = StartCoroutine(DisableParryTemporarily());
            animator.SetTrigger("Magic");
            if (bindingPosCoroutine == null)
            {
                bindingPosCoroutine = StartCoroutine(BindingPositionFocusing());
            }
        }

    }
}