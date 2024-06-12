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

    private void Awake()
    {
        instance = this;
    }


    public async UniTask SpawnPlayer(int typeId)
    {
        GameObject playerPrefab = PhotonNetwork.Instantiate("Player", playerSpawnPoint[Random.Range(0, playerSpawnPoint.Length)].transform.position, Quaternion.identity);

        Player player = playerPrefab.GetComponent<Player>();

        await player.Init(typeId, PhotonNetwork.LocalPlayer.NickName);
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
