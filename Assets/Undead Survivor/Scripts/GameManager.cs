using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager Instance;

    // Header 인스펙터의 속성들을 구분시켜주는 타이틀
    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 2 * 10f; // 20초
    public PhotonView gameManagerPV;
    public int num = 0;

    [Header("# Player Info")]
    public int playerId;
    public float health;
    public float maxHealth = 100;
    public int Playerlevel;
    public int kill;
    public int exp;
    public int[] nextExp = { 3, 5, 10, 100, 150, 210, 280, 360, 450, 600 };

    [Header("# Game Object")]
    public PoolManager pool;
    public Player player;
    public List<Player> playerList = new List<Player>();
    public LevelUp uiLevelUp;
    public Result uiResult;
    public Transform uiGameStart;
    public Transform uiJoy;
    public GameObject enemyCleaner;
    public Transform spawnPoint;
    public GameObject spawner;
    public GameObject poolManager;

    void Awake()
    {
        // 생명 주기에서 인스턴스 변수를 자기 자신으로 초기화 
        Instance = this;
        Application.targetFrameRate = 60;   // 게임 프레임 강제 지정
    }



    // ========================================== [ 게임 시작 ]
    //public void GameStart(int id)
    //{
    //    gameManagerPV.RPC("GameStartRPC", RpcTarget.All, id);
    //}

    //[PunRPC]
    public void GameStart(int id)
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal && !PhotonNetwork.IsMasterClient)
            return;

        playerId = id;                      // 캐릭터 종류 ID
        health = maxHealth;                 // 초기 체력 설정

        //GameObject playerPrefab = PhotonNetwork.Instantiate("Player", spawnPoint.transform.position, Quaternion.identity);
        //player = playerPrefab.GetComponent<Player>();
        //player.transform.name = "Player" + player.playerPV.OwnerActorNr;
        playerList.Add(player);

        poolManager.SetActive(true);
        spawner.SetActive(true);

        uiLevelUp.Show();
        uiLevelUp.Select(playerId % 2);     // 기존 무기 지급을 위한 함수 호출 -> 캐릭터 ID로 변경
        uiGameStart.localScale = Vector3.zero;
        gameManagerPV.RPC("Resume", RpcTarget.All);

        AudioManager.instance.PlayBgm(true);                    // 게임 배경음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select); // 클릭 효과음 재생
    }



    // ========================================== [ 게임 종료 ]
    IEnumerator GameOverRoutine()       // 코루틴 활용
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        // UI 활성화 및 패배 UI 표시
        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        AudioManager.instance.PlayBgm(false);                   // 게임 배경음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);   // 패배 효과음 재생
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }



    // ========================================== [ 게임 승리 ]
    IEnumerator GameVictoryRoutine()    // 코루틴 활용
    {
        isLive = false;
        enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // UI 활성화 및 승리 UI 표시
        uiResult.gameObject.SetActive(true);
        uiResult.Win();
        Stop();

        AudioManager.instance.PlayBgm(false);                  // 게임 배경음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);   // 승리 효과음 재생
    }

    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }



    // ========================================== [ 게임 재시작 ]
    public void GaemRetry()
    {
        // 씬의 이름을 넣거나 순번을 넣을 수 있습니다.
        SceneManager.LoadScene(0);
    }



    // ========================================== [ 게임 종료 ]
    public void GaemQuit()
    {
        Application.Quit();
    }

    void Update()
    {
        if (!isLive)
            return;

        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }
    }



    // ========================================== [ 경험치 획득 ]
    public void GetExp()
    {
        if (!isLive)
            return;

        exp++;

        // Mathf.Min(level, nextExp.Length - 1) 를 통해서 에러방지(초과) 및 마지막 레벨만 나오게 합니다.
        if (exp == nextExp[Mathf.Min(Playerlevel, nextExp.Length - 1)])
        {
            Playerlevel++;      // 레벨업 적용
            exp = 0;            // 경험치 초기화
            player.Cost++;      // Player 레벨업 스킬 강화용 코스트 추가
            uiLevelUp.CallLevelUp();
        }
    }



    // ========================================== [ 게임 정지, 시작 ]
    [PunRPC]
    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0; // 유니티의 시간 속도(배율)
        //uiJoy.localScale = Vector3.zero;
    }

    [PunRPC]
    public void Resume()
    {
        Debug.Log("=============== Call Resume ===============");
        isLive = true;
        Time.timeScale = 1;
        //uiJoy.localScale = Vector3.one;
    }



    // ========================================== [ 네트워크 상태 동기화 함수 ]
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isLive);
        }
        else
        {
            isLive = (bool)stream.ReceiveNext();
        }
    }
}
