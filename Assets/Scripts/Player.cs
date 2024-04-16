using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;

    Rigidbody2D rigid;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Input.GetAxis 의 경우 보정이 들어가 있어서 부드럽게 움직임이 멈추게 됩니다.
        // 하지만 GetAxisRaw 의 경우에는 부드럽게 멈추는 것이 아닌 그 자리에 바로 멈추게끔 됩니다.
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");
    }

    // 물리 관련한 처리를 위해서는 FixedUpdate 를 사용
    private void FixedUpdate()
    {
        /*
         * 이동 구현의 세 가지 방법 중 2개
        // 물리력(힘) 작용
        rigid.AddForce(inputVec);

        // 속도 제어
        rigid.velocity = inputVec;
        */

        // normalized 를 통해서 어떠한 방향으로 나아가도 벡터의 크기가 1이 되도록 수정, deltaTime 을 사용해서 프레임에 따른 차이를 방지
        // Time.deltaTime : 물리 프레임 하나가 소비한 시간
        Vector2 nextVec = inputVec.normalized * speed * Time.deltaTime;

        // 위치 이동(World 기준의 위치), 현재 위치를 기준으로 나아갈 방향(nextVec 활용)
        rigid.MovePosition(rigid.position + nextVec);
    }
}
