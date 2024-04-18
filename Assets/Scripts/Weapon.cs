using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;          // 무기 ID
    public int prefabId;    // 프리펩 ID(종류)
    public float damage;    // 데미지
    public int count;       // 개수
    public float speed;     // 속도

    void Start()
    {
        Init();  
    }

    void Update()
    {
        switch (id)
        {
            case 0:
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                break;
            default:
                break;
        }

        // 강화 시스템 테스트 코드
        if (Input.GetButtonDown("Jump"))
        {
            LevelUp(20, 5);
        }
    }

    public void LevelUp(float damage, int count)
    {
        this.damage = damage;
        this.count += count;

        if (id == 0)
            Batch();
    }

    public void Init()
    {
        switch (id)
        {
            case 0:
                speed = 150;   // 음수로 설정시 시계방향으로 회전
                Batch();
                break;
            default:
                break;
        }
    }

    void Batch()
    {
        for (int index = 0; index < count; index++)
        {
            Transform bullet;

            // 현재 부모가 PoolManager 인데 이를 플레이어의 자식인 Weapon 으로 부모를 변경하기 위한 작업입니다.
            // 아래 분기문은 기존 오브젝트가 있다면 이를 재활용하고 모자란 것은 풀링에서 가져온다는 의미입니다.
            if (index < transform.childCount)
            {
                bullet = transform.GetChild(index);
            }
            else
            {
                bullet = GameManager.Instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }

            // 초기 스폰시 위치 지정을 위해서 해주는 작업입니다.
            bullet.localPosition = Vector3.zero;        // 좌표값 초기화
            bullet.localRotation = Quaternion.identity; // 회전값 초기화

            // Weapon의 방향을 설정하기 위한 작업입니다.
            // 갯수에 따라 각도를 설정하기 위해서 초기에 Vector3.forward 로 처음 방향을 잡아주고 해당위치를 기준으로 360을 곱합니다.
            // 이렇게 하면 이제 회전을 위한 360도 각도를 설정했으며 무기의 갯수에 따라 일정한 간격을 주기 위해서 count 로 나누게 됩니다.
            Vector3 rotVec = Vector3.forward * 360 * index / count; 
            bullet.Rotate(rotVec);  // Rotate 로 위에서 계산된 각도를 적용해줍니다.

            // 이후에는 Translate를 이용해서 배치를 하는데 자기 자신의 위쪽 방향을 기준으로(즉, Local 좌표에 해당)위쪽 방향을 고정합니다.
            // 이렇게 해주면 시작 위치는 고정이되 회전하는 각도는 위에서 지정해주었기 때문에 일정하게 배치가 됩니다.
            bullet.Translate(bullet.up * 1.5f, Space.World);

            bullet.GetComponent<Bullet>().Init(damage, -1); // 근접 무기의 경우 관통에 제한을 주지 않습니다.(무한), -1 is Infinity Per.
        }
    }
}
