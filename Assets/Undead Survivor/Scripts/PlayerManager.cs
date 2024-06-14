using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임내 유저들을 조회 및 관리를 위한 매니저 클래스입니다.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks
{
    public static PlayerManager instance;

    public float maxHealth = 100;
    public int[] nextExp = { 3, 5, 10, 100, 150, 210, 280, 360, 450, 600 };
    public Transform[] playerSpawnPoint;
    public List<Player> playerList = new List<Player>();
    public LayerMask playerLayer;
    private bool[] isSpawnPointUsed;

    private void Awake()
    {
        instance = this;
        isSpawnPointUsed = new bool[playerSpawnPoint.Length];
    }


    public async UniTask SpawnPlayer(int typeId)
    {
        int attemptCount = 0;
        int maxAttempts = 5; // 최대 시도 횟수 설정

        while (attemptCount < maxAttempts)
        {
            int point = Random.Range(0, playerSpawnPoint.Length);

            // 이미 사용된 스폰 지점인 경우 다음 지점으로 넘어감
            if (isSpawnPointUsed[point])
            {
                attemptCount++;
                Debug.Log("Same Point !!, attemptCount : " +  attemptCount);
                continue;
            }

            // 이미 다른 플레이어가 있는지 확인
            if (Physics2D.OverlapCircle(playerSpawnPoint[point].position, 0.8f, playerLayer) == null)
            {
                // 해당 지점에 다른 플레이어가 없으면 스폰 진행
                GameObject playerPrefab = PhotonNetwork.Instantiate("Player", playerSpawnPoint[point].position, Quaternion.identity);
                Player player = playerPrefab.GetComponent<Player>();

                await player.Init(typeId, PhotonNetwork.LocalPlayer.NickName);

                // 사용된 스폰 지점으로 표시
                isSpawnPointUsed[point] = true;

                return; // 스폰 완료
            }

            // 다음 시도를 위해 시도 횟수 증가
            attemptCount++;
        }

        Debug.LogWarning("플레이어를 스폰할 수 있는 적절한 위치를 찾지 못했습니다.");
    }


    public void AddPlayer(Player newPlayer)
    {
        playerList.Add(newPlayer);
    }


    public Player FindPlayer(string name)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].playerPV.Owner.NickName == name)
            {
                return playerList[i];
            }
        }
        return null;
    }
}
