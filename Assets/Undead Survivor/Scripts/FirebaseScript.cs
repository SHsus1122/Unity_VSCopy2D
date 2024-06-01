using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System;

public class FirebaseScript : MonoBehaviour
{
    public string DBurl = "https://fireunity-7c1fd-default-rtdb.firebaseio.com/";
    DatabaseReference reference;

    private void Start()
    {
        // DB�� �����͸� �ڵ带 ���ؼ� �ٷ�� ���� DB�� �ش��ϴ� ��ü�� ���۷������ �ϸ� �̸� �������� �ڵ�
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri(DBurl);

        WriteNewUser("1", "player1", "true");
        WriteNewUser("2", "player2", "true");
        //updateUserName("2", "plpl");
        ReadDB();
    }

    private void WriteNewUser(string userId, string username, string isLogging)
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        PlayerInfo plInfo = new PlayerInfo(username, isLogging);
        string json = JsonUtility.ToJson(plInfo);

        reference.Child("users").Child("players").SetRawJsonValueAsync(json);
    }

    private void updateUserName(string userId, string username)
    {
        reference.Child("users").Child(userId).SetValueAsync(username);
    }

    public void ReadDB()
    {
        reference = FirebaseDatabase.DefaultInstance.GetReference("users");
        reference.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot data in snapshot.Children)
                {
                    IDictionary playerData = (IDictionary)data.Value;
                    Debug.Log("name : " + playerData["name"] + ", isLogging : " + playerData["isLogging"]);
                }
            }
        });
    }
}

public class PlayerInfo
{
    public string name = "";
    public string isLogging = "";

    public PlayerInfo(string name, string isLogging)
    {
        this.name = name;
        this.isLogging = isLogging;
    }
}