using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine.UI;

/// <summary>
/// Firebase DB를 사용하기 위한 클래스입니다.
/// </summary>
public class FirebaseScript : MonoBehaviour
{
    private DatabaseReference reference = null;

    public GameObject uiNotice;
    public InputField NickNameInput;

    
    // 유저정보 설정을 위한 내부 클래스
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

        //ReadUserAll();

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


    // 새로운 유저 생성
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


    // 유저 이름 변경
    public void UpdateUserName(string _name, UserInfo userInfo)
    {
        reference.Child("users").Child(_name).UpdateChildrenAsync(userInfo.ToDictionary());
    }


    // 유저 정보 가져오기
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


    // 로그인용 유저정보 읽기 및 새로운 유저 생성 함수
    public async Task<bool> ReadUserForNameAsync(string _name)
    {
        // 특정 데이터셋의 DB 참조 얻기
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("users");

        DataSnapshot snapshot = await dbRef.GetValueAsync();

        if (snapshot.Exists)
        {
            foreach (DataSnapshot data in snapshot.Children)
            {
                // JSON 자체가 딕셔너리 기반, 중복 닉네임은 기존 계정으로 로그인
                IDictionary userInfo = (IDictionary)data.Value;
                if (_name == userInfo["name"].ToString())
                {
                    return false;
                } 
            }
        }

        // 없는 계정의 경우 새로운 계정 생성
        CreateUserWithJson(_name, new UserInfo(_name, 0));
        return false;
    }


    // 모든 유저 정보 읽기
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


    // 유저 제거
    public void RemoveUserInfo(string _name)
    {
        reference.Child("users").Child(_name).RemoveValueAsync();
    }



    // ================= 미사용 함수

    //public void UpdateUserLogging(string _name, UserInfo userInfo)
    //{
    //    Debug.Log("UpdateUserLogging Call");
    //    reference.Child("users").Child(_name).UpdateChildrenAsync(userInfo.ToDictionary());
    //}


    //public void PushUserInfo(UserInfo userInfo)
    //{
    //    string key = reference.Child("users").Push().Key;
    //    reference.Child("users").Child(key).Child("name").SetValueAsync(userInfo.name);
    //    reference.Child("users").Child(key).Child("score").SetValueAsync(userInfo.actorNum);
    //}


    //public void CreateUserWithPath(string _name, UserInfo userInfo)
    //{
    //    reference.Child("users").Child(_name).Child("name").SetValueAsync(userInfo.name);
    //    reference.Child("users").Child(_name).Child("isLogging").SetValueAsync(userInfo.actorNum);
    //}
}