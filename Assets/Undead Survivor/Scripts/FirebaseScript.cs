using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class FirebaseScript : MonoBehaviour
{
    private DatabaseReference reference = null;

    public GameObject uiNotice;
    public InputField NickNameInput;

    public class UserInfo
    {
        public string name = "";
        public bool isLogging = false;

        public UserInfo(string _name, bool _isLogging)
        {
            this.name = _name;
            this.isLogging = _isLogging;
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["name"] = this.name;
            dic["isLogging"] = this.isLogging;
            return dic;
        }
    }

    private void Start()
    {
        // 파이어베이스의 메인 참조 얻기
        reference = FirebaseDatabase.DefaultInstance.RootReference;

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
                        Debug.Log("Name: " + userInfo["name"] + " / isLogging: " + userInfo["isLogging"]);
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
        reference.Child("users").Child(_name).Child("score").SetValueAsync(userInfo.isLogging);
    }

    public void UpdateUserInfo(string _name, UserInfo userInfo)
    {
        reference.Child("users").Child(_name).UpdateChildrenAsync(userInfo.ToDictionary());
    }

    public void PushUserInfo(UserInfo userInfo)
    {
        string key = reference.Child("users").Push().Key;
        reference.Child("users").Child(key).Child("name").SetValueAsync(userInfo.name);
        reference.Child("users").Child(key).Child("score").SetValueAsync(userInfo.isLogging);
    }


    public async Task<bool> ReadUserForName(string _name)
    {
        bool result = await ReadUserForNameAsync(_name);
        if (result)
        {
            Debug.Log("Nickname exists. Proceeding to NoticeRoutine.");
            StartCoroutine(NoticeRoutine());
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
                Debug.Log("Name: " + userInfo["name"] + " / isLogging: " + userInfo["isLogging"]);
                if (_name == userInfo["name"].ToString())
                {
                    Debug.Log("SAME !!!");
                    return true; // 조건 충족 시 true 반환
                }
            }
        }

        return false; // 조건을 충족하지 않는 경우 false 반환
    }

    IEnumerator NoticeRoutine()
    {
        uiNotice.SetActive(true);
        uiNotice.transform.GetChild(2).gameObject.SetActive(true);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);   // 알림 효과음 재생

        yield return new WaitForSeconds(3);

        uiNotice.transform.GetChild(2).gameObject.SetActive(false);
        uiNotice.SetActive(false);
    }


    public void ReadUserAll(string dataSet)
    {
        // 특정 데이터셋의 DB 참조 얻기
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference(dataSet);

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
                        Debug.Log("Name: " + userInfo["name"] + " / isLogging: " + userInfo["isLogging"]);
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