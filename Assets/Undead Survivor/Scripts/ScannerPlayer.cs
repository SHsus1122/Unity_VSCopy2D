using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerPlayer : MonoBehaviour
{
    public float scanRange;         // ��ĵ ����
    public LayerMask targetLayer;   // ��ĵ ��� ����(Layer ���)
    public RaycastHit2D[] targets;  // ��ĵ�� ��� ���(�迭)
    public Transform nearestTarget; // ���� ����� ��ĵ ���

    private void FixedUpdate()
    {
        // CircleCastAll : ������ ĳ��Ʈ�� ��� ��� ����� ��ȯ�ϴ� �Լ�
        //  ���ڰ� �鿡 ���� ���� : ĳ���� ���� ��ġ, ���� ������, ĳ���� ����, ĳ���� ����, ��� ���̾�
        //      Vector2.zero�� ���⼺�� ���ٴ� ���� �ǹ��մϴ�.
        //      0�� ��ĵ�� ���� ��ĵ�� ��� ���� �ƴ϶� ��ĳ�� ����� ��ġ���� ���� �����ؼ� �װ��� ����Ѵٴ� �ǹ��Դϴ�.
        targets = Physics2D.CircleCastAll(transform.position, scanRange, Vector2.zero, 0, targetLayer);
        nearestTarget = GetNearest();
    }

    // ���� ����� �༮�� ã�Ƽ� ��ȯ�ϴ� �Լ�
    Transform GetNearest()
    {
        Transform result = null;
        float diff = 100;

        foreach (RaycastHit2D target in targets)
        {
            if (!target.transform.CompareTag("Player"))
                continue;

            Vector3 myPos = transform.position;                     // ��ĳ�� ����� ��ġ
            Vector3 targetPos = target.transform.position;          // Ÿ���� ��ġ
            float curDiff = Vector3.Distance(myPos, targetPos);     // ���� A�� B�� �Ÿ��� ������ִ� �Լ�

            // �ݺ����� ���� ������ �Ÿ��� ����� �Ÿ����� ������ ��ü
            if (curDiff < diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }

        return result;
    }

}
