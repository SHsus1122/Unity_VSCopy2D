using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    // Header 인스펙터의 속성들을 구분시켜주는 타이틀
    [Header("# Game Control")]
    public float gameTime;
    public float maxGameTime = 2 * 10f; // 20초

    [Header("# Player Info")]
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 3, 5, 10, 100, 150, 210, 280, 360, 450, 600 };

    [Header("# Game Object")]
    public PoolManager pool;
    public Player player;

    void Awake()
    {
        // 생명 주기에서 인스턴스 변수를 자기 자신으로 초기화 
        Instance = this;
    }

    void Update()
    {
        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
        }
    }

    public void GetExp()
    {
        exp++;

        if (exp == nextExp[level])
        {
            level++;    // 레벨업 적용
            exp = 0;    // 경험치 초기화
        }
    }
}
