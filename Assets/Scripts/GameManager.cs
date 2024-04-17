using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PoolManager pool;
    public Player player;

    void Awake()
    {
        // 생명 주기에서 인스턴스 변수를 자기 자신으로 초기화 
        Instance = this;
    }
}
