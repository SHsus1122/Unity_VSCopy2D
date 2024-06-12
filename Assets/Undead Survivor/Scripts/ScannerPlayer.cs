using UnityEngine;

/// <summary>
/// 유저의 스캔(감지)에 사용하는 클래스입니다.
/// </summary>
public class ScannerPlayer : MonoBehaviour
{
    public float scanRange;         // 스캔 범위
    public LayerMask targetLayer;   // 스캔 대상 종류(Layer 사용)
    public Collider2D[] targets;  // 스캔된 모든 대상
    public Transform nearestTarget; // 가장 가까운 스캔 대상

    float cnt = 0;
    float cntInterval = 2f;


    private void Start()
    {
        targets = Physics2D.OverlapCircleAll(transform.position, scanRange, targetLayer);
        nearestTarget = GetNearest();
    }


    private void FixedUpdate()
    {
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

        if (targets.Length == 0)
            return PlayerManager.instance.playerList[0].transform;

        foreach (Collider2D target in targets)
        {
            if (!target.transform.CompareTag("Player"))
                continue;

            if (target.transform.GetComponentInParent<Player>().isPlayerLive == false)
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
