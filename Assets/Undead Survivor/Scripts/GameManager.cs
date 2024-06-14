using Cinemachine;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 게임의 전반적인 상태를 관리하는 매니저 클래스입니다.
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager instance;

    // Header 인스펙터의 속성들을 구분시켜주는 타이틀
    [Header("# Game Control")]
    public bool isGameLive;
    public double gameStartTime;
    public float gameTime;
    public float maxGameTime = 2 * 10f; // 20초
    public int num = 0;
    public PhotonView gameManagerPV;

    [Header("# Game Object")]
    public PoolManager pool;
    public Transform spawnPoint;
    public GameObject enemyCleaner;
    public GameObject spawner;
    public GameObject CharacterGroup;

    [Header("# Game UI")]
    public Transform uiResult;
    public Transform uiGameStart;
    public Transform uiJoy;
    public GameObject uiNotice;
    public LevelUp uiLevelup;


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;  // 씬 로드 이벤트 등록
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  // 씬 로드 이벤트 해제
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "UndeadSurvivarGame")     // 레벨 이름 검증(조건식)
        {
            gameStartTime = PhotonNetwork.Time;
            int playerType = (int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerType"];
            await GameStart(playerType);
        }
        if (scene.name == "UndeadSurvivar" && PhotonNetwork.InRoom)
        {
            gameStartTime = PhotonNetwork.Time;
            GameObject.Find("NetworkManager").GetComponent<NetworkManager>().ReJoinRoom(PhotonNetwork.CurrentRoom.Name);
        }
    }


    private void Awake()
    {
        // 생명 주기에서 인스턴스 변수를 자기 자신으로 초기화 
        instance = this;
        Application.targetFrameRate = 60;   // 게임 프레임 강제 지정
    }


    void Update()
    {
        if (!isGameLive)
            return;

        gameTime = (float)(PhotonNetwork.Time - gameStartTime);

        if (gameTime > maxGameTime)
        {
            GameVictory();
        }
    }


    // ========================================== [ 게임 시작 ]
    public async UniTask GameStart(int id)
    {
        pool.gameObject.SetActive(true);
        spawner.SetActive(true);
        if (!PhotonNetwork.IsMasterClient)
        {
            spawner = GameObject.FindWithTag("Spawner");
        }

        await PlayerManager.instance.SpawnPlayer(id);

        uiGameStart.localScale = Vector3.zero;
        gameManagerPV.RPC("Resume", RpcTarget.All);

        AudioManager.instance.PlayBgm(true);                    // 게임 배경음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select); // 클릭 효과음 재생
    }


    // ========================================== [ 게임 종료 ]
    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }


    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        int cnt = 0;
        foreach (Player pls in PlayerManager.instance.playerList)
        {
            if (!pls.isPlayerLive)
            {
                cnt++;
                if (PhotonNetwork.LocalPlayer.NickName == pls.playerPV.Owner.NickName)
                {
                    pls.uiLevelUp.Hide();
                }
            }
        }

        // 사망시 다른 유저의 플레이 화면을 보게되며 전원 사망시 게임오버 함수를 호출합니다.
        if (cnt != PlayerManager.instance.playerList.Count)
        {
            foreach (Player pls in PlayerManager.instance.playerList)
            {
                if (pls.isPlayerLive)
                {
                    CinemachineVirtualCamera CM = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
                    CM.Follow = pls.transform;
                }
            }
            yield break;
        } 
        else if (cnt == PlayerManager.instance.playerList.Count)
        {
            gameManagerPV.RPC("GameOverRPC", RpcTarget.All);
        }
    }


    [PunRPC]
    public void GameOverRPC()
    {
        //GameObject.Find("AchiveManager").GetComponent<AchiveManager>().UnlockCharacter();

        isGameLive = false;

        // UI 활성화 및 패배 UI 표시
        uiResult.gameObject.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
        {
            uiResult.GetChild(2).gameObject.SetActive(true);
        }
        uiResult.GetChild(0).gameObject.SetActive(true);
        uiResult.GetChild(1).gameObject.SetActive(false);
        gameManagerPV.RPC("Stop", RpcTarget.All);

        AudioManager.instance.PlayBgm(false);                   // 게임 배경음 정지
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);   // 패배 효과음 재생
    }


    // ========================================== [ 게임 승리 ]
    public void GameVictory()
    {
        gameManagerPV.RPC("GameVictoryRPC", RpcTarget.AllBuffered);
    }


    IEnumerator GameVictoryRoutine()
    {
        isGameLive = false;
        enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // UI 활성화 및 승리 UI 표시
        uiResult.gameObject.SetActive(true);
        uiLevelup.Hide();
        if (PhotonNetwork.IsMasterClient)
        {
            uiResult.GetChild(2).gameObject.SetActive(true);
        }
        uiResult.GetChild(0).gameObject.SetActive(false);
        uiResult.GetChild(1).gameObject.SetActive(true);
        gameManagerPV.RPC("Stop", RpcTarget.All);

        AudioManager.instance.PlayBgm(false);                  // 게임 배경음 정지
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);   // 승리 효과음 재생
    }


    [PunRPC]
    public void GameVictoryRPC()
    {
        //GameObject.Find("AchiveManager").GetComponent<AchiveManager>().UnlockCharacter();
        StartCoroutine(GameVictoryRoutine());
    }


    // ========================================== [ 게임 재시작 ]
    public void GameRetry()
    {
        gameManagerPV.RPC("AllDestroyObj", RpcTarget.All);
        PhotonNetwork.DestroyAll();

        PlayerPrefs.DeleteKey("PlayerType");

        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        customProperties["ReadyCount"] = 0;
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

        PhotonNetwork.LoadLevel(0);
    }


    [PunRPC]
    public void AllDestroyObj()
    {
        for (int i = 0; i < pool.pools.Length; i++)
        {
            pool.pools[i].Clear();
        }

        PlayerManager.instance.playerList.Clear();

        GameObject[] list1 = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] list2 = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject obj in list1) Destroy(obj);
        foreach (GameObject obj in list2) Destroy(obj);
    }


    // ========================================== [ 어플 종료 ]
    public void GaemQuit()
    {
        Application.Quit();
    }


    // ========================================== [ 게임 정지, 시작 ]
    [PunRPC]
    public void Stop()
    {
        isGameLive = false;
        //Time.timeScale = 0; // 유니티의 시간 속도(배율)
    }


    [PunRPC]
    public void Resume()
    {
        isGameLive = true;
        //Time.timeScale = 1;
    }


    // ========================================== [ 네트워크 상태 동기화 함수 ]
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isGameLive);
        }
        else
        {
            isGameLive = (bool)stream.ReceiveNext();
        }
    }
}
