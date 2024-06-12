using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

/// <summary>
/// 게임 준비, 시작까지를 감시하며 다루기 위한 클래스입니다.
/// </summary>
public class GameRoom : MonoBehaviourPunCallbacks
{
    public string playerName;
    public int playerType;
    public bool isReady;

    public PhotonView gameRoomPV;
    public Transform charTransform;
    public GameObject timerUi;

    float timer = 3f;
    float timerInterval = 3f;
    bool isTimerStart = false;

    private void Update()
    {
        if (isTimerStart)
        {
            timer -= Time.deltaTime;
            timerUi.GetComponentInChildren<Text>().text = Mathf.Round(timer).ToString();
        }
    }


    // 유저별로 레디 여부를 Photon의 커스텀 프로퍼티를 활용해 판단해서 게임 시작을 하게 됩니다.
    public void Ready(int id)
    {
        if (playerName.IsNullOrEmpty())
            playerName = PhotonNetwork.LocalPlayer.NickName;

        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;

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
                    trs.GetComponent<Button>().interactable = true;
                }
            }
        }
        playerType = id;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PlayerType", id } });

        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["ReadyCount"] == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            gameRoomPV.RPC("UiStartRPC", RpcTarget.AllBuffered);
            Invoke("StartCall", 3f);
        }
    }


    [PunRPC]
    public void UiStartRPC()
    {
        timerUi.transform.localScale = Vector3.one;
        isTimerStart = true;
    }


    void StartCall()
    {
        gameRoomPV.RPC("RoomGameStartRPC", RpcTarget.AllBuffered);
    }


    [PunRPC]
    void RoomGameStartRPC()
    {
        isTimerStart = false;
        timer = timerInterval;
        timerUi.transform.localScale = Vector3.zero;

        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.LoadLevel(1);
    }
}
