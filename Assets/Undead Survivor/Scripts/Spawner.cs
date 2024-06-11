using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : MonoBehaviourPun
{
    public Transform[] enemySpawnPoint;
    public SpawnData[] enemySpawnData;
    public float levelTime;
    public LayerMask enemyLayer;

    int level;
    float timer;
    bool startInterval = false;

    void Awake()
    {
        // 마찬가지로 초기화 작업 선행
        enemySpawnPoint = GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray();
    }


    private void Start()
    {
        // 최대 시간에 따라 몬스터 데이터 크기로 나누어 자동으로 구간 시간 계산을 합니다.
        levelTime = GameManager.instance.maxGameTime / enemySpawnData.Length;
        StartCoroutine(StartIntervalCall());
    }


    void Update()
    {
        if (!GameManager.instance.isGameLive)
            return;

        timer += Time.deltaTime;
        // FloorToInt : 소수점 아래는 버리고 int형으로 변환(올림은 CeilToInt)
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / levelTime), enemySpawnData.Length - 1);

        if (timer > enemySpawnData[level].spawnTime && startInterval)
        {
            timer = 0f;
            EnemySpawn();
        }
    }


    IEnumerator StartIntervalCall()
    {
        yield return new WaitForSeconds(5);
        startInterval = true;
    }


    void EnemySpawn()
    {
        if (!PhotonNetwork.IsMasterClient || !GameManager.instance.isGameLive)
            return;

        if (Physics2D.OverlapCircle(enemySpawnPoint[Random.Range(1, enemySpawnPoint.Length)].position, 0.6f, enemyLayer))
        {
            //Debug.Log("[ Spawner ] Spawn Chechk Sphere True");
            return;
        }

        // 0~1 사이의 랜덤 숫자를 이용
        GameObject enemy = GameManager.instance.pool.Get(0);

        // 자식 오브젝트에서만 선택되도록 랜덤 시작은 1로 지정합니다.(Spanwer의 자식으로 포인트가 존재하기에 0번째는 Spanwer입니다)
        enemy.transform.position = enemySpawnPoint[Random.Range(1, enemySpawnPoint.Length)].position;

        enemy.GetComponent<Enemy>().Init(
            enemySpawnData[level].spawnTime,
            enemySpawnData[level].spriteType,
            enemySpawnData[level].health,
            enemySpawnData[level].speed);

        if (!enemy.GetComponent<Enemy>().isLive)
        {
            enemy.GetComponent<Enemy>().isLive = true;
            enemy.GetComponent<Enemy>().spriter.sortingOrder = 2;
            enemy.GetComponent<Collider2D>().enabled = true;
            enemy.GetComponent<Rigidbody2D>().simulated = true;
        }

        //Debug.Log("[ Spawner ] enemy view id : " + enemy.GetPhotonView().ViewID);
        StartCoroutine(enemy.GetComponent<Enemy>().ReActive(0, enemy.GetPhotonView().ViewID));
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
