using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

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


    public void Select(int index)
    {
        Debug.Log("[ LevelUp ] Select index is : " + index);

        items[index].OnClick();
        player.Cost--;
        player.playerPV.RPC("UpdateInfoRPC", RpcTarget.All, player.Cost, player.exp, player.level);
        InfoUpdate();
    }


    void InfoUpdate()
    {
        //Debug.Log("[ LevelUp ] Now Cost is : " + player.Cost);
        textCost.text = "Cost\n" + player.Cost;

        // 1. 모든 아이템 비활성화
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
