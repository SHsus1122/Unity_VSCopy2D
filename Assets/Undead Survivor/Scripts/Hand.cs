using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool isLeft;
    public SpriteRenderer spriter;

    SpriteRenderer player;

    // 오른손의 위치 좌표와 반전시(방향전환) 위치좌표
    Vector3 rightPos = new Vector3(0.35f, -0.15f, 0);
    Vector3 rightPosReverse = new Vector3(-0.15f, -0.15f, 0);

    // 왼손의 회전 각도와 반전시 회전각도(Euler는 기존 회전각도와 상관없이 지정한 각도로 회전값을 설정합니다)
    Quaternion leftRot = Quaternion.Euler(0, 0, -35);
    Quaternion leftRotReverse = Quaternion.Euler(0, 0, -135);

    void Awake()
    {
        // 부모로부터 접근해서 가져와야 합니다.
        // 하지만, 0번째는 자기 자신이므로 1번을 해주어야 가져오고자 하는 Player를 가져올 수 있습니다.
        player = GetComponentsInParent<SpriteRenderer>()[1];
    }

    // 모든 Update가 호출된 뒤 마지막 호출인 LateUpdate를 이용합니다.
    void LateUpdate()
    {
        bool isReverse = player.flipX;

        if (isLeft)
        {
            // 근접무기
            transform.localRotation = isReverse ? leftRotReverse : leftRot;
            spriter.flipY = isReverse;  // 왼손 스프라이트는 Y축 반전
            spriter.sortingOrder = isReverse ? 4 : 6;
        }
        else
        {
            // 원거리무기
            transform.localPosition = isReverse ? rightPosReverse : rightPos;
            spriter.flipX = isReverse;  // 오른손 스프라이트는 X축 반전
            spriter.sortingOrder = isReverse ? 6 : 4;
        }
    }
}
