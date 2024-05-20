using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoom : MonoBehaviourPunCallbacks
{
    public string playerName;
    public int playerType;
    public bool isReady;

    public PhotonView gameRoomPV;

    private void Awake()
    {
        gameRoomPV = GetComponent<PhotonView>();
        playerName = PhotonNetwork.LocalPlayer.NickName;
    }


    public void Ready(int id)
    {
        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        playerType = id;

        if (!isReady)
        {
            customProperties["ReadyCount"] = (int)customProperties["ReadyCount"] + 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            isReady = true;
        }
        else
        {
            customProperties["ReadyCount"] = (int)customProperties["ReadyCount"] - 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            isReady = false;
        }

        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["ReadyCount"] == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("RoomGameStartRPC", RpcTarget.All);
        }
    }


    [PunRPC]
    void RoomGameStartRPC()
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal && !PhotonNetwork.IsMasterClient)
            return;

        GameManager.instance.GameStart(playerType);
    }
}
