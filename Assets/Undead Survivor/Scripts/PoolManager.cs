using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // 프리펩들을 보관할 변수
    public GameObject[] prefabs;
    public PhotonView poolManagerPV;

    // 풀 담당을 하는 리스트
    public List<GameObject>[] pools;

    void Awake()
    {
        pools = new List<GameObject>[prefabs.Length];

        // 위에서 생성한 리스트는 초기화 전이기 때문에 아래처럼 반복문을 통해 초기화 작업을 선행합니다.
        for (int index = 0; index < pools.Length; index++)
        {
            // 반복문을 통해 모든 오브젝트 풀 리스트 초기화
            pools[index] = new List<GameObject>();
        }
    }

    // 오브젝트 반환용 함수
    public GameObject Get(int index)
    {
        Debug.Log("[ PoolManager ] Get Call With index : " + index + ", [ PoolManager ] pools size : " + pools[index].Count);
        GameObject select = null;

        // 선택한 Pool의 놀고 있는 게임오브젝트에 접근
        foreach (GameObject item in pools[index])
        {
            // 만약 존재할 경우 select 변수에 할당합니다.
            if (!item.activeSelf)
            {
                Debug.Log("[ PoolManager ] 분기문 첫 번째 !item.activeSelf");
                select = item;          // 변수 할당
                select.SetActive(true); // 활성화
                break;
            }
        }
        
        if (!select)
        {
            Debug.Log("[ PoolManager ] 분기문 두 번째 !item.activeSelf");
            GameObject rpcResult = GetRPC(index);
            if (rpcResult != null)
            {
                select = rpcResult;
            }
        }

        return select;
    }

    [PunRPC]
    public GameObject GetRPC(int index)
    {
        GameObject select = null;

        Debug.Log("[ PoolManager ] GetRPC Call Prefab ID : " + prefabs[index].name);
        // 존재하지 않는 즉, 비활성화 된 모두 사용중인 경우에는 새롭게 생성해서 select 변수에 할당합니다.
        select = PhotonNetwork.Instantiate(prefabs[index].name, transform.position, Quaternion.identity);
        select.transform.parent = transform;

        Debug.Log("[ PoolManager ] GetRPC Call select name is : " + select.transform.name);
        Debug.Log("[ PoolManager ] GetRPC Call select parent : " + select.transform.parent.name);
        //Debug.LogWarning("parent is null : " + (pools[index] == null) + ", select is null : " + (select == null));
        //select = Instantiate(prefabs[index], transform);    // transform은 부모로 여기서는 PoolManager의 자식으로 넣는다는 의미
        pools[index].Add(select);   // 오브젝트 풀 리스트에 새롭게 생성된 것을 추가(등록)

        return select;
    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
