using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public ItemData data;
    public int level;
    public Weapon weapon;
    public Gear gear;

    Image icon;
    Text textLevel;

    private void Awake()
    {
        // 자식 오브젝트의 컴포넌트가 필요하며 배열의 첫 번째는 자기 자신이기에 순번은 두번째인 1로 지정
        icon = GetComponentsInChildren<Image>()[1];
        icon.sprite = data.itemIcon;

        Text[] texts = GetComponentsInChildren<Text>();
        textLevel = texts[0];
    }

    private void LateUpdate()
    {
        textLevel.text = "Lv." + (level + 1);
    }

    // 사용자가 Button UI 를 통해서 클릭 이벤트로 레벨업을 통해 능력치 활성화 및 강화에 사용할 함수입니다.
    public void OnClick()
    {
        // 능력(아이템)에 타입에 따라 각각 다르게 처리합니다.
        switch (data.itemType)
        {
            // 아래처럼 case문 두 개를 동시에 사용하는 방법도 있습니다.
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                if (level == 0)
                {
                    // level이 0인 즉, 처음 초기값의 실행부입니다. 
                    GameObject newWeapon = new GameObject();    // 무기를 담을 빈 오브젝트 생성
                    weapon = newWeapon.AddComponent<Weapon>();  // 새롭게 컴포넌트를 추가해서 현재 무기에 대입
                    weapon.Init(data);
                }
                else
                {
                    // level이 0이 아닌 즉, 한 번이라도 실행했다면 이후 레벨업 관련할 실행부입니다.
                    float nextDamage = data.baseDamage;
                    int nextCount = 0;

                    // 처음 이후의 레벨업은 데미지와 횟수를 계산해서 적용합니다. 결과는 백분율이기에 곱하기 계산입니다.
                    nextDamage += data.baseDamage * data.damages[level];    // 데미지 관련
                    nextCount += data.counts[level];                        // 횟수와 관통 관련

                    weapon.LevelUp(nextDamage, nextCount);
                }

                // 위의 과정을 거치고 나면 level업 처리를 진행합니다.
                level++;
                break;
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (level == 0)
                {
                    GameObject newGear = new GameObject();
                    gear = newGear.AddComponent<Gear>();
                    gear.Init(data);
                }
                else
                {
                    float nextRate = data.damages[level];
                    gear.LevelUp(nextRate);
                }

                // 위의 과정을 거치고 나면 level업 처리를 진행합니다.
                level++;
                break;
            case ItemData.ItemType.Heal:
                GameManager.Instance.health = GameManager.Instance.maxHealth;
                break;
        }

        // 하지만, 스크립터블 오브젝트를 통해서 생성한 각 아이템들에 설정되어 있는 최대 수치 즉, data.damages.Length 의 길이가
        // level 과 동일한 즉, 레벨업을 한 결과 현재 레벨이 더 이상 상승할 레벨이 없는 경우에 해당하는 경우 레벨업 버튼 자체를 비활성화
        // 함으로 UI 에서도 클릭을 못하게 해 더이상의 레벨업을 막아줍니다.
        if (level == data.damages.Length)
        {
            GetComponent<Button>().interactable = false;
        }
    }
}
