using UnityEngine;

public class Scanner : MonoBehaviour
{
    public float scanRange;         // 스캔 범위
    public LayerMask targetLayer;   // 스캔 대상 종류(Layer 사용)
    public Collider2D[] targets;  // 스캔된 모든 대상(배열)
    public Transform nearestTarget; // 가장 가까운 스캔 대상

    float cnt = 0;
    float cntInterval = 0.5f;


    private void Start()
    {
        targets = Physics2D.OverlapCircleAll(transform.position, scanRange, targetLayer);
        nearestTarget = GetNearest();
    }


    private void FixedUpdate()
    {
        // CircleCastAll : 원형의 캐스트를 쏘고 모든 결과를 반환하는 함수
        //  인자값 들에 대한 설명 : 캐스팅 시작 위치, 원의 반지름, 캐스팅 방향, 캐스팅 길이, 대상 레이어
        //      Vector2.zero는 방향성이 없다는 것을 의미합니다.
        //      0은 스캔을 위해 스캔을 쏘는 것이 아니라 스캐너 사용자 위치에서 원을 형성해서 그것을 사용한다는 의미입니다.

        cnt += Time.fixedDeltaTime;

        if (cnt > cntInterval)
        {
            targets = Physics2D.OverlapCircleAll(transform.position, scanRange, targetLayer);
            nearestTarget = GetNearest();
            cnt = 0f;
        }
    }


    // 가장 가까운 녀석을 찾아서 반환하는 함수
    Transform GetNearest()
    {
        Transform result = null;
        float diff = 100;

        foreach (Collider2D target in targets)
        {
            if (!target.transform.CompareTag("Enemy"))
                continue;

            Vector3 myPos = transform.position;                     // 스캐너 사용자 위치
            Vector3 targetPos = target.transform.position;          // 타겟의 위치
            float curDiff = Vector3.Distance(myPos, targetPos);     // 벡터 A와 B의 거리를 계산해주는 함수

            // 반복문을 돌며 가져온 거리가 저장된 거리보다 작으면 교체
            if (curDiff < diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }

        return result;
    }

}
