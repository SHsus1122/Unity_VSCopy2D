using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public string username;
    public bool isLogging = false;
    DatabaseReference reference;

    public PlayerData()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public PlayerData(string username, bool isLogging)
    {
        this.username = username;
        this.isLogging = isLogging;
    }
}

