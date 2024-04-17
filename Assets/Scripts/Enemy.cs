using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public Rigidbody2D target;

    bool isLive = true;

    Rigidbody2D rigid;
    SpriteRenderer spriter;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
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
}
