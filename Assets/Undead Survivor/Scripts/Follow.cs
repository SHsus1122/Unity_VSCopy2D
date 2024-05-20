using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Player player;

    RectTransform rect;
    Vector3 posNick;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }


    void FixedUpdate()
    {
        // 월드 좌표와 스크린 좌표는 다릅니다. 그래서 아래처럼 코드를 작성해줍니다.
        // WorldToScreenPoint : 월드 상의 오브젝트 위치를 스크린 좌표로 변환합니다.
        if (GameManager.instance.isGameLive)
        {
            rect.position = Camera.main.WorldToScreenPoint(player.transform.position);
        }
    }
}
