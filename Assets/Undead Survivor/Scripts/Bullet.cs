using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks, IPunObservable
{
    public float damage;    // 데미지
    public int per;         // 관통력
    public PhotonView bulletPV;

    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    
    public void Init(float damage, int per, Vector3 dir)
    {
        bulletPV.RPC("InitRPC", RpcTarget.AllBuffered, damage, per, dir);
    }

    [PunRPC]
    public void InitRPC(float damage, int per, Vector3 dir)
    {
        
        this.damage = damage;
        this.per = per;

        // 관통이 안되는 경우 원거리 무기로 설정
        if (per >= 0)
        {
            rigid.velocity = dir * 15f;   // Velocity : 속도
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") || per == -100)
            return;

        CheckTriggerEnter2D();

        bulletPV.RPC("OnTriggerEnter2DRPC", RpcTarget.AllBuffered);
    }

    void CheckTriggerEnter2D()
    {
        per--;

        // 관통력을 상실했을 경우
        if (per < 0)
        {
            rigid.velocity = Vector2.zero;  // 물리 초기화
            gameObject.SetActive(false);    // 풀링 오브젝트 비활성화
        }
    }

    [PunRPC]
    void OnTriggerEnter2DRPC()
    {
        CheckTriggerEnter2D();
    }



    // 총알이 플레이어가 가지고있는 Area영역 밖으로 벗어나면 날아가던 투사체를 비활성화 해줍니다.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area") || per == -100)
            return;

        gameObject.SetActive(false);
    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
