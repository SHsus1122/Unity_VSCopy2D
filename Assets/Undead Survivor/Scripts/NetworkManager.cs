using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;

/// <summary>
/// Photon을 사용하기 위해서(네트워크 작업) 작성된 클래스입니다.
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    // Firebase 연결 오브젝트
    [Header("FirebasePanel")]
    public FirebaseScript firebaseScript;

    [Header("LobbyPanel")]
    public GameObject uiLogin;
    public GameObject uiLobby;
    public GameObject uiRoom;
    public GameObject gameRoom;
    public InputField roomInput, NickNameInput;
    public List<RoomInfo> rooms = new List<RoomInfo>();

    [Header("RoomPanel")]
    public Button roomButtonPrefab;
    public Transform roomContent;
    public PhotonView networkManagerPV;
    public AchiveManager achiveManager;

    [Header("GamePanel")]
    public Transform spawnPoint;

    Regex NickRegex = new Regex(@"^[0-9a-zA-Z가-힣]{2,10}$");
    Regex RoomRegex = new Regex(@"^[0-9a-zA-Z가-힣]{4,12}$");
    Coroutine runningCoroutine = null;

    // =================== 초기 셋팅
    private void Awake()
    {
        Screen.SetResolution(1280, 720, false);
        PhotonNetwork.AutomaticallySyncScene = true;

        NickNameInput.characterLimit = 10;
        NickNameInput.onValueChanged.AddListener(
            (word) => NickNameInput.text = Regex.Replace(word, @"[^0-9a-zA-Z가-힣]", "")
        );
        roomInput.characterLimit = 12;
        roomInput.onValueChanged.AddListener(
            (word) => roomInput.text = Regex.Replace(word, @"[^0-9a-zA-Z가-힣]", "")
        );
    }


    private void Update()
    {
        // 현재 연결 상태를 확인하는 코드입니다.(현재 커넥트 상태, 방에 있는지, 로비에 있는지 등...)
        //StatusText.text = PhotonNetwork.NetworkClientState.ToString();
    }


    // ========================================== [ 네트워크 연결 ]
    // 해당 연결 함수의 호출이 성공적으로 완료되면 OnConnectedToMaster함수가 호출됩니다.
    public async void Connect()
    {
        if (!NickRegex.IsMatch(NickNameInput.text))
        {
            StartNewCoroutine(NickNameNoticeRoutine());
            return;
        }

        bool isDupNick = await firebaseScript.ReadPlayerForName(NickNameInput.text);
        if (!isDupNick)
        {
            NicknameSet();
            PhotonNetwork.ConnectUsingSettings();
        }
    }


    public void NicknameSet()
    {
        PlayerPrefs.SetString("PlayerName", NickNameInput.text.ToString());
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
    }


    public override void OnConnectedToMaster()
    {
        print("서버 접속 완료");
        JoinLobby();
    }


    // ========================================== [ 네트워크 연결 종료 ]
    // 연결 끊기의 경우에는 OnDisconnected를 콜백함수로 호출합니다.
    public void Disconnect() => PhotonNetwork.Disconnect();


    public override void OnDisconnected(DisconnectCause cause)
    {
        print("연결 끊김");
        uiLobby.SetActive(false);
        uiRoom.SetActive(false);
        uiLogin.SetActive(true);
    }


    // ========================================== [ 로비 연결 ]
    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        print("로비 접속 완료");
        uiLogin.SetActive(false);
        uiLobby.SetActive(true);
        rooms.Clear();
    }


    public void ReJoinLobby()
    {
        gameRoom.SetActive(true);
        uiRoom.SetActive(false);
        uiLobby.SetActive(true);
    }


    // ========================================== [ 방 생성 ]
    public void CreateRoom()
    {
        if (!RoomRegex.IsMatch(roomInput.text))
        {
            StartNewCoroutine(RoomNameNoticeRoutine());
            return;
        }

        foreach (RoomInfo room in rooms)
        {
            if (room.Name == roomInput.text)
            {
                StartNewCoroutine(DuplicateRoomNoticeRoutine());
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
        uiLobby.SetActive(false);
        uiRoom.SetActive(true);
    }


    // ========================================== [ 방 입장 ]
    public void JoinRoomBtn(Button button)
    {
        PhotonNetwork.JoinRoom(button.name);
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

            achiveManager.UnlockCharacter();
            networkManagerPV.RPC("UpdateRoomStatus", RpcTarget.All, PhotonNetwork.CurrentRoom.Name);
        }
    }


    public void ReJoinRoom(string roomName)
    {
        gameRoom.SetActive(true);
        uiLobby.SetActive(false);
        uiLogin.SetActive(false);
        uiRoom.SetActive(true);
        uiRoom.transform.localScale = Vector3.one;

        achiveManager.UnlockCharacter();

        networkManagerPV.RPC("UpdateRoomStatus", RpcTarget.All, roomName);
    }

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();


    // ========================================== [ 방 퇴실 ]
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


    public override void OnLeftRoom()
    {
        uiRoom.SetActive(false);
        uiLobby.SetActive(true);
        print("방 나가기 완료");
    }


    [PunRPC]
    void AllLeaveRoomRPC()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
        ReJoinLobby();
    }


    // ========================================== [ 중복 유저 체크 ]
    public bool CheckDuplicateNickname()
    {
        foreach (Photon.Realtime.Player pl in PhotonNetwork.PlayerList)
        {
            if (PhotonNetwork.LocalPlayer.NickName == pl.NickName && PhotonNetwork.LocalPlayer.UserId != pl.UserId)
            {
                StartNewCoroutine(DuplicatePlayerNoticeRoutine());
                return true;
            }
        }
        return false;
    }


    // ========================================== [ 중복 관련 UI 호출 ]
    IEnumerator DuplicatePlayerNoticeRoutine()
    {
        GameManager.instance.uiNotice.SetActive(true);
        GameManager.instance.uiNotice.transform.GetChild(2).gameObject.SetActive(true);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);   // 알림 효과음 재생

        yield return new WaitForSeconds(3);

        GameManager.instance.uiNotice.transform.GetChild(2).gameObject.SetActive(false);
        GameManager.instance.uiNotice.SetActive(false);
    }


    IEnumerator DuplicateRoomNoticeRoutine()
    {
        GameManager.instance.uiNotice.SetActive(true);
        GameManager.instance.uiNotice.transform.GetChild(3).gameObject.SetActive(true);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);   // 알림 효과음 재생

        yield return new WaitForSeconds(3);

        GameManager.instance.uiNotice.transform.GetChild(3).gameObject.SetActive(false);
        GameManager.instance.uiNotice.SetActive(false);
    }


    IEnumerator NickNameNoticeRoutine()
    {
        GameManager.instance.uiNotice.SetActive(true);
        GameManager.instance.uiNotice.transform.GetChild(4).gameObject.SetActive(true);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);   // 알림 효과음 재생

        yield return new WaitForSeconds(3);

        GameManager.instance.uiNotice.transform.GetChild(4).gameObject.SetActive(false);
        GameManager.instance.uiNotice.SetActive(false);
    }


    IEnumerator RoomNameNoticeRoutine()
    {
        GameManager.instance.uiNotice.SetActive(true);
        GameManager.instance.uiNotice.transform.GetChild(5).gameObject.SetActive(true);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);   // 알림 효과음 재생

        yield return new WaitForSeconds(3);

        GameManager.instance.uiNotice.transform.GetChild(5).gameObject.SetActive(false);
        GameManager.instance.uiNotice.SetActive(false);
    }


    public void StartNewCoroutine(IEnumerator newCoroutine)
    {
        // 기존 코루틴이 실행 중이라면 중지
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
        }

        // 새로운 코루틴을 시작하고 참조를 저장
        runningCoroutine = StartCoroutine(newCoroutine);
    }


    // ========================================== [ 방 List 업데이트 ]
    public void RoomRenewal()
    {
        // 모든 방 버튼을 제거합니다.
        for (int i = roomContent.childCount - 1; i >= 0; i--)
        {
            Destroy(roomContent.GetChild(i).gameObject);
        }

        // 새로운 방 목록으로 방 버튼을 생성합니다.
        foreach (RoomInfo room in rooms)
        {
            if (room.PlayerCount > 0)
            {
                Button myInstance = Instantiate(roomButtonPrefab, roomContent);
                myInstance.name = room.Name;
                myInstance.GetComponentInChildren<Text>().text = room.Name;
                myInstance.onClick.AddListener(() => JoinRoomBtn(myInstance));
            }
        }
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                // 방이 삭제된 경우
                rooms.RemoveAll(r => r.Name == room.Name);
            }
            else
            {
                // 기존 방 업데이트 또는 새로운 방 추가
                int index = rooms.FindIndex(r => r.Name == room.Name);
                if (index != -1)
                {
                    rooms[index] = room;
                }
                else
                {
                    rooms.Add(room);
                }
            }
        }
        RoomRenewal();
    }


    [PunRPC]
    public void UpdateRoomStatus(string roomName)
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Name == roomName)
        {
            ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            foreach (Transform trs in uiRoom.GetComponentsInChildren<Transform>())
            {
                if (trs.gameObject.name == "RoomStatusText")
                {
                    trs.GetComponent<Text>().text = roomName + " 방 / " + ((int)customProperties["PlayerCount"]) + "명 / " + (PhotonNetwork.CurrentRoom.MaxPlayers) + "최대";
                }
            }
        }
    }


    // ========================================== [ 에러 발생 시 로그 출력 ]
    public override void OnCreateRoomFailed(short returnCode, string message) => Debug.Log("방 만들기 실패");

    public override void OnJoinRoomFailed(short returnCode, string message) => Debug.Log("방 참가 실패");

    public override void OnJoinRandomFailed(short returnCode, string message) => Debug.Log("방 랜덤 참가 실패");


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
