using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager instance;

    // Header 인스펙터의 속성들을 구분시켜주는 타이틀
    [Header("# Game Control")]
    public bool isGameLive;
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
    public Result uiResult;
    public Transform uiGameStart;
    public Transform uiJoy;
    public GameObject uiNotice;



    private void Awake()
    {
        // 생명 주기에서 인스턴스 변수를 자기 자신으로 초기화 
        instance = this;
        Application.targetFrameRate = 60;   // 게임 프레임 강제 지정
    }



    // ========================================== [ 게임 시작 ]
    public void GameStart(int id)
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal && !PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.SerializationRate = 60;   // OnPhotonSerializeView 호출 빈도
        PhotonNetwork.SendRate = 60;            // RPC 원격 프로시저 호출 빈도 // 단발성 원할 때 한번

        pool.gameObject.SetActive(true);
        spawner.SetActive(true);
        if (!PhotonNetwork.IsMasterClient)
        {
            spawner = GameObject.FindWithTag("Spawner");
        }

        PlayerManager.instance.SpawnPlayer(id);

        uiGameStart.localScale = Vector3.zero;
        gameManagerPV.RPC("Resume", RpcTarget.All);

        AudioManager.instance.PlayBgm(true);                    // 게임 배경음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select); // 클릭 효과음 재생
    }



    // ========================================== [ 게임 종료 ]
    IEnumerator GameOverRoutine()       // 코루틴 활용
    {
        isGameLive = false;

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
        isGameLive = false;
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
        if (!isGameLive)
            return;

        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }
    }



    // ========================================== [ 게임 정지, 시작 ]

    [PunRPC]
    public void Stop()
    {
        isGameLive = false;
        Time.timeScale = 0; // 유니티의 시간 속도(배율)
        //uiJoy.localScale = Vector3.zero;
    }

    [PunRPC]
    public void Resume()
    {
        Debug.Log("=============== Call Resume ===============");
        isGameLive = true;
        Time.timeScale = 1;
        //uiJoy.localScale = Vector3.one;
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
