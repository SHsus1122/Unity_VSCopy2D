using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void SpawnPlayer(int typeId)
    {
        GameObject playerPrefab = PhotonNetwork.Instantiate("Player", playerSpawnPoint[Random.Range(0, playerSpawnPoint.Length)].transform.position, Quaternion.identity);
        Player player = playerPrefab.GetComponent<Player>();
        AddPlayer(player);
        player.Init(typeId);
    }

    public void AddPlayer(Player newPlayer)
    {
        playerList.Add(newPlayer);
    }
}
