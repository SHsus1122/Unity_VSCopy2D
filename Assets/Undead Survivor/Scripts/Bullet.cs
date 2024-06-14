using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// 총알(탄)에 관련한 설정 및 기능이 담긴 클래스입니다.
/// </summary>
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


    // 모든 유저 화면에서 오브젝트로 문제없이 작동을 위한 부모 설정
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
                    StartCoroutine(ReParentRoutine(owName));
                }
            }
        }
    }


    // 로드 순서의 문제로 부모설정이 안 되었을 경우 재설정을 위한 코루틴
    IEnumerator ReParentRoutine(string owName)
    {
        yield return new WaitForSeconds(0.5f);
        SetParent(owName);
    }


    // UniTask, 초기화 설정은 문제없는 진행을 위해 비동기로 수행합니다.
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


    // 트리거 즉, 충돌 이벤트 처리
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


    // 게임 내 필드를 벗어났을 경우 처리
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
            stream.SendNext(damage);
            stream.SendNext(per);
        }
        else
        {
            damage = (float)stream.ReceiveNext();
            per = (int)stream.ReceiveNext();
        }
    }
}
