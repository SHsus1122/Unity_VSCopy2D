using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 프리펩들을 보관할 변수
    public GameObject[] prefabs;

    // 풀 담당을 하는 리스트
    List<GameObject>[] pools;

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
        GameObject select = null;
        
        // 선택한 Pool의 놀고 있는(활성화 된) 게임오브젝트에 접근
        foreach (GameObject item in pools[index])
        {
            // 만약 존재할 경우 select 변수에 할당합니다.
            if (!item.activeSelf)
            {
                select = item;          // 변수 할당
                select.SetActive(true); // 활성화
                break;
            }
        }

        // 존재하지 않는 즉, 비활성화 된 모두 사용중인 경우에는 새롭게 생성해서 select 변수에 할당합니다.
        if (!select)
        {
            select = Instantiate(prefabs[index], transform);    // transform은 부모로 여기서는 PoolManager의 자식으로 넣는다는 의미
            pools[index].Add(select);   // 오브젝트 풀 리스트에 새롭게 생성된 것을 추가(등록)
        }

        return select;
    }
}
