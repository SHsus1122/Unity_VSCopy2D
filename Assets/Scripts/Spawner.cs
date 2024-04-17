using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;

    float timer;

    void Awake()
    {
        // 마찬가지로 초기화 작업 선행
        spawnPoint = GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 0.2f)
        {
            Spawn();
            timer = 0f;
        }
    }

    void Spawn()
    {
        // 0~1 사이의 랜덤 숫자를 이용
        GameObject enemy = GameManager.Instance.pool.Get(Random.Range(0, 2));
        // 자식 오브젝트에서만 선택되도록 랜덤 시작은 1로 지정합니다.(Spanwer의 자식으로 포인트가 존재하기에 0번째는 Spanwer입니다)
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
    }
}
