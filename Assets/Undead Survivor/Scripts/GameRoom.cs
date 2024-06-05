using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class GameRoom : MonoBehaviourPunCallbacks
{
    public string playerName;
    public int playerType;
    public bool isReady;

    public PhotonView gameRoomPV;
    public Transform charTransform;

    public void Ready(int id)
    {
        if (playerName.IsNullOrEmpty())
            playerName = PhotonNetwork.LocalPlayer.NickName;

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
        //PlayerPrefs.SetInt("PlayerType", playerType);
        //PlayerPrefs.Save();

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PlayerType", id } });

        //Debug.Log("[ GameRoom ] PlayerType prefs : " + PlayerPrefs.GetInt("PlayerType", 0));

        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["ReadyCount"] == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Invoke("TestCode", 5f);
        }
    }

    void TestCode()
    {
        gameRoomPV.RPC("RoomGameStartRPC", RpcTarget.MasterClient);
    }

    [PunRPC]
    void RoomGameStartRPC()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.LoadLevel(1);
    }
}
