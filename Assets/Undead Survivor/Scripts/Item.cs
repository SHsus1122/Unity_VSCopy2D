using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviourPunCallbacks, IPunObservable
{
    public ItemData data;
    public int itemLevel;
    public Weapon weapon;
    public Gear gear;
    public PhotonView itemPV;
    public Player player;

    Image icon;
    Text textLevel;
    Text textName;
    Text textDesc;
    int nowLevel;

    private void Awake()
    {
        // 자식 오브젝트의 컴포넌트가 필요하며 배열의 첫 번째는 자기 자신이기에 순번은 두번째인 1로 지정
        player = GetComponent<Player>();
        icon = GetComponentsInChildren<Image>()[1];
        icon.sprite = data.itemIcon;

        Text[] texts = GetComponentsInChildren<Text>();
        for (int i = 0; i < texts.Length; i++)
        {
            Debug.Log("[ Item ] child is : " + texts[i].transform.name);
        }
        textLevel = texts[0];   // 인스펙터 창의 순서대로 순번을 지정해줍니다.
        //textName = texts[1];
        //textDesc = texts[2];

        textName.text = data.itemName;

        itemPV = GetComponent<PhotonView>();
        
    }

    private void OnEnable()
    {
        //ItemInfoUpdate();
    }

    void ItemInfoUpdate()
    {
        nowLevel = itemLevel - 1;

        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                textLevel.text = "Lv." + itemLevel;
                //textDesc.text = string.Format(data.itemDesc, data.damages[nowLevel] * 100, data.counts[nowLevel]);
                break;
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                textLevel.text = "Lv." + itemLevel;
                //textDesc.text = string.Format(data.itemDesc, data.damages[nowLevel] * 100);
                break;
            case ItemData.ItemType.Heal:
                //textDesc.text = string.Format(data.itemDesc);
                break;
        }
    }



    // 사용자가 Button UI 를 통해서 클릭 이벤트로 레벨업을 통해 능력치 활성화 및 강화에 사용할 함수입니다.
    public void OnClick()
    {
        Debug.Log("[ Item ] OnClick Call");
        if (player.Cost < 1) 
            return;

        Debug.Log("==== [ Item ] OnClick : " + data.itemType);

        // 능력(아이템)에 타입에 따라 각각 다르게 처리합니다.
        switch (data.itemType)
        {
            // 아래처럼 case문 두 개를 동시에 사용하는 방법도 있습니다.
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                if (PhotonNetwork.LocalPlayer.ActorNumber != player.playerPV.Owner.ActorNumber)
                    return;

                if (itemLevel == 0)
                {
                    Debug.Log("==== [ Item ] level.Melee, Range 0 으로 첫 시작부");
                    // level이 0인 즉, 처음 초기값의 실행부입니다. 
                    //GameObject newWeapon = new GameObject();    // 무기를 담을 빈 오브젝트 생성
                    //weapon = newWeapon.AddComponent<Weapon>();  // 새롭게 컴포넌트를 추가해서 현재 무기에 대입

                    weapon = PhotonNetwork.Instantiate("Weapon", transform.position, Quaternion.identity).GetComponent<Weapon>();
                    weapon.player = player;

                    Debug.Log("[ Item ] Player Name is : " + player.name);
                    weapon.Init(data, weapon);
                }
                else
                {
                    // level이 0이 아닌 즉, 한 번이라도 실행했다면 이후 레벨업 관련할 실행부입니다.
                    float nextDamage = data.baseDamage;
                    int nextCount = 0;

                    // 처음 이후의 레벨업은 데미지와 횟수를 계산해서 적용합니다. 결과는 백분율이기에 곱하기 계산입니다.
                    Debug.Log("data.baseDamage : " + data.baseDamage + ", data.damages : " + data.damages[itemLevel]);
                    nextDamage += data.baseDamage * data.damages[itemLevel];    // 데미지 관련
                    nextCount += data.counts[itemLevel];                        // 횟수와 관통 관련

                    weapon.WeaponLevelUp(nextDamage, nextCount);
                }

                // 위의 과정을 거치고 나면 level업 처리를 진행합니다.
                itemLevel++;
                break;
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (itemLevel == 0)
                {
                    Debug.Log("==== [ Item ] level.Glove, Shoe 0 으로 첫 시작부");
                    GameObject newGear = new GameObject();
                    gear = newGear.AddComponent<Gear>();
                    gear.Init(data, player);
                }
                else
                {
                    Debug.Log("==== [ Item ] level.Glove, Shoe 레벨업 이후 시작부");
                    float nextRate = data.damages[itemLevel];
                    gear.GearLevelUp(nextRate);
                }

                // 위의 과정을 거치고 나면 level업 처리를 진행합니다.
                itemLevel++;
                break;
            case ItemData.ItemType.Heal:
                Debug.Log("==== [ Item ] level.Heal 호출");
                player.health = PlayerManager.instance.maxHealth;
                break;
        }

        // 하지만, 스크립터블 오브젝트를 통해서 생성한 각 아이템들에 설정되어 있는 최대 수치 즉, data.damages.Length 의 길이가
        // level 과 동일한 즉, 레벨업을 한 결과 현재 레벨이 더 이상 상승할 레벨이 없는 경우에 해당하는 경우 레벨업 버튼 자체를 비활성화
        // 함으로 UI 에서도 클릭을 못하게 해 더이상의 레벨업을 막아줍니다.
/*        if (Itemlevel == data.damages.Length)
        {
            GetComponent<Button>().interactable = false;
        }*/
        ItemInfoUpdate();
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
