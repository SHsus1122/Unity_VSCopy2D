using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviourPunCallbacks, IPunObservable
{
    public int id;          // 무기 ID
    public int prefabId;    // 프리펩 ID(종류)
    public float damage;    // 데미지
    public int count;       // 개수
    public float speed;     // 속도
    public PhotonView weaponPV;

    float timer;
    Player player;

    void Awake()
    {
        // [기존 방식] 자기자신 말고 부모 오브젝트로부터 가져오는 방법
        //player = GetComponentInParent<Player>();

        // [변경된 방식] 플레이어 초기화에 매개변수가 들어감으로 인해 처음 초기화는 게임 매니저를 활용하는 것으로 변경합니다.
        player = GameManager.Instance.player;
        transform.parent = player.GetComponentsInChildren<Transform>()[0];
    }

    void Update()
    {
        if (!GameManager.Instance.isLive)
            return;

        switch (id)
        {
            case 0:
                // 음수로 설정시 시계방향으로 회전, Vector3.back은 음수입니다.
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                break;
            default:
                timer += Time.deltaTime;

                if (timer > speed)
                {
                    timer = 0f;
                    Fire();
                } 
                break;
        }

        // 강화 시스템 테스트 코드
        if (Input.GetButtonDown("Jump"))
        {
            WeaponLevelUp(10, 1);
        }
    }

    public void WeaponLevelUp(float damage, int count)
    {
        this.damage = damage * Character.Damage;
        this.count += count;

        if (id == 0)
            Batch();

        // 물론 레벨업을 하는 경우에도 레벨업에 대한 Gear데미지를 올려 달라는 의미로 여기서 호출시켜 줍니다.
        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    // 초기화 함수에 만들어둔 스크립트블 오브젝트를 매개변수로 받아서 활용합니다.
    [PunRPC]
    public void Init(ItemData data)
    {
        Debug.Log("[ Weapon ] Init Call");
        // Basic Set
        name = "Weapon " + data.itemId;         // Weapon 이름 설정, 클래스 자체 변수
        transform.parent = player.transform;    // 위에서 만든 Player변수는 Weapon을 소유중인 부모 오브젝트를 지정
        transform.localPosition = Vector3.zero; // 플레이어 안에서 생성되기에 지역 위치에 해당하는 localposition을 사용

        // Property Set
        // 각종 무기 속성들은 스크립트블 오브젝트의 데이터로 초기화 작업(셋팅값을 가져오기 위해서)
        id = data.itemId;
        damage = data.baseDamage * Character.Damage;
        count = data.baseCount + Character.Count;

        // prefabId 를 찾기위한 반복문입니다.
        // 프리펩의 종류만큼 들고와서 순회시킵니다.(즉, 등록된 모든 프리펩을 순회합니다)
        for (int index = 0; index < GameManager.Instance.pool.prefabs.Length; index++)
        {
            if (data.projectile == GameManager.Instance.pool.prefabs[index])
            {
                prefabId = index;
                break;
            }
        }

        switch (id)
        {
            case 0:
                // speed : 회전방향 및 속도
                speed = 150 * Character.WeaponSpeed;
                Batch();
                break;
            default:
                // 발사 속도
                speed = 0.5f * Character.WeaponRate;
                break;
        }

        // Hand Set
        Hand hand = player.hands[(int)data.itemType];   // itemType(열거형)을 이용해서 가져옵니다.
        hand.spriter.sprite = data.hand;
        hand.gameObject.SetActive(true);

        // 특정 함수 호출을 모든 자식에게 방송하는 역할인 BroadcastMessage를 사용합니다.
        // 즉, Player가 가지고 있는 모든 Gear에 한해서 실행시키게 하는게 목적입니다.
        // 이를 하는 이유는 이미 레벨업이 되어있는 상태에서 새롭게 Weapon이 추가되면 Gear가 이미 활성화 되어 있을 경우 이 수치가 적용되지 않습니다.
        // 그래서 초기화가 들어가는 즉, 초기 실행 시점에도 ApplyGear를 실행시키도록 해줍니다.
        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }



    [PunRPC]
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
                Debug.Log(bullet.name + "'s parent is " + bullet.parent.name);
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

            // 근접 무기의 경우 관통에 제한을 주지 않습니다.(무한), -1 is Infinity Per.
            bullet.GetComponent<Bullet>().Init(damage, -100, Vector3.zero);
        }
    }

    [PunRPC]
    void BatchRPC()
    {
        
    }



    void Fire()
    {
        if (!player.scanner.nearestTarget)
            return;

        // 타겟의 위치 및 방향 구하기(즉, 총알이 나아가고자 하는 위치, 방향, 속도 구하기)
        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = targetPos - transform.position;
        dir = dir.normalized;   // 현재 벡터의 방향은 유지하고 크기를 1로 변환(정규화)

        // 총알 생성 및 스폰 위치 지정(플레이어 자신 위치)
        Transform bullet = GameManager.Instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;

        // FromToRotation : 지정된 축을 중심으로 목표를 향해 회전하는 함수
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);   // 축 지정, 위에서 구한 방향 지정
        bullet.GetComponent<Bullet>().Init(damage, count, dir);         // 원하는 값들로 초기화 작업, count가 관통 값

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);          // 발사 효과음 재생
    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    
    }
}
