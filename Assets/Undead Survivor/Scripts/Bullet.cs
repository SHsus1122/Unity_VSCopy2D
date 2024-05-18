using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class Bullet : MonoBehaviourPunCallbacks, IPunObservable
{
    public float damage;    // 데미지
    public int per;         // 관통력
    public PhotonView bulletPV;

    public Vector3 curPos;
    public Quaternion curRot;

    Rigidbody2D rigid;

    void Awake()
    {
        //bulletPV = photonView;
        Debug.Log("[ Bullet ] bulletPV Owner Name : " + bulletPV.Owner.NickName);
        rigid = GetComponent<Rigidbody2D>();

        SetParent(bulletPV.Owner.NickName);
    }

    private void Update()
    {
        if (!bulletPV.IsMine)
        {
            //transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            transform.position = curPos;
        }
    }

    void SetParent(string owName)
    {
        Player player = PlayerManager.instance.FindPlayer(owName);

        Transform[] list = player.gameObject.GetComponentsInChildren<Transform>();
        for (int j = 0; j < list.Length; j++)
        {
            if (list[j].CompareTag("Weapon") && list[j].GetComponent<Weapon>().id.ToString() == this.name.Split(' ')[1].Split('(')[0])
            {
                Debug.Log("[ Bullet ] Parent name is : " + list[j].name);
                transform.parent = list[j].transform;

                if (transform.parent == null)
                {
                    Debug.Log("[ Bullet ] SetParent in Re Call !!");
                    StartCoroutine(ReParent(owName));
                }
            }
        }
    }

    IEnumerator ReParent(string owName)
    {
        Debug.Log("[ Bullet ] ReParent Call !!");
        yield return new WaitForSeconds(0.5f);
        SetParent(owName);
    }

    public void Init(float damage, int per, Vector3 dir, string owName)
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
        if (!collision.CompareTag("Enemy") || per <= -100)
            return;

        Debug.Log("[ Bullet ] OnTriggerEnter2D Target is : " + collision.name);

        per--;

        if (per < 0)
        {
            Debug.LogWarning("[ Bullet ] OnTriggerEnter2D Call, Col name : " + collision.name);
            rigid.velocity = Vector2.zero;
            gameObject.SetActive(false);

            // 비활성화 전 객체가 활성화 상태인지 확인
            bulletPV.RPC("ObjActiveToggle", RpcTarget.Others, bulletPV.ViewID, false);
        }
    }

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (!collision.CompareTag("Area") || per <= -100)
    //        return;

    //    Debug.LogWarning("[ Bullet ] OnTriggerExit2D Call, Col name : " + collision.name);
    //    rigid.velocity = Vector2.zero;
    //    bulletPV.RPC("ObjActiveToggle", RpcTarget.AllBuffered, bulletPV.ViewID, false);
    //}

    [PunRPC]
    public void ObjActiveToggle(int viewId, bool isActive)
    {
        PhotonView targetView = PhotonView.Find(viewId);
        if (targetView != null)
        {
            GameObject targetObject = targetView.gameObject;
            if (targetObject.activeSelf != isActive)
            {
                Debug.Log("[ Bullet ] ObjActiveToggle - Setting active state to: " + isActive);
                targetObject.SetActive(isActive);
            }
        }
        else
        {
            Debug.LogError("[ Bullet ] ObjActiveToggle - PhotonView not found for viewId: " + viewId);
        }
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
            transform.rotation = (Quaternion)stream.ReceiveNext();
            damage = (float)stream.ReceiveNext();
            per = (int)stream.ReceiveNext();
        }
    }
}
