using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reposition : MonoBehaviour
{
    public Player player;
    Collider2D coll;

    void Awake()
    {
        coll = GetComponent<Collider2D>();
        player = GetComponent<Player>();
    }

    /*void Start()
    {
        // GameManager에서 정적으로 클래스를 올리면 이렇게 변수로 접근이 가능합니다.
        // 이러한 정적 변수는 즉시 클래스에서 부를 수 있기에 편리합니다.
        // 즉, 게임매니저 변수를 만들어서 넣고 하는 과정을 생략 가능합니다.
        // GameManager.Instance.player <- 이런식으로 가져오는것이 가능 !!
    }*/

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area")) 
            return;

        // 플레이어 위치와 타일맵 위치
        Vector3 playerPos = player.transform.position;
        Vector3 myPos = transform.position;

        // 스위치문을 통해서 태그에 따라 처리를 다르게 가져갑니다.
        switch (transform.tag)
        {
            case "Ground":
                // 플레이어 위치 - 타일맵 위치를 계산해서 거리를 구합니다.
                float diffX = playerPos.x - myPos.x;
                float diffY = playerPos.y - myPos.y;
                // 플레이어 방향(Player InputSystem 사용한 경우)
                // 대각선일 때에는 normalize에 의해서 1보다 작은 값이 되는것을 감안해서 작성
                float dirX = diffX < 0 ? -1 : 1;
                float dirY = diffY < 0 ? -1 : 1;
                diffX = Mathf.Abs(diffX);   // Mathf.Abs : 결과값이 - 이더라도 + 로 나옵니다.
                diffY = Mathf.Abs(diffY);

                // Ground 즉, 땅에 대한 이동 코드입니다.
                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * 80);
                }
                else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * dirY * 80);
                }
                break;
            case "Enemy":
                if (coll.enabled)
                {
                    // 몬스터가 너무 플레이어로부터 너무 멀어지면 플레이어로부터 일정 거리 내에 랜덤 재배치
                    Vector3 dist = playerPos - myPos;
                    Vector3 ran = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 0);
                    transform.Translate(ran + dist * 2);
                }
                break;
        }
    }
}
