using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;        // 현재 체력
    public float maxHealth;     // 최대 체력
    public RuntimeAnimatorController[] animCon; // Sprite 즉, 적의 종류(타입) 변경을 위한 변수
    public Rigidbody2D target;

    bool isLive;

    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    SpriteRenderer spriter;
    WaitForFixedUpdate wait;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.isLive)
            return;

        // GetCurrentAnimatorStateInfo : 현재 상태 정보를 가져오는 함수
        // 추가 조건을 이용해서 Hit 상태일 때는 움직이는 물리력을 제거해서 밀려나도록 합니다.
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        // 방향의 크기는 1 이 아니기에 normalize 를 사용할 것입니다.
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;

        // 현재 위치에서 다음 방향으로 가야할 만큼 주면 됩니다.
        // 즉, 플레이어의 키입력 값을 더한 이동 = 몬스터의 방향 값을 더한 이동과 같습니다.
        rigid.MovePosition(rigid.position + nextVec);
        // 물리 속도가 이동에 영향을 주지 않도록 속도를 제거해주도록 합니다.(캐릭터와 충돌 시 영향을 위해서)
        rigid.velocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive || !GameManager.Instance.isLive)
            return;

        // 방향 전환, 캐릭터가 바라보는 방향으로 회전시키기 위해서 Flip 기능을 사용합니다.
        // target(플레이어) 위치의 X값과 rigid(적) X값의 크기에 따라 변동시키게 됩니다.
        spriter.flipX = target.position.x < rigid.position.x;
    }

    // 스크립트가 활성화 될 때, 호출되는 함수
    void OnEnable()
    {
        // 기존에 계층구조에서 Player에 지정하던 것이 현재는 프리펩으로 변경되었기에 이처럼 지정해줍니다.
        target = GameManager.Instance.player.GetComponent<Rigidbody2D>();
        isLive = true;                  // 생존 상태 변경
        coll.enabled = true;            // 콜라이더 비활성화
        rigid.simulated = true;         // 물리(움직임) 비활성화
        spriter.sortingOrder = 2;       // 표현 우선순위 변경
        anim.SetBool("Dead", false);    // 애니메이터 파라메터 상태 변경
        health = maxHealth;
    }

    // 생성시 초기값 초기화 함수
    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animCon[data.spriteType];  // 적 타입
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    // 적의 충돌 이벤트 관련 처리용 함수
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 사망 로직이 연달아 실행되는 것을 방지하기 위해서 !isLive 조건을 추가
        // 즉, 너무 짧은 순간에 두 번 일어나는 경우를 방지하는 것입니다.
        if (!collision.CompareTag("Bullet") || !isLive)
            return;

        health -= collision.GetComponent<Bullet>().damage;  // 데미지 처리
        StartCoroutine(KnockBack());                        // 물리력 발생 함수 호출

        if (health > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            isLive = false;             // 생존 상태 변경
            coll.enabled = false;       // 콜라이더 비활성화
            rigid.simulated = false;    // 물리(움직임) 비활성화
            spriter.sortingOrder = 1;   // 표현 우선순위 변경
            anim.SetBool("Dead", true); // 애니메이터 파라메터 상태 변경

            // 경험치 적용을 위한 코드
            GameManager.Instance.kill++;
            GameManager.Instance.GetExp();
        }
    }

    // 코루틴만의 반환형 인터페이스
    IEnumerator KnockBack() 
    {
        // null 을 리턴할 경우 1 프레임 쉬기
        yield return null;  // 다음 하나의 물리 프레임까지 기다리는 딜레이
        Vector3 playerPos = GameManager.Instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;    // 플레이어로부터 반대의 방향
        rigid.AddForce(dirVec.normalized * 3f, ForceMode2D.Impulse);    // Impulse : 즉시 발동(물리력)
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}
