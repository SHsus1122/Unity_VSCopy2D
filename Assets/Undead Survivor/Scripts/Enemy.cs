using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviourPunCallbacks, IPunObservable
{
    public float enemySpeed;
    public float originSpeed;
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
        this.transform.parent = GameManager.instance.pool.transform;
    }


    private void Update()
    {
        if (!GameManager.instance.isGameLive)
            return;

        if (isLive && timer > 0.2f)
        {
            target = scanner.nearestTarget.GetComponent<Rigidbody2D>();
            Player targetPlayer = PlayerManager.instance.FindPlayer(target.GetComponent<Player>().photonView.Owner.NickName);
            Vector3 viewPos = targetPlayer.camera.WorldToViewportPoint(transform.position);

            if (target != null)
            {
                if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1 || viewPos.z < 0)
                {
                    enemySpeed = originSpeed * 2;
                }
                else
                {
                    enemySpeed = originSpeed;
                }
            }

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


    void LateUpdate()
    {
        if (!isLive || !GameManager.instance.isGameLive)
            return;

        if (target != null)
            spriter.flipX = target.position.x < rigid.position.x;
    }


    public void Init(float spawnTime, int spriteType, int health, float speed)
    {
        anim.runtimeAnimatorController = animCon[spriteType];  // 적 타입
        enemySpeed = speed;
        originSpeed = enemySpeed;
        enemyMaxHealth = health;
        enemyHealth = health;

        enemyPV.RPC("InitRPC", RpcTarget.Others, spriteType);
    }

    [PunRPC]
    public void InitRPC(int spriteType)
    {
        anim.runtimeAnimatorController = animCon[spriteType];  // 적 타입
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyCleaner"))
        {
            enemyHealth = 0;
            enemyPV.RPC("Dead", RpcTarget.All);
            return;
        }

        // 사망 로직이 연달아 실행되는 것을 방지하기 위해서 !isLive 조건을 추가
        // 즉, 너무 짧은 순간에 두 번 일어나는 경우를 방지하는 것입니다.
        if (!collision.CompareTag("Bullet") || !isLive)
            return;

        enemyHealth -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack(collision));

        if (enemyHealth > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);   // 피격 효과음 재생
        }
        else
        {
            enemyPV.RPC("Dead", RpcTarget.All);

            Player owPlayer = collision.GetComponentInParent<Player>();

            // 경험치 적용을 위한 코드
            owPlayer.kill++;
            owPlayer.GetExp(collision.GetComponentInParent<Player>());

            // 분기문으로 플레이어가 생존함에 따라 최종 결과에서 몬스터들이 전부 사망시 다량의 사망 효과음 재생을 방지합니다.
            if (collision.GetComponentInParent<Player>().isPlayerLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);   // 사망 효과음 재생
        }
    }


    IEnumerator KnockBack(Collider2D collision)
    {
        // null 을 리턴할 경우 1 프레임 쉬기
        yield return null;  // 다음 하나의 물리 프레임까지 기다리는 딜레이
        //Debug.Log("[ Enemy ] KnockBack Collision Name: " + collision.name);
        Vector3 playerPos = collision.GetComponentInParent<Player>().transform.position;
        Vector3 dirVec = transform.position - playerPos;                // 플레이어로부터 반대의 방향
        rigid.AddForce(dirVec.normalized * 3f, ForceMode2D.Impulse);    // Impulse : 즉시 발동(물리력)
    }


    public IEnumerator ReActive(int typeId, int viewId)
    {
        yield return new WaitForSeconds(30f);

        if (!GameManager.instance.isGameLive)
            yield break;

        GameObject obj = GameManager.instance.pool.FindPoolObj(typeId, viewId);

        if (!obj)
            yield break;

        if (obj.gameObject.activeSelf && !obj.GetComponent<Enemy>().isLive)
        {
            obj.GetPhotonView().RPC("ResetAnim", RpcTarget.AllBuffered, typeId, viewId);
            GameManager.instance.pool.poolPV.RPC("ObjActiveToggle", RpcTarget.AllBuffered, typeId, viewId, false);
        }
        else if (obj.gameObject.activeSelf && obj.GetComponent<Enemy>().isLive)
        {
            StartCoroutine(ReActive(typeId, viewId));
        }
    }


    [PunRPC]
    void ResetAnim(int typeId, int viewId)
    {
        GameObject obj = GameManager.instance.pool.FindPoolObj(typeId, viewId);
        obj.GetComponent<Enemy>().anim.SetTrigger("Dead");
        obj.GetComponent<Enemy>().spriter.sortingOrder = 2;
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
