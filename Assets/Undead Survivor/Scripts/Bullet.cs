using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;    // 데미지
    public int per;         // 관통력

    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();   
    }

    public void Init(float damage, int per, Vector3 dir)
    {
        this.damage = damage;
        this.per = per;

        // 관통이 안되는 경우 원거리 무기로 설정
        if (per > -1)
        {
            rigid.velocity = dir * 15f;   // Velocity : 속도
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") || per == -1)
            return;

        per--;

        // 관통력을 상실했을 경우
        if (per == -1)
        {
            rigid.velocity = Vector2.zero;  // 물리 초기화
            gameObject.SetActive(false);    // 풀링 오브젝트 비활성화
        }
    }
}
