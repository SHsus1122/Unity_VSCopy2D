using Cinemachine;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유저들의 정보를 관리 및 정의한 클래스입니다.
/// </summary>
public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("# Player Info")]
    public Vector2 inputVec;
    public Vector2 resultVec;
    public Vector3 curPos;
    public Vector3 curVel;
    public ScannerEnemy scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;
    public PhotonView playerPV;
    public AchiveManager achiveManager;
    public Character character;
    public GameObject playerBorder;

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
    public Animator anim;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    float callCnt = 0;
    float callCntInterval = 0.1f;

    public void Awake()
    {
        PhotonNetwork.NickName = PhotonNetwork.LocalPlayer.NickName;

        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<ScannerEnemy>();
        hands = GetComponentsInChildren<Hand>(true);    // 인자값에 true를 넣을 시 Active상태가 아닌 오브젝트도 가져옵니다.
        achiveManager = GetComponent<AchiveManager>();
        character = GetComponent<Character>();
        PlayerManager.instance.AddPlayer(this);

        if (playerPV.IsMine)
        {
            uiHud = GameObject.Find("HUD");
            uiLevelUp = GameObject.Find("LevelUp").GetComponent<LevelUp>();
        }

        NickNameText.text = playerPV.IsMine ? PhotonNetwork.LocalPlayer.NickName : playerPV.Owner.NickName.ToString();
        NickNameText.color = playerPV.IsMine ? Color.green : Color.red;

        if (playerPV.IsMine)
        {
            // 2D 카메라
            CinemachineVirtualCamera CM = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
        }
    }


    public async UniTask Init(int playerTypeId, string owName)
    {
        uiLevelUp.player = this;
        achiveManager.uiNotice = GameManager.instance.uiNotice;
        uiHud.transform.localScale = Vector3.one;
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

        photonView.RPC("InitRPC", RpcTarget.AllBuffered, playerTypeId, owName);

        await uiLevelUp.Select(playerTypeId % 2);
    }


    [PunRPC]
    public void InitRPC(int newTypeId, string owName)
    {
        Player player = PlayerManager.instance.FindPlayer(owName);

        player.typeId = newTypeId;                             // 캐릭터 종류 ID
        player.health = PlayerManager.instance.maxHealth;      // 초기 체력 설정
        player.speed *= character.GetSpeed();                  // 캐릭터 고유 속성값 적용

        player.NickNameText.text = owName;
        player.anim.runtimeAnimatorController = player.animCon[typeId];
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
        if (!GameManager.instance.isGameLive)
            return;

        if (playerPV.IsMine)
        {
            resultVec = inputVec.normalized * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + resultVec);
        }
        else
        {
            transform.position = Vector3.Lerp(rigid.position, curPos, 10 * Time.fixedDeltaTime);
        }
    }


    // 프레임이 종료 되기 전 실행되는 생명주기 함수(즉, 업데이트가 끝나고 다음 프레임으로 넘어가기 직전에 실행)
    void LateUpdate()
    {
        if (!GameManager.instance.isGameLive)
            return;

        if (playerPV.IsMine)
        {
            achiveManager.CheckAchive(transform.GetComponent<Player>());

            callCnt += Time.deltaTime;
            if (callCnt >= callCntInterval)
            {
                callCnt = 0;
                playerPV.RPC("FlipXRPC", RpcTarget.All, resultVec.x);
            }
        }
    }


    [PunRPC]
    void FlipXRPC(float direction)
    {
        // magnitude : 백터의 크기를 가져오는 방법
        anim.SetFloat("Speed_f", resultVec.magnitude);

        // Flip 을 이용해서 Sprite를 반전 시켜 방향을 구현, inputVec의 x값이 양수냐 음수냐에 따라 방향 처리
        if (direction != 0)
            spriter.flipX = direction < 0;
    }

   
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isGameLive || !collision.gameObject.CompareTag("Enemy"))
            return;

        // 플레이어와 몬스터가 충돌하고 있는 상태면 지속적으로 체력 감소
        health -= Time.deltaTime * 10;

        if (health < 0)
        {
            for (int index = 2; index < transform.childCount; index++)
            {
                // GetChild : 해당 오브젝트의 자식 오브젝트를 반환
                transform.GetChild(index).gameObject.SetActive(false);
            }

            isPlayerLive = false;
            anim.SetTrigger("Dead_t");
            playerPV.RPC("UpdatePlayerLive", RpcTarget.All, isPlayerLive, playerPV.Owner.NickName);
            GameManager.instance.GameOver();
        }
    }


    [PunRPC]
    public void UpdatePlayerLive(bool isLive, string owName)
    {
        Player owPlayer = PlayerManager.instance.FindPlayer(owName);
        owPlayer.isPlayerLive = isLive;
        owPlayer.GetComponent<CapsuleCollider2D>().enabled = false;
        owPlayer.spriter.sortingOrder = 1;
        foreach (Transform trs in owPlayer.GetComponentsInChildren<Transform>())
        {
            if (trs.name.Contains("Hand")) trs.gameObject.SetActive(false);
            if (trs.name.Contains("Weapon")) trs.gameObject.SetActive(false);
        }
    }


    [PunRPC]
    public void UpdateKillCountRPC(int newKillCount)
    {
        kill = newKillCount;
    }


    [PunRPC]
    public void UpdateInfoRPC(int newCost, int newExp, int newLevel)
    {
        Cost = newCost;
        exp = newExp;
        level = newLevel;
    }


    // ========================================== [ 경험치 획득 ]
    [PunRPC]
    public void GetExp(string owName)
    {
        if (!isPlayerLive)
            return;

        Player owPlayer = PlayerManager.instance.FindPlayer(owName);

        owPlayer.kill++;
        owPlayer.exp++;

        if (owPlayer.exp == PlayerManager.instance.nextExp[Mathf.Min(level, PlayerManager.instance.nextExp.Length - 1)])
        {
            owPlayer.level++;     // 레벨업 적용
            owPlayer.exp = 0;     // 경험치 초기화
            owPlayer.Cost++;      // Player 레벨업 스킬 강화용 코스트 추가
            owPlayer.playerPV.RPC("UpdateInfoRPC", RpcTarget.All, Cost, exp, level);

            if (PhotonNetwork.LocalPlayer.NickName == owPlayer.playerPV.Owner.NickName)
                owPlayer.uiLevelUp.CallLevelUp();
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(health);
            stream.SendNext(kill);
            stream.SendNext(speed);
            stream.SendNext(typeId);
            stream.SendNext(level);
            stream.SendNext(exp);
            stream.SendNext(Cost);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            health = (float)stream.ReceiveNext();
            kill = (int)stream.ReceiveNext();
            speed = (float)stream.ReceiveNext();
            typeId = (int)stream.ReceiveNext();
            level = (int)stream.ReceiveNext();
            exp = (int)stream.ReceiveNext();
            Cost = (int)stream.ReceiveNext();
        }
    }
}
