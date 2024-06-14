using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

/// <summary>
/// Firebase DB를 사용하기 위한 클래스입니다.
/// </summary>
public class FirebaseScript : MonoBehaviour
{
    private DatabaseReference reference = null;

    public GameObject uiNotice;
    public InputField NickNameInput;


    private void Awake()
    {
        NickNameInput.characterLimit = 10;
        NickNameInput.onValueChanged.AddListener(
            (word) => NickNameInput.text = Regex.Replace(word, @"[^0-9a-zA-Z가-힣]", "")
        );
    }


    // 유저정보 설정을 위한 내부 클래스
    public class PlayerInfo
    {
        public string name = "";
        public int kill = 0;
        public bool unlockPotato = false;
        public bool unlockBean = false;

        public PlayerInfo(string _name, int _kill, bool unlockPotato, bool unlockBean)
        {
            this.name = _name;
            this.kill = _kill;
            this.unlockPotato = unlockPotato;
            this.unlockBean = unlockBean;
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["name"] = this.name;
            dic["kill"] = this.kill;
            dic["unlockPotato"] = this.unlockPotato;
            dic["unlockBean"] = this.unlockBean;
            return dic;
        }
    }

    private void Start()
    {
        // 파이어베이스의 메인 참조 얻기
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        //ReadPlayerAll();

        // 추가
        //CreatePlayerWithJson("AA", new PlayerInfo("AA", false));
        //CreatePlayerWithJson("AA", new PlayerInfo("AA", true));
        //CreatePlayerWithPath("kch_path", new PlayerInfo("kch_path", 40));

        // 갱신
        //UpdatePlayerInfo("kch_path", new PlayerInfo("kch_update", 140));

        // 빠르게 데이터 삽입
        //PushPlayerInfo(new PlayerInfo("kch_push", 200));

        // 제거
        //RemovePlayerInfo("kch_push");

        // 읽기
        //ReadPlayerForName("AA");
    }


    // 새로운 유저 생성
    public void CreatePlayerWithJson(string _name, PlayerInfo playerInfo)
    {
        reference.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    foreach (DataSnapshot data in snapshot.Children)
                    {
                        // JSON 자체가 딕셔너리 기반
                        IDictionary playerInfo = (IDictionary)data.Value;
                        if (playerInfo["name"].Equals(_name))
                        {
                            return;
                        }
                    }
                }
            });

        string json = JsonUtility.ToJson(playerInfo);
        reference.Child("players").Child(_name).SetRawJsonValueAsync(json);
    }


    // 유저 이름 변경
    public async UniTask UpdatePlayerName(string _name, string newName)
    {
        DatabaseReference userRef = reference.Child("players").Child(_name);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { _name, newName }
        };

        await userRef.UpdateChildrenAsync(updates);
    }


    // 유저 킬 카운트 변경
    public async UniTask UpdatePlayerKill(string _name, int newKill)
    {
        DatabaseReference userRef = reference.Child("players").Child(_name);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "kill", newKill}
        };

        await userRef.UpdateChildrenAsync(updates);
    }


    // 유저 업적 상태 변경
    public async UniTask UpdatePlayerAchive(string _name, string achiveName, bool newValue)
    {
        DatabaseReference userRef = reference.Child("players").Child(_name);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { achiveName, newValue }
        };

        await userRef.UpdateChildrenAsync(updates);
    }


    // 유저 존재 여부 조회
    public async UniTask<bool> ReadPlayerForName(string _name)
    {
        bool result = await ReadPlayerForNameAsync(_name);
        if (result)
        {
            Debug.Log("Nickname exists. Proceeding to NoticeRoutine.");
        }
        else
        {
            Debug.Log("Nickname does not exist.");
        }
        return result;
    }


    // 유저 업적 정보 조회
    public async UniTask<bool> ReadPlayerForNameAndAchive(string _name, string achiveName)
    {
        if (reference == null)
            reference = FirebaseDatabase.DefaultInstance.RootReference;

        bool result = false;

        try
        {
            DatabaseReference userRef = reference.Child("players").Child(_name).Child(achiveName);
            DataSnapshot snapshot = await userRef.GetValueAsync();

            if (snapshot.Exists)
            {
                return Convert.ToBoolean(snapshot.Value);
            }
            else
            {
                Debug.LogWarning($"해당 {achiveName} 업적은 해당 플레이어 {_name} 에 존재하지 않습니다 !!");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading from Firebase: {e.Message}\n{e.StackTrace}");
        }

        return result;
    }


    // 유저 킬 카운트 조회
    public async UniTask<int> ReadPlayerForNameAndKill(string _name)
    {
        if (reference == null)
            reference = FirebaseDatabase.DefaultInstance.RootReference;

        DatabaseReference userRef = reference.Child("players").Child(_name).Child("kill");
        DataSnapshot snapshot = await userRef.GetValueAsync();

        if (snapshot.Exists)
        {
            return Convert.ToInt32(snapshot.Value);
        }
        else
        {
            Debug.LogWarning($"{_name} 의 킬 카운트를 가져오는데 실패했습니다 !!");
            return 0;
        }
    }


    // 로그인용 유저정보 읽기 및 새로운 유저 생성 함수
    public async UniTask<bool> ReadPlayerForNameAsync(string _name)
    {
        // 특정 데이터셋의 DB 참조 얻기
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("players");

        DataSnapshot snapshot = await dbRef.GetValueAsync();

        if (snapshot.Exists)
        {
            foreach (DataSnapshot data in snapshot.Children)
            {
                // JSON 자체가 딕셔너리 기반, 중복 닉네임은 기존 계정으로 로그인
                IDictionary playerInfo = (IDictionary)data.Value;
                if (_name == playerInfo["name"].ToString())
                {
                    return false;
                }
            }
        }

        // 없는 계정의 경우 새로운 계정 생성
        CreatePlayerWithJson(_name, new PlayerInfo(_name, 0, false, false));
        return false;
    }


    // 모든 유저 정보 읽기
    public void ReadPlayerAll()
    {
        // 특정 데이터셋의 DB 참조 얻기
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("players");

        dbRef.GetValueAsync().ContinueWith(
            task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    foreach (DataSnapshot data in snapshot.Children)
                    {
                        // JSON 자체가 딕셔너리 기반
                        IDictionary playerInfo = (IDictionary)data.Value;
                        Debug.Log("name : " + playerInfo["name"] + " / kill : " + playerInfo["kill"]);
                        break;
                    }
                }
            });
    }


    // 유저 제거
    public void RemovePlayerInfo(string _name)
    {
        reference.Child("players").Child(_name).RemoveValueAsync();
    }
}