using Cinemachine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("# Player Info")]
    public Vector2 inputVec;
    public Vector2 resultVec;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;
    public PhotonView playerPV;
    public AchiveManager achiveManager;
    public Character character;

    [Header("# Player Status")]
    public Text NickNameText;
    public int typeId;
    public int level;
    public int Cost = 1;
    public int kill;
    public int exp;
    public float health;
    public float speed;
    public bool isPlayerLive = true;

    [Header("# Player UI")]
    public GameObject uiHud;
    public LevelUp uiLevelUp;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;


    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);    // 인자값에 true를 넣을 시 Active상태가 아닌 오브젝트도 가져옵니다.
        achiveManager = GetComponent<AchiveManager>();
        character = GetComponent<Character>();
        uiHud = GameObject.Find("HUD");
        uiLevelUp = GameObject.Find("LevelUp").GetComponent<LevelUp>();
        PlayerManager.instance.AddPlayer(this);

        NickNameText.text = playerPV.IsMine ? PhotonNetwork.NickName.ToString() : playerPV.Owner.NickName.ToString();
        NickNameText.color = playerPV.IsMine ? Color.green : Color.red;

        if (playerPV.IsMine)
        {
            // 2D 카메라
            CinemachineVirtualCamera CM = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;
        }
    }

    [PunRPC]
    public void Init(int playerTypeId)
    {
        //transform.name = "Player " + playerPV.OwnerActorNr;
        typeId = playerTypeId;                          // 캐릭터 종류 ID
        health = PlayerManager.instance.maxHealth;      // 초기 체력 설정
        speed *= character.GetSpeed();                  // 캐릭터 고유 속성값 적용
        anim.runtimeAnimatorController = animCon[typeId];

        achiveManager.uiNotice = GameManager.instance.uiNotice;
        uiHud.transform.localScale = Vector3.one;
        uiLevelUp.player = this;
        uiLevelUp.Show();

        Transform[] hudArr = uiHud.GetComponentsInChildren<Transform>(true);
        foreach (Transform Child in hudArr)
        {
            if (Child.GetComponent<HUD>())
                Child.GetComponent<HUD>().player = this;

            if (Child.GetComponent<Follow>())
                Child.GetComponent<Follow>().player = this;
        }

        Transform[] levelUpArr = uiLevelUp.GetComponentsInChildren<Transform>();
        foreach (Transform Child in levelUpArr)
        {
            if (Child.GetComponent<Item>())
                Child.GetComponent<Item>().player = this;
        }

        photonView.RPC("InitRPC", RpcTarget.All, NickNameText.text.ToString(), typeId, health, speed);
        uiLevelUp.Select(playerTypeId % 2);
    }


    [PunRPC]
    public void InitRPC(string owName, int newTypeId, float newHealth, float newSpeed)
    {
        Player player = PlayerManager.instance.FindPlayer(owName);
        player.NickNameText.text = owName;
        player.typeId = newTypeId;
        player.health = newHealth;
        player.speed = newSpeed;
    }


    void Update()
    {
        if (!GameManager.instance.isGameLive)
            return;

        // 예전 방식의 컨트롤러 적용법 코드
        // Input.GetAxis 의 경우 보정이 들어가 있어서 부드럽게 움직임이 멈추게 됩니다.
        // 하지만 GetAxisRaw 의 경우에는 부드럽게 멈추는 것이 아닌 그 자리에 바로 멈추게끔 됩니다.
        if (playerPV.IsMine)
        {
            inputVec.x = Input.GetAxisRaw("Horizontal");
            inputVec.y = Input.GetAxisRaw("Vertical");
        }
    }



    // 물리 관련한 처리를 위해서는 FixedUpdate 를 사용
    private void FixedUpdate()
    {
        /*
         * 이동 구현의 세 가지 방법 중 2개
        // 물리력(힘) 작용
        rigid.AddForce(inputVec);

        // 속도 제어
        rigid.velocity = inputVec;
        */
        if (!GameManager.instance.isGameLive)
            return;

        // normalized 를 통해서 어떠한 방향으로 나아가도 벡터의 크기가 1이 되도록 수정, deltaTime 을 사용해서 프레임에 따른 차이를 방지
        // Time.deltaTime : 물리 프레임 하나가 소비한 시간
        //resultVec = inputVec.normalized * speed * Time.deltaTime;

        if (playerPV.IsMine)
        {
            // InputSystem 사용시 에디터에서 normalized를 사용하기에 위의 이전 코드처럼 normalized를 추가할 필요가 없습니다.
            //resultVec = inputVec * speed * Time.deltaTime;

            resultVec = inputVec.normalized * speed * Time.deltaTime;

            // 위치 이동(World 기준의 위치), 현재 위치를 기준으로 나아갈 방향(nextVec 활용)
            rigid.MovePosition(rigid.position + resultVec);
        }
    }


    /*
    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }*/


    // 프레임이 종료 되기 전 실행되는 생명주기 함수(즉, 업데이트가 끝나고 다음 프레임으로 넘어가기 직전에 실행)
    void LateUpdate()
    {
        if (!GameManager.instance.isGameLive)
            return;

        achiveManager.CheckAchive(transform.GetComponent<Player>());

        playerPV.RPC("FlipXRPC", RpcTarget.AllBuffered);
    }


    [PunRPC]
    void FlipXRPC()
    {
        // magnitude : 백터의 크기를 가져오는 방법
        anim.SetFloat("Speed_f", resultVec.magnitude);

        // Flip 을 이용해서 Sprite를 반전 시켜 방향을 구현, inputVec의 x값이 양수냐 음수냐에 따라 방향 처리
        if (resultVec.x != 0)
            spriter.flipX = resultVec.x < 0;

        /*// 키 입력에 따라 캐릭터의 회전 방향을 처리
        if (resultVec.x != 0)
        {
            // Flip 을 이용해서 Sprite를 반전 시켜 방향을 구현, inputVec의 x값이 양수냐 음수냐에 따라 방향 처리
            spriter.flipX = resultVec.x < 0;
        }*/
    }


    // 플레이어와 몬스터가 충돌하고 있는 상태면 지속적으로 체력 감소
    private void OnCollisionStay2D(Collision2D collision)
    {
        //if (!GameManager.Instance.isLive)
        return;

        health -= Time.deltaTime * 10;

        if (health < 0)
        {
            for (int index = 2; index < transform.childCount; index++)
            {
                // GetChild : 해당 오브젝트의 자식 오브젝트를 반환
                transform.GetChild(index).gameObject.SetActive(false);
            }

            anim.SetTrigger("Dead_t");
            GameManager.instance.GameOver();
        }
    }


    // ========================================== [ 경험치 획득 ]
    public void GetExp(Player player)
    {
        if (!player.isPlayerLive)
            return;

        player.exp++;

        // Mathf.Min(level, nextExp.Length - 1) 를 통해서 에러방지(초과) 및 마지막 레벨만 나오게 합니다.
        if (player.exp == PlayerManager.instance.nextExp[Mathf.Min(player.level, PlayerManager.instance.nextExp.Length - 1)])
        {
            player.level++;     // 레벨업 적용
            player.exp = 0;     // 경험치 초기화
            player.Cost++;      // Player 레벨업 스킬 강화용 코스트 추가
            uiLevelUp.CallLevelUp();
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(new Vector2(transform.position.x, transform.position.y));
            stream.SendNext(health);
            stream.SendNext(kill);
        }
        else
        {
            resultVec = (Vector2)stream.ReceiveNext();
            health = (float)stream.ReceiveNext();
            kill = (int)stream.ReceiveNext();
        }
    }
}
