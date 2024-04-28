using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

// CreateAssetMenu : 커스텀 메뉴를 생성하는 속성
[System.Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/ItemData")]
public class ItemData : ScriptableObject
{
    // 아이템들의 종류를 담고있는 열거형
    // 근접, 원거리, 장갑, 신발, 힐 포션(음료수)
    public enum ItemType { Melee, Range, Glove, Shoe, Heal }
    
    // 가장 기본적인 데이터 (Id, 이름, 속성 등)
    [Header("# Main Info")]
    public ItemType itemType;
    public int itemId;
    public string itemName;

    [TextArea]  // TextArea : 인스펙터에 텍스트를 여러 줄 넣을 수 있게 해주는 속성을 부여하는 기능입니다.
    public string itemDesc;
    public Sprite itemIcon;

    // 레벨별로 상승하는 능력치들에 대한 데이터
    [Header("# Level Data")]
    public float baseDamage;
    public int baseCount;   // 근접은 갯수, 원거리는 관통 횟수에 대한 변수
    public float[] damages; // 레벨이 오름에 따라 따른 데미지 수치
    public int[] counts;    // 레벨이 오름에 따라 따른 횟수 수치

    // 사격, 회전 등 다양한 아이템들에 대한 데이터
    [Header("# Weapon")]
    public GameObject projectile;   // 투사체 변수
    public Sprite hand;


    /*// 직렬화 메서드
    public static byte[] SerializeItemData(object customObject)
    {
        ItemData itemData = (ItemData)customObject; // ItemData로 캐스팅

        // 아이템 데이터를 바이트 배열로 변환
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // 아이템의 주요 정보를 직렬화
                writer.Write((int)itemData.itemType);
                writer.Write(itemData.itemId);
                writer.Write(itemData.itemName);
                writer.Write(itemData.itemDesc);
                writer.Write(itemData.itemIcon);    //
                writer.Write(itemData.baseDamage);
                writer.Write(itemData.baseCount);
                writer.Write(itemData.projectile);
                writer.Write(itemData.hand);
                // 아이템 아이콘은 별도 처리가 필요할 수 있습니다.
            }
            return stream.ToArray();
        }
    }

    // 역직렬화 메서드
    public static object DeserializeItemData(byte[] serializedCustomObject)
    {
        ItemData itemData = new ItemData();

        // 바이트 배열을 아이템 데이터로 역직렬화
        using (MemoryStream stream = new MemoryStream(serializedCustomObject))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                itemData.itemType = (ItemData.ItemType)reader.ReadInt32();
                itemData.itemId = reader.ReadInt32();
                itemData.itemName = reader.ReadString();
                itemData.itemDesc = reader.ReadString();
                itemData.itemIcon = reader.
                itemData.itemDesc = reader.ReadString();
                itemData.itemDesc = reader.ReadString();
                itemData.itemDesc = reader.ReadString();
                itemData.itemDesc = reader.ReadString();
                // 아이템 아이콘은 별도 처리가 필요할 수 있습니다.
            }
        }
        return itemData;
    }*/
}
