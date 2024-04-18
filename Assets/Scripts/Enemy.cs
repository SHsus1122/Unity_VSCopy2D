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
    Animator anim;
    SpriteRenderer spriter;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (!isLive)
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
        if (!isLive)
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
        isLive = true;
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
}
