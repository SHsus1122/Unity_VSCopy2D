using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 장갑 관련 클래스
public class Gear : MonoBehaviourPunCallbacks
{
    public ItemData.ItemType type;
    public float rate;


    public void Init(ItemData data)
    {
        // Basic Set
        name = "Gear " + data.itemId;
        transform.parent = GameManager.Instance.player.transform;
        transform.localPosition = Vector3.zero;

        // Property Set
        type = data.itemType;
        rate = data.damages[0];
        ApplyGear();
    }

    // 레벨업 함수
    public void GearLevelUp(float rate)
    {
        this.rate = rate;   // 레벨업 수치 적용
        ApplyGear();        // 레벨업 실제 적용
    }

    // 실제 기능 적용 함수
    void ApplyGear()
    {
        switch (type)
        {
            case ItemData.ItemType.Glove:
                RateUp();
                break;
            case ItemData.ItemType.Shoe:
                SpeedUp();
                break;
        }
    }

    // 장갑의 기능인 근접무기 회전력과 원거리 무기 연사력과 올리는 함수
    void RateUp()
    {
        Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();

        foreach (Weapon weapon in weapons)
        {
            switch (weapon.id)
            {
                // case 0번은 근접무기인 Melee
                case 0:
                    float speed = 150 * Character.WeaponSpeed;
                    weapon.speed = speed + (speed * rate);  // 회전 속도 
                    break;
                default:
                    speed = 0.5f * Character.WeaponRate;
                    weapon.speed = speed * (1f - rate);  // 원거리 무기 발사 속도
                    break;
            }
        }
    }

    // 신발의 기능인 플레이어의 이동속도를 올리는 함수
    void SpeedUp()
    {
        float speed = 3 * Character.Speed;  // 캐릭터 고유속성 함께 적용
        GameManager.Instance.player.speed = speed + (speed * rate);
    }
}
