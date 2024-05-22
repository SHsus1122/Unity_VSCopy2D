using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRoom : MonoBehaviourPunCallbacks
{
    public string playerName;
    public int playerType;
    public bool isReady;

    public PhotonView gameRoomPV;
    public Transform charTransform;

    private void Awake()
    {
        
        playerName = PhotonNetwork.LocalPlayer.NickName;
    }


    public void Ready(int id)
    {
        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        Debug.Log("[ GameRoom ] customProperties == " + (customProperties == null));
        if (!isReady)
        {
            customProperties["ReadyCount"] = (int)customProperties["ReadyCount"] + 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            isReady = true;

            foreach (Transform trs in charTransform.GetComponentsInChildren<Transform>()) 
            {
                if (trs.name.Contains("Group") || trs.name.Contains(id.ToString())) continue;
                if (!trs.name.Contains("Lock") && trs.name.Contains("Character"))
                {
                    Debug.Log("[ GameRoom ] Button name : " + trs.name);
                    trs.GetComponent<Button>().interactable = false;
                }
            }
        }
        else
        {
            customProperties["ReadyCount"] = (int)customProperties["ReadyCount"] - 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            isReady = false;

            foreach (Transform trs in charTransform.GetComponentsInChildren<Transform>())
            {
                if (trs.name.Contains("Group") || trs.name.Contains(id.ToString())) continue;
                if (!trs.name.Contains("Lock") && trs.name.Contains("Character"))
                {
                    Debug.Log("[ GameRoom ] Button name : " + trs.name);
                    trs.GetComponent<Button>().interactable = true;
                }
            }
        }
        playerType = id;
        PlayerPrefs.SetInt("PlayerType", playerType);

        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["ReadyCount"] == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("RoomGameStartRPC", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void RoomGameStartRPC()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.LoadLevel(1);
    }
}
