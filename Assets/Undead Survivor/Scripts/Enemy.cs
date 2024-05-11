﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviourPunCallbacks, IPunObservable
{
    public float enemySpeed;
    public float enemyHealth;        // 현재 체력
    public float enemyMaxHealth;     // 최대 체력
    public RuntimeAnimatorController[] animCon; // Sprite 즉, 적의 종류(타입) 변경을 위한 변수
    public Rigidbody2D target;
    public PhotonView enemyPV;
    public ScannerPlayer scanner;
    public SpriteRenderer spriter;
    public bool isLive;

    float timer;

    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    Vector3 nowPos;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        scanner = GetComponent<ScannerPlayer>();
    }

    private void Start()
    {
        isLive = true;                  // 생존 상태 변경
        coll.enabled = true;            // 콜라이더 비활성화
        rigid.simulated = true;         // 물리(움직임) 비활성화
        spriter.sortingOrder = 2;       // 표현 우선순위 변경
        anim.SetBool("Dead", false);    // 애니메이터 파라메터 상태 변경
        enemyHealth = enemyMaxHealth;

        SetParent();
    }

    void SetParent()
    {
        this.transform.parent = PoolManager.instance.transform;
    }

    #region Update 관련 로직
    private void Update()
    {
        if (isLive && timer > 0.2f)
        {
            target = scanner.nearestTarget.GetComponent<Rigidbody2D>();
            timer = 0;
        }
        timer += Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isGameLive)
            return;

        // GetCurrentAnimatorStateInfo : 현재 상태 정보를 가져오는 함수
        // 추가 조건을 이용해서 Hit 상태일 때는 움직이는 물리력을 제거해서 밀려나도록 합니다.
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit") || target == null)
            return;

        // 방향의 크기는 1 이 아니기에 normalize 를 사용할 것입니다.
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * enemySpeed * Time.fixedDeltaTime;

        // 현재 위치에서 다음 방향으로 가야할 만큼 주면 됩니다.
        // 즉, 플레이어의 키입력 값을 더한 이동 = 몬스터의 방향 값을 더한 이동과 같습니다.
        rigid.MovePosition(rigid.position + nextVec);
        // 물리 속도가 이동에 영향을 주지 않도록 속도를 제거해주도록 합니다.(캐릭터와 충돌 시 영향을 위해서)
        rigid.velocity = Vector2.zero;

        nowPos = rigid.position;

        if (!enemyPV.IsMine)
            transform.position = Vector3.Lerp(transform.position, nowPos, 10 * Time.deltaTime);
    }

/*    [PunRPC]
    void UpdatePosRPC()
    {
        // GetCurrentAnimatorStateInfo : 현재 상태 정보를 가져오는 함수
        // 추가 조건을 이용해서 Hit 상태일 때는 움직이는 물리력을 제거해서 밀려나도록 합니다.
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit") || target == null)
            return;

        // 방향의 크기는 1 이 아니기에 normalize 를 사용할 것입니다.
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * enemySpeed * Time.fixedDeltaTime;

        // 현재 위치에서 다음 방향으로 가야할 만큼 주면 됩니다.
        // 즉, 플레이어의 키입력 값을 더한 이동 = 몬스터의 방향 값을 더한 이동과 같습니다.
        rigid.MovePosition(rigid.position + nextVec);
        // 물리 속도가 이동에 영향을 주지 않도록 속도를 제거해주도록 합니다.(캐릭터와 충돌 시 영향을 위해서)
        rigid.velocity = Vector2.zero;

        nowPos = rigid.position;

        if (!enemyPV.IsMine)
            transform.position = Vector3.Lerp(transform.position, nowPos, 10 * Time.deltaTime);
    }*/

    void LateUpdate()
    {
        if (!isLive || !GameManager.instance.isGameLive)
            return;

        if (target != null)
            spriter.flipX = target.position.x < rigid.position.x;
    }

