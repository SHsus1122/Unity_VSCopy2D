using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 월드 좌표와 스크린 좌표는 다릅니다. 그래서 아래처럼 코드를 작성해줍니다.
        // WorldToScreenPoint : 월드 상의 오브젝트 위치를 스크린 좌표로 변환합니다.
        rect.position = Camera.main.WorldToScreenPoint(GameManager.Instance.player.transform.position);
    }
}
