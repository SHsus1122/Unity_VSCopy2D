﻿using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun.UtilityScripts;
using static FirebaseScript;
using System.Linq;

// MonoBehaviourPunCallbacks 를 사용하기 위한 선행 using Photon.Pun, Realtime
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public FirebaseScript firebaseScript;

    //public Text StatusText;
    public InputField roomInput, NickNameInput;

    [Header("LobbyPanel")]
    public GameObject uiLogin;
    public GameObject uiLobby;
    public GameObject uiRoom;
    public GameObject gameRoom;
    public List<RoomInfo> rooms = new List<RoomInfo>();
    bool isCheck = false;

    [Header("RoomPanel")]
    public Button roomButtonPrefab;
    public Transform roomContent;
    public PhotonView networkManagerPV;

    [Header("GamePanel")]
    public Transform spawnPoint;


    // 람다식 작성법
    private void Awake() => Screen.SetResolution(1280, 720, false);


    // 일반적인 작성법(작동은 동일합니다)
    private void Update()
    {
        // 현재 연결 상태를 확인하는 코드입니다.(현재 커넥트 상태, 방에 있는지, 로비에 있는지 등...)
        //StatusText.text = PhotonNetwork.NetworkClientState.ToString();
    }

    // 해당 연결 함수의 호출이 성공적으로 완료되면 OnConnectedToMaster함수가 호출됩니다.
    public async void Connect()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("input : " + NickNameInput.text);

        bool isDupNick = await firebaseScript.ReadUserForName(NickNameInput.text);
        if (!isDupNick)
        {
            NicknameSet();
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        print("서버 접속 완료");
        JoinLobby();
    }

    public void NicknameSet()
    {
        PlayerPrefs.SetString("PlayerName", NickNameInput.text.ToString());
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
    }

    // 연결 끊기의 경우에는 OnDisconnected를 콜백함수로 호출합니다.
    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {
        print("연결 끊김");
        uiLobby.SetActive(false);
        uiRoom.SetActive(false);
        uiLogin.SetActive(true);
    }


    // 대형 게임이 아니기에 로비는 하나만 사용하도록 합니다.(여러개도 가능은 합니다)
    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        Debug.Log("[ NetworkManager ] OnJoinedLobby Call, PlayerPrefs Name : " + PlayerPrefs.GetString("PlayerName"));
        print("로비 접속 완료");
        uiLogin.SetActive(false);
        uiLobby.SetActive(true);
        rooms.Clear();
    }

    public void CreateRoom()
    {
        foreach (RoomInfo room in rooms)
        {
            if (room.Name == roomInput.text)
            {
                Debug.Log("이미 존재하는 방이름입니다 !!");
                return;
            }
        }
        CustomCreateRoom();
    }

    void CustomCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "RoomName", "ReadyCount", "PlayerCount" };

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() {
            { "RoomName", roomInput.text },
            { "ReadyCount", 0 },
            { "PlayerCount", 0 }
        };

        PhotonNetwork.CreateRoom(roomInput.text, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        print("방 만들기 완료");
        if (PhotonNetwork.CurrentRoom.Name == "ForCheck")
        {
            return;
        }

        uiLobby.SetActive(false);
        uiRoom.SetActive(true);
    }

    public void JoinRoomBtn(Button button)
    {
        Debug.Log("JoinRoomBtn Call , button Name is : " + (button.name) + ", rooms count : " + (rooms.Count));
        PhotonNetwork.JoinRoom(button.name);
    }

    public void ReJoinRoom(string roomName)
    {
        Debug.Log("[ NetworkManager ] ReJoinRoom Call !!!");

        gameRoom.SetActive(true);
        uiLobby.SetActive(false);
        uiLogin.SetActive(false);
        uiRoom.SetActive(true);
        uiRoom.transform.localScale = Vector3.one;

        networkManagerPV.RPC("UpdateRoomStatus", RpcTarget.All, roomName);

        StartCoroutine(ReSetting());
    }

    public void ReJoinLobby()
    {
        Debug.Log("[ NetworkManager ] ReJoinLobby Call !!!");

        gameRoom.SetActive(true);
        uiRoom.SetActive(false);
        uiLobby.SetActive(true);

        StartCoroutine(ReSetting());
    }

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void LeaveRoom()
    {
        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (PhotonNetwork.IsMasterClient)
        {
            networkManagerPV.RPC("AllLeaveRoomRPC", RpcTarget.Others);

            PhotonNetwork.LeaveRoom();
            PhotonNetwork.JoinLobby();
            ReJoinLobby();
        }
        else
        {
            customProperties["PlayerCount"] = (int)customProperties["PlayerCount"] - 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

            networkManagerPV.RPC("UpdateRoomStatus", RpcTarget.All, PhotonNetwork.CurrentRoom.Name);

            PhotonNetwork.LeaveRoom();
            PhotonNetwork.JoinLobby();
            ReJoinLobby();
        }
    }

    [PunRPC]
    void AllLeaveRoomRPC()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
        ReJoinLobby();
    }

    public override void OnLeftRoom()
    {
        uiRoom.SetActive(false);
        uiLobby.SetActive(true);
        print("방 나가기 완료");
    }

    public IEnumerator ReSetting()
    {
        yield return new WaitForSeconds(0.1f);

        if (!gameRoom.GetPhotonView().IsMine)
            gameRoom.GetPhotonView().TransferOwnership(PhotonNetwork.LocalPlayer);

        if (gameRoom.GetPhotonView().Owner == null)
            yield break;

        if (gameRoom.GetPhotonView().Owner.NickName != PlayerPrefs.GetString("PlayerName"))
        {
            PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("PlayerName");
            gameRoom.GetPhotonView().Owner.NickName = PlayerPrefs.GetString("PlayerName");
        }
        else if (gameRoom.GetPhotonView().Owner.NickName != PlayerPrefs.GetString("PlayerName")) StartCoroutine(ReSetting());
        else yield break;
    }


    public bool CheckDuplicateNickname()
    {
        foreach (Photon.Realtime.Player pl in PhotonNetwork.PlayerList)
        {
            Debug.Log("local name : " + (PhotonNetwork.LocalPlayer.NickName) + ", local ActorNum : " + (PhotonNetwork.LocalPlayer.UserId));
            Debug.Log("pl name : " + (pl.NickName) + ", pl ActorNum : " + (pl.UserId));
            if (PhotonNetwork.LocalPlayer.NickName == pl.NickName && PhotonNetwork.LocalPlayer.UserId != pl.UserId)
            {
                Debug.Log("이미 로그인한 유저이기에 로그인을 종료합니다");
                return true;
            }
        }
        return false;
    }


    IEnumerator NoticeRoutine()
    {
        GameManager.instance.uiNotice.SetActive(true);
        GameManager.instance.uiNotice.transform.GetChild(2).gameObject.SetActive(true);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);   // 알림 효과음 재생

        yield return new WaitForSeconds(3);

        GameManager.instance.uiNotice.transform.GetChild(2).gameObject.SetActive(false);
        GameManager.instance.uiNotice.SetActive(false);
    }


    public override void OnJoinedRoom()
    {
        if (CheckDuplicateNickname())
        {
            Disconnect();
        }
        else
        {
            print("방 참가 완료");
            uiLogin.SetActive(false);
            uiLobby.SetActive(false);
            uiRoom.SetActive(true);
            uiRoom.transform.localScale = Vector3.one;

            ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            customProperties["PlayerCount"] = (int)customProperties["PlayerCount"] + 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

            networkManagerPV.RPC("UpdateRoomStatus", RpcTarget.All, PhotonNetwork.CurrentRoom.Name);
        }
    }


    [PunRPC]
    public void UpdateRoomStatus(string roomName)
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Name == roomName)
        {
            Debug.Log("[ NetworkManager ] UpdateRoomStatus Call, current room name : " + (PhotonNetwork.CurrentRoom.Name) + ", refName : " + (roomName));

            ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            foreach (Transform trs in uiRoom.GetComponentsInChildren<Transform>())
            {
                if (trs.gameObject.name == "RoomStatusText")
                {
                    trs.GetComponent<Text>().text = roomName + " 방 / " + ((int)customProperties["PlayerCount"]) + "명 / " + (PhotonNetwork.CurrentRoom.MaxPlayers) + "최대";
                }
            }
        }

        Debug.Log("[ NetworkManager ] UpdateRoomStatus Call, Player Count : " + PhotonNetwork.CountOfPlayers);
    }


    public override void OnCreateRoomFailed(short returnCode, string message) => print("방 만들기 실패");

    public override void OnJoinRoomFailed(short returnCode, string message) => print("방 참가 실패");

    public override void OnJoinRandomFailed(short returnCode, string message) => print("방 랜덤 참가 실패");


    [PunRPC]
    public void RoomRenewal(List<RoomInfo> roomList)
    {
        // 모든 방 버튼을 제거합니다.
        for (int i = 0; i < roomContent.childCount; i++)
        {
            Destroy(roomContent.GetChild(i).gameObject);
        }

        // 새로운 방 목록으로 방 버튼을 생성합니다.
        Debug.Log("RoomRenewal Call : " + roomList.Count);
        foreach (RoomInfo room in roomList)
        {
            if (room.Name == "ForCheck")
                continue;
            if (room.PlayerCount != 0 && !rooms.Contains(room))
            {
                Button myInstance = Instantiate(roomButtonPrefab, roomContent);
                myInstance.name = room.Name;
                myInstance.GetComponentInChildren<Text>().text = room.Name;
                myInstance.onClick.AddListener(() => JoinRoomBtn(myInstance));
                rooms.Add(room);
            }
            if (room.PlayerCount == 0)
                rooms.Remove(room);
        }
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate Call : " + roomList.Count);
        RoomRenewal(roomList);
    }


    [ContextMenu("정보")]
    void Info()
    {
        if (PhotonNetwork.InRoom)
        {
            // 방에 있는 경우에 해당하는 정보들
            print("현재 방 이름 : " + PhotonNetwork.CurrentRoom.Name);
            print("현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("현재 방 최대인원수 : " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "방에 있는 플레이어 목록 : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
                print(playerStr);
            }
        }
        else
        {
            // 방에 없는 경우에 해당하는 정보들
            print("접속한 인원 수 : " + PhotonNetwork.CountOfPlayers);
            print("방 개수 : " + PhotonNetwork.CountOfRooms);
            print("모든 방에 있는 인원 수 : " + PhotonNetwork.CountOfPlayersInRooms);
            print("로비에 있는가 : " + PhotonNetwork.InLobby);
            print("연결 되었는가 : " + PhotonNetwork.IsConnected);
        }
    }
}
