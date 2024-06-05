using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
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
                transform.parent = list[j].transform;

                if (transform.parent == null)
                {
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


    public async UniTask Init(float damage, int per, Vector3 dir, string owName)
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

        per--;

        if (per < 0)
        {
            rigid.velocity = Vector2.zero;
            gameObject.SetActive(false);

            GameManager.instance.pool.poolPV.RPC("ObjActiveToggle", RpcTarget.Others, 2, bulletPV.ViewID, false);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area") || per <= -100)
            return;

        rigid.velocity = Vector2.zero;
        GameManager.instance.pool.poolPV.RPC("ObjActiveToggle", RpcTarget.AllBuffered, 2, bulletPV.ViewID, false);
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
