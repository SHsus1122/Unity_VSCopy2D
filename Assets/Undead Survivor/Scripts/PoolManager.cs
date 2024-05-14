using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviourPun
{
    public static PoolManager instance;

    // 프리펩들을 보관할 변수
    public GameObject[] prefabs;

    // 풀 담당을 하는 리스트
    public List<GameObject>[] pools;

    void Awake()
    {
        instance = GameManager.instance.pool;

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
        //Debug.Log("[ PoolManager ] Get Call With index : " + index + ", [ PoolManager ] pools size : " + pools[index].Count);
        GameObject select = null;

        //Debug.Log("[ PoolManager ] index is null : " + index == null);
        Debug.Log("[ PoolManager ] pools Count : " + pools[index].Count);

        // 선택한 Pool의 놀고 있는 게임오브젝트에 접근
        foreach (GameObject item in pools[index])
        {
            // 만약 존재할 경우 select 변수에 할당합니다.
            if (!item.activeSelf)
            {
                //Debug.Log("[ PoolManager ] 분기문 첫 번째 !item.activeSelf");
                select = item;          // 변수 할당
                select.SetActive(true); // 활성화
                Debug.Log("[ PoolManager ] select name : " + (select.name));
                return select;
            }
        }
        
        //if (select == null)
        //{
        //    Debug.Log("[ PoolManager ] 분기문 두 번째 select == null");
        //    select = PhotonNetwork.Instantiate(prefabs[index].name, transform.position, Quaternion.identity);

        //    Debug.Log("[ PoolManager ] select == null - select owner : " + select.GetPhotonView().Owner.NickName);
        //    pools[index].Add(select);   // 오브젝트 풀 리스트에 새롭게 생성된 것을 추가(등록)
        //}

        return select;
    }
}