/*    [PunRPC]
    void UpdateFlipXRPC()
    {
        // 방향 전환, 캐릭터가 바라보는 방향으로 회전시키기 위해서 Flip 기능을 사용합니다.
        // target(플레이어) 위치의 X값과 rigid(적) X값의 크기에 따라 변동시키게 됩니다.
        if (target != null)
            spriter.flipX = target.position.x < rigid.position.x;
    }*/
    #endregion

    [PunRPC]
    public void InitRPC(float spawnTim, int spriteType, int health, float speed)
    {
        anim.runtimeAnimatorController = animCon[spriteType];  // 적 타입
        enemySpeed = speed;
        enemyMaxHealth = health;
        enemyHealth = health;
    }

    //// 적의 충돌 이벤트 관련 처리용 함수
    //[PunRPC]
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    // 사망 로직이 연달아 실행되는 것을 방지하기 위해서 !isLive 조건을 추가
    //    // 즉, 너무 짧은 순간에 두 번 일어나는 경우를 방지하는 것입니다.
    //    if (!collision.CompareTag("Bullet") || !isLive)
    //        return;

    //    enemyHealth -= collision.GetComponent<Bullet>().damage; // 데미지 처리

    //    StartCoroutine(KnockBack(collision));                   // 물리력 발생 함수 호출

    //    if (enemyHealth > 0)
    //    {
    //        anim.SetTrigger("Hit");
    //        AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);   // 피격 효과음 재생
    //    }
    //    else
    //    {
    //        isLive = false;             // 생존 상태 변경
    //        coll.enabled = false;       // 콜라이더 비활성화
    //        rigid.simulated = false;    // 물리(움직임) 비활성화
    //        spriter.sortingOrder = 1;   // 표현 우선순위 변경
    //        anim.SetBool("Dead", true); // 애니메이터 파라메터 상태 변경

    //        // 경험치 적용을 위한 코드
    //        collision.GetComponentInParent<Player>().kill++;
    //        collision.GetComponentInParent<Player>().GetExp(collision.GetComponentInParent<Player>());

    //        // 분기문으로 플레이어가 생존함에 따라 최종 결과에서 몬스터들이 전부 사망시 다량의 사망 효과음 재생을 방지합니다.
    //        if (collision.GetComponentInParent<Player>().isPlayerLive)
    //            AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);   // 사망 효과음 재생
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 사망 로직이 연달아 실행되는 것을 방지하기 위해서 !isLive 조건을 추가
        // 즉, 너무 짧은 순간에 두 번 일어나는 경우를 방지하는 것입니다.
        if (!collision.CompareTag("Bullet") || !isLive)
            return;

        //Debug.Log("[ Enemy ] TriggerEvent Damage ViewID : " + collision.gameObject.GetPhotonView().ViewID);
        //enemyPV.RPC("TriggerEventRPC", RpcTarget.All, collision.gameObject.GetPhotonView().ViewID);
        //StartCoroutine(KnockBack(collision));


        enemyHealth -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack(collision));

        if (enemyHealth > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);   // 피격 효과음 재생
        }
        else
        {
            //isLive = false;             // 생존 상태 변경
            //coll.enabled = false;       // 콜라이더 비활성화
            //rigid.simulated = false;    // 물리(움직임) 비활성화
            //spriter.sortingOrder = 1;   // 표현 우선순위 변경
            //anim.SetBool("Dead", true); // 애니메이터 파라메터 상태 변경
            enemyPV.RPC("Dead", RpcTarget.All);

            // 경험치 적용을 위한 코드
            collision.GetComponentInParent<Player>().kill++;
            collision.GetComponentInParent<Player>().GetExp(collision.GetComponentInParent<Player>());

            // 분기문으로 플레이어가 생존함에 따라 최종 결과에서 몬스터들이 전부 사망시 다량의 사망 효과음 재생을 방지합니다.
            if (collision.GetComponentInParent<Player>().isPlayerLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);   // 사망 효과음 재생
        }
    }

    [PunRPC]
    void TriggerEventRPC(int senderViewid)
    {
        Collider2D sender = PhotonView.Find(senderViewid).transform.GetComponent<Collider2D>();
        Debug.Log("[ Enemy ] TriggerEvent Sender Name : " + sender.name);

        enemyHealth -= sender.GetComponent<Bullet>().damage;
        //StartCoroutine(KnockBack(sender));

        if (enemyHealth > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);   // 피격 효과음 재생
        }
        else
        {
            isLive = false;             // 생존 상태 변경
            coll.enabled = false;       // 콜라이더 비활성화
            rigid.simulated = false;    // 물리(움직임) 비활성화
            spriter.sortingOrder = 1;   // 표현 우선순위 변경
            anim.SetBool("Dead", true); // 애니메이터 파라메터 상태 변경

            // 경험치 적용을 위한 코드
            sender.GetComponentInParent<Player>().kill++;
            sender.GetComponentInParent<Player>().GetExp(sender.GetComponentInParent<Player>());

            // 분기문으로 플레이어가 생존함에 따라 최종 결과에서 몬스터들이 전부 사망시 다량의 사망 효과음 재생을 방지합니다.
            if (sender.GetComponentInParent<Player>().isPlayerLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);   // 사망 효과음 재생
        }
    }


    IEnumerator KnockBack(Collider2D collision)
    {
        // null 을 리턴할 경우 1 프레임 쉬기
        yield return null;  // 다음 하나의 물리 프레임까지 기다리는 딜레이
        Debug.Log("[ Enemy ] KnockBack Collision Name: " + collision.name);
        Vector3 playerPos = collision.GetComponentInParent<Player>().transform.position;
        Vector3 dirVec = transform.position - playerPos;                // 플레이어로부터 반대의 방향
        rigid.AddForce(dirVec.normalized * 3f, ForceMode2D.Impulse);    // Impulse : 즉시 발동(물리력)
    }



    [PunRPC]
    void Dead()
    {
        isLive = false;             // 생존 상태 변경
        coll.enabled = false;       // 콜라이더 비활성화
        rigid.simulated = false;    // 물리(움직임) 비활성화
        spriter.sortingOrder = 1;   // 표현 우선순위 변경
        anim.SetBool("Dead", true); // 애니메이터 파라메터 상태 변경
    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(enemySpeed);
            stream.SendNext(enemyHealth);
            stream.SendNext(enemyMaxHealth);
            stream.SendNext(isLive);
        }
        else
        {
            nowPos = (Vector3)stream.ReceiveNext();
            enemySpeed = (float)stream.ReceiveNext();
            enemyHealth = (float)stream.ReceiveNext();
            enemyMaxHealth = (float)stream.ReceiveNext();
            isLive = (bool)stream.ReceiveNext();
        }
    }
}
