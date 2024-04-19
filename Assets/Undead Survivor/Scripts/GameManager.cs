﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Header 인스펙터의 속성들을 구분시켜주는 타이틀
    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 2 * 10f; // 20초

    [Header("# Player Info")]
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 3, 5, 10, 100, 150, 210, 280, 360, 450, 600 };

    [Header("# Game Object")]
    public PoolManager pool;
    public Player player;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public GameObject enemyCleaner;

    void Awake()
    {
        // 생명 주기에서 인스턴스 변수를 자기 자신으로 초기화 
        Instance = this;
    }

    // ========================================== [ 게임 시작 ]
    public void GameStart()
    {
        health = maxHealth;
        uiLevelUp.Select(0);    // 임시 스크립트(첫 번째 캐릭터 선택)
        Resume();
    }

    // ========================================== [ 게임 종료 ]
    IEnumerator GameOverRoutine()       // 코루틴 활용
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        // UI 활성화 및 패배 UI 표시
        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    // ========================================== [ 게임 승리 ]
    IEnumerator GameVictoryRoutine()    // 코루틴 활용
    {
        isLive = false;
        enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // UI 활성화 및 승리 UI 표시
        uiResult.gameObject.SetActive(true);
        uiResult.Win(); 
        Stop();
    }

    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }

    // ========================================== [ 게임 재시작 ]
    public void GaemRetry()
    {
        // 씬의 이름을 넣거나 순번을 넣을 수 있습니다.
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        if (!isLive)
            return;

        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }
    }
    public void GetExp()
    {
        if (!isLive)
            return;

        exp++;

        // Mathf.Min(level, nextExp.Length - 1) 를 통해서 에러방지(초과) 및 마지막 레벨만 나오게 합니다.
        if (exp == nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            level++;    // 레벨업 적용
            exp = 0;    // 경험치 초기화
            uiLevelUp.Show();
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0; // 유니티의 시간 속도(배율)
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
    }
}
