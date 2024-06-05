using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviourPun, IPunObservable
{
    public ItemData data;
    public int itemLevel;
    public Weapon weapon;
    public Gear gear;
    public Player player;

    Image icon;
    Text textLevel;
    Text textName;
    Text textDesc;
    int nowLevel;

    private void Awake()
    {
        // 자식 오브젝트의 컴포넌트가 필요하며 배열의 첫 번째는 자기 자신이기에 순번은 두번째인 1로 지정
        //player = GetComponent<Player>();
        icon = GetComponentsInChildren<Image>()[1];
        icon.sprite = data.itemIcon;

        Text[] texts = GetComponentsInChildren<Text>();
        textLevel = texts[0];   // 인스펙터 창의 순서대로 순번을 지정해줍니다.
        //textName = texts[1];
        //textDesc = texts[2];

        //textName.text = data.itemName;
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


    //// 사용자가 Button UI 를 통해서 클릭 이벤트로 레벨업을 통해 능력치 활성화 및 강화에 사용할 함수입니다.
    public void OnClick()
    {
        // _ = 를 통해서 이 이벤트 핸들러 메서드인 OnClick 자체를 비동기적으로 만들지 않고 대신 비동기 작업을 처리하는 메서드를 호출하도록 합니다.
        _ = OnClickCall();
    }

    public async UniTask OnClickCall()
    {
        if (player.Cost < 1)
            return;

        Debug.Log("==== [ Item ] OnClick : " + (data.itemType) + ", player : " + (player.playerPV.Owner.NickName));

        // 능력(아이템)에 타입에 따라 각각 다르게 처리합니다.
        switch (data.itemType)
        {
            // 아래처럼 case문 두 개를 동시에 사용하는 방법도 있습니다.
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                Debug.Log("==== [ Item ] LocalPlayer.ActorNumber : " + (PhotonNetwork.LocalPlayer.ActorNumber) + ", player ActorNum : " + (player.playerPV.Owner.ActorNumber));

                if (itemLevel == 0)
                {
                    // level이 0인 즉, 처음 초기값의 실행부입니다.

                    Debug.Log("==== [ Item ] Lv.0, OnClick : " + data.itemType);
                    weapon = PhotonNetwork.Instantiate("Weapon", transform.position, Quaternion.identity).GetComponent<Weapon>();
                    //weapon.player = player;

                    await weapon.Init(data, weapon, player.playerPV.Owner.NickName);
                }
                else
                {
                    // level이 0이 아닌 즉, 한 번이라도 실행했다면 이후 레벨업 관련할 실행부입니다.
                    Debug.Log("==== [ Item ] Not Lv.0, OnClick : " + data.itemType);
                    float nextDamage = data.baseDamage;
                    int nextCount = 0;

                    // 처음 이후의 레벨업은 데미지와 횟수를 계산해서 적용합니다. 결과는 백분율이기에 곱하기 계산입니다.
                    nextDamage += data.baseDamage * data.damages[itemLevel];    // 데미지 관련
                    nextCount += data.counts[itemLevel];                        // 횟수와 관통 관련

                    await weapon.WeaponLevelUp(nextDamage, nextCount, player.playerPV.Owner.NickName);
                }

                // 위의 과정을 거치고 나면 level업 처리를 진행합니다.
                itemLevel++;
                break;
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (itemLevel == 0)
                {
                    GameObject newGear = new GameObject();
                    gear = newGear.AddComponent<Gear>();
                    gear.Init(data, player);
                }
                else
                {
                    float nextRate = data.damages[itemLevel];
                    gear.GearLevelUp(nextRate);
                }

                // 위의 과정을 거치고 나면 level업 처리를 진행합니다.
                itemLevel++;
                break;
            case ItemData.ItemType.Heal:
                player.health = PlayerManager.instance.maxHealth;
                break;
        }
        ItemInfoUpdate();
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(itemLevel);
        }
        else
        {
            itemLevel = (int)stream.ReceiveNext();
        }
    }
}
