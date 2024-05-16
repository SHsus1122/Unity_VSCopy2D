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

    private void Update()
    {
        if (!bulletPV.IsMine && gameObject.activeSelf)
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") || per <= -100)
            return;
        Debug.Log("[ Bullet ] OnTriggerEnter2D Target is : " + collision.name);

        per--;

        // 관통력을 상실했을 경우
        if (per < 0)
        {
            rigid.velocity = Vector2.zero;  // 물리 초기화
            gameObject.SetActive(false);    // 풀링 오브젝트 비활성화
            bulletPV.RPC("ObjActiveToggle", RpcTarget.Others, bulletPV.ViewID);
        }
    }


    [PunRPC]
    void ObjActiveToggle(int viewId)
    {
        foreach (GameObject obj in GameManager.instance.pool.pools[1])
        {
            if (obj.GetPhotonView().ViewID == viewId)
            {
                obj.SetActive(false);
                break;
            }
        }
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
            transform.rotation = (Quaternion)stream.ReceiveNext();
            damage = (float)stream.ReceiveNext();
            per = (int)stream.ReceiveNext();
        }
    }
}
