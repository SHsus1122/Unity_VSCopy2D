using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class FirebaseScript : MonoBehaviour
{
    private DatabaseReference reference = null;

    public GameObject uiNotice;
    public InputField NickNameInput;

    public class UserInfo
    {
        public string name = "";
        public int actorNum = 0;

        public UserInfo(string _name, int _actorNum)
        {
            this.name = _name;
            this.actorNum = _actorNum;
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["name"] = this.name;
            dic["isLogging"] = this.actorNum;
            return dic;
        }
    }

    private void Start()
    {
        // 파이어베이스의 메인 참조 얻기
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        ReadUserAll();

        // 추가
        //CreateUserWithJson("AA", new UserInfo("AA", false));
        //CreateUserWithJson("AA", new UserInfo("AA", true));
        //CreateUserWithPath("kch_path", new UserInfo("kch_path", 40));

        // 갱신
        //UpdateUserInfo("kch_path", new UserInfo("kch_update", 140));

        // 빠르게 데이터 삽입
        //PushUserInfo(new UserInfo("kch_push", 200));

        // 제거
        //RemoveUserInfo("kch_push");

        // 읽기
        //ReadUserForName("AA");
    }

    public void CreateUserWithJson(string _name, UserInfo userInfo)
    {
        reference.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    foreach (DataSnapshot data in snapshot.Children)
                    {
                        // JSON 자체가 딕셔너리 기반
                        IDictionary userInfo = (IDictionary)data.Value;
                        Debug.Log("name : " + userInfo["name"] + " / actorNum : " + userInfo["actorNum"]);
                        if (userInfo["name"].Equals(_name))
                        {
                            return;
                        }
                    }
                }
            });

        string json = JsonUtility.ToJson(userInfo);
        reference.Child("users").Child(_name).SetRawJsonValueAsync(json);
    }

    public void CreateUserWithPath(string _name, UserInfo userInfo)
    {
        reference.Child("users").Child(_name).Child("name").SetValueAsync(userInfo.name);
        reference.Child("users").Child(_name).Child("isLogging").SetValueAsync(userInfo.actorNum);
    }

    public void UpdateUserName(string _name, UserInfo userInfo)
    {
        reference.Child("users").Child(_name).UpdateChildrenAsync(userInfo.ToDictionary());
    }

    //public void UpdateUserLogging(string _name, UserInfo userInfo)
    //{
    //    Debug.Log("UpdateUserLogging Call");
    //    reference.Child("users").Child(_name).UpdateChildrenAsync(userInfo.ToDictionary());
    //}

    public void PushUserInfo(UserInfo userInfo)
    {
        string key = reference.Child("users").Push().Key;
        reference.Child("users").Child(key).Child("name").SetValueAsync(userInfo.name);
        reference.Child("users").Child(key).Child("score").SetValueAsync(userInfo.actorNum);
    }


    public async Task<bool> ReadUserForName(string _name)
    {
        bool result = await ReadUserForNameAsync(_name);
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


    public async Task<bool> ReadUserForNameAsync(string _name)
    {
        // 특정 데이터셋의 DB 참조 얻기
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("users");

        DataSnapshot snapshot = await dbRef.GetValueAsync();

        if (snapshot.Exists)
        {
            foreach (DataSnapshot data in snapshot.Children)
            {
                // JSON 자체가 딕셔너리 기반
                IDictionary userInfo = (IDictionary)data.Value;
                Debug.Log("_name : " + _name);
                Debug.Log("name : " + userInfo["name"] + " / actorNum : " + userInfo["actorNum"]);
                if (_name == userInfo["name"].ToString())
                {
                    Debug.Log("존재하는 계정으로 로그인합니다");
                    return false;
                } 
            }
        }

        Debug.Log("존재하지 않는 계정이므로 새롭게 생성하고 로그인합니다");
        CreateUserWithJson(_name, new UserInfo(_name, 0));
        return false; // 조건을 충족하지 않는 경우 false 반환
    }


    public void ReadUserAll()
    {
        // 특정 데이터셋의 DB 참조 얻기
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("users");

        dbRef.GetValueAsync().ContinueWith(
            task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    foreach (DataSnapshot data in snapshot.Children)
                    {
                        // JSON 자체가 딕셔너리 기반
                        IDictionary userInfo = (IDictionary)data.Value;
                        Debug.Log("name : " + userInfo["name"] + " / actorNum : " + userInfo["actorNum"]);
                        break;
                    }
                }
            });
    }


    public void RemoveUserInfo(string _name)
    {
        reference.Child("users").Child(_name).RemoveValueAsync();
    }
}