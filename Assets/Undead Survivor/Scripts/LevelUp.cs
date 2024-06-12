using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유저의 레벨업에 관련한 정보를 관련한 기능을 가진 클래스입니다.
/// </summary>
public class LevelUp : MonoBehaviour
{
    public Player player;
    public Item[] items;

    RectTransform rect;
    Button[] buttons;
    Text textCost;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
        buttons = GetComponentsInChildren<Button>();
        textCost = GetComponentsInChildren<Text>()[0];
    }


    private void Start()
    {
        buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < items.Length; i++)
        {
            buttons[i] = items[i].GetComponent<Button>();
        }
    }


    public void CallLevelUp()
    {
        InfoUpdate();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);    // 레벨업 효과음 재생
        //AudioManager.instance.EffectBgm(true);
    }


    public void Show()
    {
        rect.localScale = Vector3.one;
    }


    public void Hide()
    {
        rect.localScale = Vector3.zero;
    }


    public void SelectCall(int index)
    {
        _ = Select(index);
    }


    public async UniTask Select(int index)
    {
        await items[index].OnClickCall();
        player.Cost--;
        player.playerPV.RPC("UpdateInfoRPC", RpcTarget.All, player.Cost, player.exp, player.level);
        InfoUpdate();
    }


    void InfoUpdate()
    {
        textCost.text = "Cost\n" + player.Cost;

        // 아이템 레벨이 최대치일 경우 버튼 이벤트 비활성화
        foreach (Item item in items)
        {
            if (item.itemLevel == item.data.damages.Length || player.Cost < 1)
            {
                item.GetComponentsInChildren<Button>()[0].interactable = false;
            }
            else
            {
                item.GetComponentsInChildren<Button>()[0].interactable = true;
            }
        }
    }
}
