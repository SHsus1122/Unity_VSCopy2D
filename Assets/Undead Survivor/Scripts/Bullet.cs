using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks, IPunObservable
{
    public float damage;    // 데미지
    public int per;         // 관통력
    public PhotonView bulletPV;

    Rigidbody2D rigid;
    Vector3 curPos;
    Quaternion curRot;

    void Awake()
    {
        bulletPV = photonView;
        rigid = GetComponent<Rigidbody2D>();

        SetParent();
    }

    void SetParent()
    {
        for (int i = 0; i < PlayerManager.instance.playerList.Count; i++)
        {
            if (this.bulletPV.Owner.NickName == PlayerManager.instance.playerList[i].playerPV.Owner.NickName)
            {
                Transform[] list = PlayerManager.instance.playerList[i].gameObject.GetComponentsInChildren<Transform>();
                for (int j = 0; j < list.Length; j++)
                {
                    if (list[j].CompareTag("Weapon"))
                    {
                        Debug.Log("[ Bullet ] prefabId is : " + list[j].GetComponent<Weapon>().id.ToString());
                        Debug.Log("[ Bullet ] this split is : " + this.name.Split(' ')[1]);
                    }

                    if (list[j].CompareTag("Weapon") && list[j].GetComponent<Weapon>().id.ToString() == this.name.Split(' ')[1].Split('(')[0])
                    {
                        Debug.Log("[ Bullet ] Parent name is : " + list[j].name);
                        this.transform.parent = list[j].transform;
                    }
                }
            }
        }
    }

    public void Init(float damage, int per, Vector3 dir, string weaponName)
    {
        this.damage = damage;
        this.per = per;

        // 관통이 안되는 경우 원거리 무기로 설정
        if (per >= 0)
        {
            rigid.velocity = dir * 15f;   // Velocity : 속도
        }
    }

    private void Update()
    {
        if (!bulletPV.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, curRot, Time.deltaTime * 10);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") || per <= -100)
            return;
        Debug.Log("[ Bullet ] OnTriggerEnter2D Target is : " + collision.name);

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
        if (!collision.CompareTag("Area") || per <= -100)
            return;

        gameObject.SetActive(false);
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(damage);
            stream.SendNext(per);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            curRot = (Quaternion)stream.ReceiveNext();
            damage = (float)stream.ReceiveNext();
            per = (int)stream.ReceiveNext();
        }
    }
}
