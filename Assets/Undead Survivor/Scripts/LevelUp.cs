using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        //player = GetComponent<Player>();
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
        //if (!player.gameObject.GetPhotonView().IsMine)
        //    return;

        Debug.Log("[ LevelUp ] Select index is : " + index);

        items[index].OnClick();
        player.Cost--;
        player.playerPV.RPC("UpdateCost", RpcTarget.AllBuffered, player.Cost);
        InfoUpdate();
    }


    /*void Next()
    {
        // 1. 모든 아이템 비활성화
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        // 2. 그 중에서 랜덤 3개 아이템 활성화
        int[] ran = new int[3];
        while (true)
        {
            ran[0] = Random.Range(0, items.Length);
            ran[1] = Random.Range(0, items.Length);
            ran[2] = Random.Range(0, items.Length);

            // 랜덤 결과가 서로가 같지 않을 때 까지 반복하다가 만족시 반복문 종료
            if (ran[0] != ran[1] && ran[1] != ran[2] && ran[0] != ran[2])
                break;
        }

        for (int index = 0; index < ran.Length; index++)
        {
            Item ranItem = items[ran[index]];

            // 3. 만렙 아이템의 경우는 소비 아이템으로 대체
            if (ranItem.level == ranItem.data.damages.Length)
            {
                items[4].gameObject.SetActive(true);    // 회복포션(음료수)
            }
            else
            {
                ranItem.gameObject.SetActive(true);
            }
        }
    }*/

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
