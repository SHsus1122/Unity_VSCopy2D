using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    public float levelTime;

    int level;
    float timer;

    void Awake()
    {
        // 마찬가지로 초기화 작업 선행
        spawnPoint = GetComponentsInChildren<Transform>();

        // 최대 시간에 따라 몬스터 데이터 크기로 나누어 자동으로 구간 시간 계산을 합니다.
        levelTime = GameManager.Instance.maxGameTime / spawnData.Length;
    }

    void Update()
    {
        if (!GameManager.Instance.isLive)
            return;

        timer += Time.deltaTime;
        // FloorToInt : 소수점 아래는 버리고 int형으로 변환(올림은 CeilToInt)
        level = Mathf.Min(Mathf.FloorToInt(GameManager.Instance.gameTime / 10f), spawnData.Length - 1);

        if (timer > spawnData[level].spawnTime)
        {
            timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {
        // 0~1 사이의 랜덤 숫자를 이용
        GameObject enemy = GameManager.Instance.pool.Get(0);
        // 자식 오브젝트에서만 선택되도록 랜덤 시작은 1로 지정합니다.(Spanwer의 자식으로 포인트가 존재하기에 0번째는 Spanwer입니다)
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        enemy.GetComponent<Enemy>().Init(spawnData[level]);

    }
}

// 이렇게 직렬화를 통해 인스펙터 창에서 내부 클래스도 보여줄 수 있습니다.
[System.Serializable]
public class SpawnData
{
    // 소환시간, 체력, 속도 등
    public float spawnTime;
    public int spriteType;
    public int health;
    public float speed;
}
