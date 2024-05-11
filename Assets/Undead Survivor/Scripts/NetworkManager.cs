using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Pun.Demo.PunBasics;

// MonoBehaviourPunCallbacks 를 사용하기 위한 선행 using Photon.Pun, Realtime
public class NetworkManager : MonoBehaviourPunCallbacks
{
    //public Text StatusText;
    public InputField roomInput, NickNameInput;

    [Header("LobbyPanel")]
    public GameObject uiLogin;
    public GameObject uiLobby;
    public GameObject uiRoom;
    public GameObject gameRoom;
    public List<RoomInfo> rooms = new List<RoomInfo>();

    [Header("RoomPanel")]
    public Button roomButtonPrefab;
    public Transform roomContent;

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
    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
        print("서버 접속 완료");
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        JoinLobby();
    }

    // 연결 끊기의 경우에는 OnDisconnected를 콜백함수로 호출합니다.
    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause) => print("연결 끊김");



    // 대형 게임이 아니기에 로비는 하나만 사용하도록 합니다.(여러개도 가능은 합니다)
    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        print("로비 접속 완료");
        uiLogin.SetActive(false);
        uiLobby.SetActive(true);
        rooms.Clear();

        gameRoom.SetActive(true);
    }

    void CustomCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "RoomName", "ReadyCount" };
        roomOptions.CustomRoomProperties = new Hashtable() { { "RoomName", roomInput.text } };
        roomOptions.CustomRoomProperties = new Hashtable() { { "ReadyCount", 0 } };

        PhotonNetwork.CreateRoom(roomInput.text, roomOptions);
    }

    public void CreateRoom()
    {
        CustomCreateRoom();

        Button myInstance = Instantiate(roomButtonPrefab, roomContent);
        myInstance.name = roomInput.text;
    }

    public void JoinRoomBtn(Button button)
    {
        Debug.Log("JoinRoomBtn Call , Room Name is : " + button.name);
        PhotonNetwork.JoinRoom(button.name);
    }

    public void JoinRoom() => PhotonNetwork.JoinRoom(roomInput.text);

    public void JoinOrCreateRoom()
    {
        //PhotonNetwork.JoinOrCreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 }, null);
        CustomCreateRoom();
    }
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();


    public override void OnLeftRoom()
    {
        uiRoom.SetActive(false);
        uiLobby.SetActive(true);
        print("방 나가기 완료");
    }

    public override void OnCreatedRoom()
    {
        print("방 만들기 완료");
        uiLobby.SetActive(false);
        uiRoom.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        print("방 참가 완료");
        uiLobby.SetActive(false);
        uiRoom.SetActive(true);
        uiRoom.transform.localScale = Vector3.one;
    }


    public override void OnCreateRoomFailed(short returnCode, string message) => print("방 만들기 실패");

    public override void OnJoinRoomFailed(short returnCode, string message) => print("방 참가 실패");

    public override void OnJoinRandomFailed(short returnCode, string message) => print("방 랜덤 참가 실패");


/*    public void RoomRenewal()
    {
        for (int i = 0; i < roomContent.childCount; i++)
        {
            GameObject.Destroy(roomContent.GetChild(i).gameObject);
        }

        Debug.Log("RoomRenewal Call : " + rooms.Count);
        for (int i = 0; i < rooms.Count; i++)
        {
            Button myInstance = Instantiate(roomButtonPrefab, roomContent);
            myInstance.name = rooms[i].Name;
            myInstance.GetComponentInChildren<Text>().text = rooms[i].Name;
        }
    }*/

    public void RoomRenewal()
    {
        // 모든 방 버튼을 제거합니다.
        for (int i = 0; i < roomContent.childCount; i++)
        {
            Destroy(roomContent.GetChild(i).gameObject);
        }

        // 새로운 방 목록으로 방 버튼을 생성합니다.
        Debug.Log("RoomRenewal Call : " + rooms.Count);
        foreach (RoomInfo room in rooms)
        {
            Button myInstance = Instantiate(roomButtonPrefab, roomContent);
            myInstance.name = room.Name;
            myInstance.GetComponentInChildren<Text>().text = room.Name;
            myInstance.onClick.AddListener(() => JoinRoomBtn(myInstance));
        }
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate Call : " + roomList.Count);
        rooms = roomList;
        RoomRenewal();
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
